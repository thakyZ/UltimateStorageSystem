using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewModdingAPI.Events;
using HarmonyLib;
using UltimateStorageSystem.Drawing;
using UltimateStorageSystem.Integrations.GenericModConfigMenu;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Overrides;
using UltimateStorageSystem.Integrations.SpaceCore;

namespace UltimateStorageSystem
{
    public class ModEntry : Mod
    {
        /// <summary>Instance of the Mod for static methods.</summary>
        internal static ModEntry Instance { get; private set; } = null!;

        /// <summary>Adjacent tile offsets to the terminal to check.</summary>
        internal static Vector2[] AdjacentTilesOffsets =>
        [
            new Vector2(-1f, 1f),
            new Vector2(0f,  1f),
            new Vector2(1f,  1f),
            new Vector2(-1f, 0f),
            new Vector2(1f,  0f),
            new Vector2(-1f, -1f),
            new Vector2(0f,  -1f),
            new Vector2(1f,  -1f)
        ];

        /// <summary>Farmlink Terminal Data for blacklisting chests from the Farmlink Terminal.</summary>
        internal FarmLinkTerminalData? FarmLinkTerminalData => farmLinkTerminalData;
        private  FarmLinkTerminalData? farmLinkTerminalData;

        /// <summary>The key for the Farmlink Terminal Save Data.</summary>
        private const string FarmLinkTerminalDataKey = "farmlink-terminal-data";

        /// <summary>
        /// The button to open the farmlink terminal wirelessly.
        /// <remarks>
        /// TODO: Maybe make it locked behind an upgrade.
        /// </remarks>
        /// <remarks>Nullable SButton für den Hotkey</remarks>
        /// </summary>
        private SButton? OpenTerminalHotkey => config?.OpenFarmLinkTerminalHotkey ?? SButton.None;

        /// <summary>The threshold for recusive methods before throwing an error.</summary>
        const int RecurseThreshold = 5;

        /// <summary>The custom basket texture.</summary>
        internal static Texture2D BasketTexture => basketTexture ??= Instance.Helper.ModContent.Load<Texture2D>("Assets/basket.png");
        private  static Texture2D? basketTexture;

        /// <summary>The instance of the mod config.</summary>
        private ModConfig config = null!;

        /// <summary>The instance of the mod logger class.</summary>
        private Logger logger = null!;

        /// <summary>The name of the farm link terminal object.</summary>
        private const string farmLinkTerminalName = "holybananapants.UltimateStorageSystemContentPack_FarmLinkTerminal";
        internal static string FarmLinkTerminalName => farmLinkTerminalName; // Required for the patch.

        /// <summary>Determines if the next mouse/controller right click is ignored.</summary>
        internal bool IgnoreNextRightClick { get; set; } = true;

        public ModEntry()
        {
            Instance = this;
        }

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            ModXmlTypeConstructorAttribute.Init(this.ModManifest);
            this.logger = new(this.Monitor);
            this.config  = helper.ReadConfig<ModConfig>();

            // // Initiales Laden der Konfiguration aus der config.json
            // LoadConfig();

            // Laden der Texturen aus dem Assets Ordner
            basketTexture = helper.ModContent.Load<Texture2D>("Assets/basket.png");

            // Patch all harmony patches.
            Harmony harmony = new Harmony("holybananapants.UltimateStorageSystem");
            harmony.PatchAll();

            helper.Events.Input.ButtonPressed     += OnButtonPressed;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Player.Warped           += OnLocationChanged;
            helper.Events.GameLoop.GameLaunched   += OnGameLaunched;
            helper.Events.GameLoop.DayStarted     += OnDayStarted;
            helper.Events.GameLoop.SaveLoaded     += OnSaveLoaded;
            helper.Events.GameLoop.Saving         += OnSaving;
            helper.Events.GameLoop.Saved          += OnSaved;
        }

        /* Unused now. */
        // private void LoadConfig()
        // {
        //    try
        //    {
        //        config = Helper.ReadConfig<ModConfig>();

        //        // Versuche, den Hotkey zu parsen
        //        if (string.IsNullOrWhiteSpace(config.OpenFarmLinkTerminalHotkey) ||
        //            !Enum.TryParse(config.OpenFarmLinkTerminalHotkey.ToUpper(), true, out SButton parsedHotkey))
        //        {
        //            openTerminalHotkey = null;

        //            // Hinweis anzeigen, wenn kein Hotkey gesetzt ist
        //            Monitor.Log("No hotkey is set for opening the FarmLink Terminal. You can set a hotkey in the config.json file located in the mod folder if desired.", LogLevel.Info);
        //        }
        //        else
        //        {
        //            openTerminalHotkey = parsedHotkey;
        //        }
        //    }
        //    catch
        //    {
        //        openTerminalHotkey = null;  // Falls ein Fehler auftritt, setzen Sie den Hotkey auf null
        //    }
        // }

        private void InitFarmLinkTerminalData(int recurseStep = 0)
        {
            if (recurseStep >= RecurseThreshold)
            {
                RecurseMethodException.RecursedTooManySteps(recurseStep, RecurseThreshold);
            }

            if (Context.IsMainPlayer)
            {
                Instance.Helper.Data.WriteSaveData<FarmLinkTerminalData>(FarmLinkTerminalDataKey, new());
                LoadFarmLinkTerminalData();
            }
            else
            {
                Logger.Warn("Tried to Initialize the FarmLink Terminal Data, via a remote client, ignoring.");
            }
        }

        internal void SaveFarmLinkTerminalData(FarmLinkTerminalData? data)
        {
            Instance.Helper.Data.WriteSaveData(FarmLinkTerminalDataKey, data);
        }

        private FarmLinkTerminalData? LoadFarmLinkTerminalDataImpl()
        {
            if (Context.IsMainPlayer)
            {
                return Instance.Helper.Data.ReadSaveData<FarmLinkTerminalData>(FarmLinkTerminalDataKey);
            }
            else
            {
                Logger.Warn("Tried to load the FarmLink Terminal Data, via a remote client, ignoring.");
            }

            return null;
        }

        internal void LoadFarmLinkTerminalData(int recurseStep = 1)
        {
            if (Context.IsMainPlayer)
            {
                farmLinkTerminalData = LoadFarmLinkTerminalDataImpl();
                if (farmLinkTerminalData is null)
                {
                    Logger.Error("Failed to load the FarmLink Terminal Data, will create a new one.");
                    InitFarmLinkTerminalData(recurseStep > 1 ? recurseStep + 1 : 1);
                }
            }
            else
            {
                Logger.Warn("Tried to load the FarmLink Terminal Data, via a remote client, ignoring.");
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            LoadFarmLinkTerminalData();
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            GenericModConfigMenuIntegration.Register(
                manifest: this.ModManifest,
                modRegistry: this.Helper.ModRegistry,
                monitor: this.logger,
                getConfig: () => this.config,
                reset: () => config = new(),
                save: () => this.Helper.WriteConfig(this.config),
                titleScreenOnly: false);
            SpaceCoreIntegration.Init(this.Helper.ModRegistry, this.Monitor);
            SpaceCoreIntegration.RegisterSerializerTypes();
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            /* Unused now. */
            // foreach (var location in Game1.locations)
            // {
            //     CheckForFarmLinkTerminal(location);
            // }
        }

        private void OnSaving(object? sender, SavingEventArgs e)
        {
            /* Unused now. */
            // foreach (var location in Game1.locations)
            // {
            //     ConvertCustomWorkbenchesToStandard(location);
            // }
        }

        private void OnSaved(object? sender, SavedEventArgs e)
        {
            /* Unused now. */
            // foreach (var location in Game1.locations)
            // {
            //     CheckForFarmLinkTerminal(location);
            // }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // Prüfen, ob der Hotkey definiert ist und gedrückt wurde
            if (OpenTerminalHotkey.HasValue && Context.IsPlayerFree && e.Button == OpenTerminalHotkey)
            {
                IgnoreNextRightClick = false;
                if (IsFarmLinkTerminalPlaced())
                {
                    OpenFarmLinkTerminalMenu();
                }
            }

            if (Context.IsPlayerFree && e.Button.IsActionButton())
            {
                Vector2 tile = e.Cursor.Tile;
                if (IsFarmLinkTerminalOnTile(tile, out StardewValley.Object terminalObject))
                {
                    if (FarmLinkTerminal.IsPlayerBelowTileAndFacingUp(Game1.player, terminalObject.TileLocation))
                    {
                        IgnoreNextRightClick = true;
                        OpenFarmLinkTerminalMenu();
                    }
                }
            }
        }

        private void OnLocationChanged(object? sender, WarpedEventArgs e)
        {
            /* Unused now. */
            // CheckForFarmLinkTerminal(e.NewLocation);
        }

        private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            if (e.Removed.Any(obj => obj.Value.Name == farmLinkTerminalName))
            {
                RevertCustomWorkbenches(e.Location);
            }

            /* Unused now. */
            // CheckForFarmLinkTerminal(e.Location);
        }

        private bool IsFarmLinkTerminalPlaced()
        {
            // Prüfe im FarmHouse
            if (Game1.locations.OfType<FarmHouse>().Any(location => location.objects.Values.Any(obj => obj.Name == farmLinkTerminalName)))
            {
                return true;
            }

            // Prüfe auf der Farm
            if (Game1.locations.OfType<Farm>().Any(location => location.objects.Values.Any(obj => obj.Name == farmLinkTerminalName)))
            {
                return true;
            }

            return false;
        }

        /* Unused now. */
        // private void CheckForFarmLinkTerminal(GameLocation location)
        // {
        //     List<KeyValuePair<Vector2, Workbench>> workbenchesToReplace = [];

        //     foreach (var pair in location.objects.Pairs)
        //     {
        //         if (pair.Value is Workbench workbench && pair.Value is not CustomWorkbench)
        //         {
        //             if (IsTerminalAdjacent(pair.Key, location))
        //             {
        //                 workbenchesToReplace.Add(new KeyValuePair<Vector2, Workbench>(pair.Key, workbench));
        //             }
        //         }
        //     }

        //     foreach (var pair in workbenchesToReplace)
        //     {
        //         location.objects.Remove(pair.Key);
        //         location.objects.Add(pair.Key, new CustomWorkbench(pair.Key));
        //     }
        // }

        /* Unused now. */
        // private bool IsTerminalAdjacent(Vector2 tileLocation, GameLocation location)
        // {
        //     foreach (var offset in AdjacentTilesOffsets)
        //     {
        //         Vector2 adjacentTile = tileLocation + offset;
        //         if (location.objects.TryGetValue(adjacentTile, out StardewValley.Object adjacentObject) && adjacentObject.Name == farmLinkTerminalName)
        //         {
        //             return true;
        //         }
        //     }
        //     return false;
        // }

        // Still use this to unpatch ny CustomWorkbenches if required.
        private void RevertCustomWorkbenches(GameLocation location)
        {
            List<Vector2> customWorkbenchesToRevert = [];

            foreach (var pair in location.objects.Pairs)
            {
                if (pair.Value is CustomWorkbench)
                {
                    customWorkbenchesToRevert.Add(pair.Key);
                }
            }

            foreach (var tileLocation in customWorkbenchesToRevert)
            {
                location.objects.Remove(tileLocation);
                location.objects.Add(tileLocation, new Workbench(tileLocation));
            }
        }

        /* Unused now. */
        // private void ConvertCustomWorkbenchesToStandard(GameLocation location)
        // {
        //     List<Vector2> customWorkbenchesToRevert = [];

        //     foreach (var pair in location.objects.Pairs)
        //     {
        //         if (pair.Value is CustomWorkbench)
        //         {
        //             customWorkbenchesToRevert.Add(pair.Key);
        //         }
        //     }

        //     foreach (var tileLocation in customWorkbenchesToRevert)
        //     {
        //         location.objects.Remove(tileLocation);
        //         location.objects.Add(tileLocation, new Workbench(tileLocation));
        //     }
        // }

        private bool IsFarmLinkTerminalOnTile(Vector2 tile, out StardewValley.Object terminalObject)
        {
            return (Game1.currentLocation.objects.TryGetValue(tile, out terminalObject) && terminalObject.Name == farmLinkTerminalName) ||
                   (Game1.currentLocation.objects.TryGetValue(tile + new Vector2(0, 1), out terminalObject) && terminalObject.Name == farmLinkTerminalName);
        }

        private void OpenFarmLinkTerminalMenu()
        {
            var itemTransferManager = new ItemTransferManager(GetAllChests(), new ItemTable(0, 0));
            itemTransferManager.UpdateChestItemsAndSort();
            Game1.activeClickableMenu = new FarmLinkTerminalMenu(GetAllChests());
        }

        private List<Chest> GetAllChests()
        {
            List<Chest> chests = [];

            void AddChestsFromLocation(GameLocation location)
            {
                // Alle Objekte in der Location durchsuchen
                foreach (var item in location.objects.Values)
                {
                    if (item is Chest chest &&
                        (chest.SpecialChestType == Chest.SpecialChestTypes.None || chest.SpecialChestType == Chest.SpecialChestTypes.BigChest))
                    {
                        chests.Add(chest);
                    }
                }

                // Kühlschrank im Farmhaus prüfen
                if (location is FarmHouse farmHouse)
                {
                    Chest fridge =farmHouse.fridge.Value;
                    if (fridge is not null)
                    {
                        chests.Add(fridge);
                    }
                }

                // Gebäude in spezifischen Locations durchsuchen
                if (location is Farm || location.Name == "FarmHouse" || location.Name == "Shed" || location.Name.Contains("Cabin"))
                {
                    if (location is Farm farm)
                    {
                        foreach (var building in farm.buildings)
                        {
                            if (building.indoors.Value is not null)
                            {
                                AddChestsFromLocation(building.indoors.Value);
                            }
                        }
                    }
                }
            }

            // Durchlaufe alle Locations im Spiel
            foreach (var location in Game1.locations)
            {
                AddChestsFromLocation(location);
            }

            return chests;
        }
    }
}

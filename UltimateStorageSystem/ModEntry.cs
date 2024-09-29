using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewModdingAPI.Events;
using UltimateStorageSystem.Drawing;
using UltimateStorageSystem.Integrations.GenericModConfigMenu;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Overrides;
using UltimateStorageSystem.Integrations.SpaceCore;
using UltimateStorageSystem.Network;
using System.Threading;
using System.Text;

namespace UltimateStorageSystem
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class ModEntry : Mod
    {
        /// <summary>Instance of the Mod for static methods.</summary>
        internal static ModEntry Instance { get; private set; } = null!;

        /// <summary>Adjacent tile offsets to the terminal to check.</summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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
        internal FarmLinkTerminalData? FarmLinkTerminalData { get; private set; }

        /// <summary>The key for the Farmlink Terminal Save Data.</summary>
        private const string FarmLinkTerminalDataKey = "farmlink-terminal-data";

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        /// <summary>
        /// The button to open the farmlink terminal wirelessly.
        /// </summary>
        /// <remarks>Nullable SButton für den Hotkey</remarks>
        /// <remarks>
        /// TODO: Maybe make it locked behind an upgrade.
        /// </remarks>
        private SButton? OpenTerminalHotkey => config?.OpenFarmLinkTerminalHotkey ?? SButton.None;

        /// <summary>The threshold for recusive methods before throwing an error.</summary>
        private const int RecurseThreshold = 5;

        /// <summary>The custom basket texture.</summary>
        internal static Texture2D? BasketTexture { get; private set; }

        /// <summary>The blacklist/whitelist filter button texture.</summary>
        internal static Texture2D? FilterButtonTexture { get; private set; }

        /// <summary>The instance of the mod config.</summary>
        private ModConfig config = null!;
        private         bool HostHasUseWhitelist { get; set; }
        internal static bool IsNotUsingWhitelist => Context.IsMainPlayer ? !Instance.config.UseWhiteList : !Instance.HostHasUseWhitelist;
        internal static bool TraceLogging        => Instance.config.TraceLogging;

        /// <summary>The instance of the mod logger class.</summary>
        private Logger logger = null!;

        /// <summary>The name of the farm link terminal object.</summary>
        private static string FarmLinkTerminalName => "holybananapants.UltimateStorageSystemContentPack_FarmLinkTerminal";

        /// <summary>Determines if the next mouse/controller right click is ignored.</summary>
        internal bool IgnoreNextRightClick { get; set; } = true;

        private static readonly CancellationTokenSource asyncRequestCancellationToken = new();

        public ModEntry()
        {
            Instance = this;
        }

        public static string UltimateStorageSystemCommandDocumentation => "";

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            ModXmlTypeConstructorAttribute.Init(this.ModManifest);
            this.logger = new(this.Monitor);

            // Initiales Laden der Konfiguration aus der config.json
            this.config  = helper.ReadConfig<ModConfig>();

            // Laden der Texturen aus dem Assets Ordner
            ModEntry.BasketTexture                = helper.ModContent.Load<Texture2D>("Assets/basket.png");
            ModEntry.FilterButtonTexture = helper.ModContent.Load<Texture2D>("Assets/filter.png");

            // Patch all harmony patches.
            Harmony harmony = new Harmony("holybananapants.UltimateStorageSystem");
            harmony.PatchAll();

            helper.ConsoleCommands.Add(nameof(UltimateStorageSystem).ToSnakeCase(), UltimateStorageSystemCommandDocumentation, this.OnCommand);
            helper.Events.Input.ButtonPressed       += OnButtonPressed;
            helper.Events.World.ObjectListChanged   += ModEntry.OnObjectListChanged;
            helper.Events.Player.Warped             += OnLocationChanged;
            helper.Events.GameLoop.GameLaunched     += OnGameLaunched;
            helper.Events.GameLoop.DayStarted       += OnDayStarted;
            helper.Events.GameLoop.SaveLoaded       += OnSaveLoaded;
            helper.Events.GameLoop.Saving           += OnSaving;
            helper.Events.GameLoop.Saved            += ModEntry.OnSaved;
            helper.Events.GameLoop.ReturnedToTitle  += OnReturnedToTitle;
            helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;
        }

        private void OnPeerConnected(object? sender, PeerConnectedEventArgs e)
        {
            MessageManager.SendConfigUpdate(Game1.player, this.config, e.Peer.PlayerID);
        }

        private void OnCommand(string command, string[] args)
        {
            if (!command.Equals(nameof(UltimateStorageSystem).ToSnakeCase()))
            {
                Logger.Warn($"Name of command is not {nameof(UltimateStorageSystem).ToSnakeCase()} got {command} instead.");
                return;
            }

            switch (args[0].ToLower())
            {
                case "help" or "h":
                    Logger.Info(UltimateStorageSystemCommandDocumentation);
                    break;
                case "find_chests" when Game1.player.currentLocation is { } location && (location is Farm || location.Name is "FarmHouse" or "Shed" || location.Name.Contains("Cabin")):
                {
                    var sb = new StringBuilder();
                    var chests = GetAllChests();
                    for (int i = 0; i < chests.Count; i++)
                    {
                        Chest? chest = chests[i];
                        sb.Append('[')
                          .Append(i)
                          .Append("] ")
                          .Append("Tile Position:")
                          .Append(" { ")
                          .Append(" X = ")
                          .Append(chest.TileLocation.X)
                          .Append(" Y = ")
                          .Append(chest.TileLocation.Y)
                          .Append(" } ")
                          .Append("Location Name:")
                          .Append(' ')
                          .Append('"')
                          .Append(chest.Location.Name)
                          .Append('"')
                          .AppendLine();
                    }
                    Logger.Info(sb.ToString());
                    break;
                }
                default:
                    Logger.Info("Unknown");
                    break;
            }
        }

        /* Unused now. */
        //private void LoadConfig()
        //{
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
        //}

        private void InitFarmLinkTerminalData(int recurseStep = 0)
        {
            if (recurseStep >= RecurseThreshold)
            {
                RecurseMethodException.RecursedTooManySteps(recurseStep, RecurseThreshold);
            }

            if (Context.IsMainPlayer)
            {
                Helper.Data.WriteSaveData(FarmLinkTerminalDataKey, FarmLinkTerminalData);
                LoadFarmLinkTerminalData();
            }
        }

        private void SaveFarmLinkTerminalData(FarmLinkTerminalData? data)
        {
            Helper.Data.WriteSaveData(FarmLinkTerminalDataKey, data);
        }

        private FarmLinkTerminalData? LoadFarmLinkTerminalDataImpl()
        {
            return Context.IsMainPlayer ? Helper.Data.ReadSaveData<FarmLinkTerminalData>(FarmLinkTerminalDataKey) : null;
        }

        private void LoadFarmLinkTerminalData(int recurseStep = 1)
        {
            if (Context.IsMainPlayer)
            {
                FarmLinkTerminalData = LoadFarmLinkTerminalDataImpl();
                if (FarmLinkTerminalData is null)
                {
                    Logger.Error("Failed to load the FarmLink Terminal Data, will create a new one.");
                    InitFarmLinkTerminalData(recurseStep > 1 ? recurseStep + 1 : 1);
                }
            }
            else
            {
                MessageManager.SendDataRequest();
            }
        }

        internal static void UpdateHostConfig(bool? useWhitelist)
        {
            if (useWhitelist is not null)
                Instance.HostHasUseWhitelist = useWhitelist.Value;
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            if (asyncRequestCancellationToken.Token.CanBeCanceled)
            {
                asyncRequestCancellationToken.Cancel();
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
            //foreach (var location in Game1.locations)
            //{
            //    CheckForFarmLinkTerminal(location);
            //}
        }

        private void OnSaving(object? sender, SavingEventArgs e)
        {
            SaveFarmLinkTerminalData(FarmLinkTerminalData);
            /* Unused now. */
            //foreach (var location in Game1.locations)
            //{
            //    ConvertCustomWorkbenchesToStandard(location);
            //}
        }

        private void OnSaved(object? sender, SavedEventArgs e)
        {
            /* Unused now. */
            //foreach (var location in Game1.locations)
            //{
            //    CheckForFarmLinkTerminal(location);
            //}
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
                if (IsFarmLinkTerminalOnTile(tile, out Object? terminalObject) && FarmLinkTerminal.IsPlayerBelowTileAndFacingUp(Game1.player, terminalObject.TileLocation))
                {
                    IgnoreNextRightClick = true;
                    OpenFarmLinkTerminalMenu();
                }
            }
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void OnLocationChanged(object? sender, WarpedEventArgs e)
        {
            /* Unused now. */
            // CheckForFarmLinkTerminal(e.NewLocation);
        }

        private static void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            if (e.Removed.Any(obj => obj.Value.Name == ModEntry.FarmLinkTerminalName))
            {
                ModEntry.RevertCustomWorkbenches(e.Location);
            }

            /* Unused now. */
            //CheckForFarmLinkTerminal(e.Location);
        }

        internal static bool IsFarmLinkTerminalPlaced()
        {
            // Prüfe im FarmHouse
            if (Game1.locations.OfType<FarmHouse>().Any(location => location.objects.Values.Any(obj => obj.Name == ModEntry.FarmLinkTerminalName)))
                return true;

            // Prüfe auf der Farm
            if (Game1.locations.OfType<Farm>().Any(location => location.objects.Values.Any(obj => obj.Name == ModEntry.FarmLinkTerminalName)))
                return true;

            return false;
        }

        /* Unused now. */
        //private void CheckForFarmLinkTerminal(GameLocation location)
        //{
        //    List<KeyValuePair<Vector2, Workbench>> workbenchesToReplace = [];

        //    foreach (var pair in location.objects.Pairs)
        //    {
        //        if (pair.Value is Workbench workbench && pair.Value is not CustomWorkbench && IsTerminalAdjacent(pair.Key, location))
        //        {
        //            workbenchesToReplace.Add(new KeyValuePair<Vector2, Workbench>(pair.Key, workbench));
        //        }
        //    }

        //    foreach (var pair in workbenchesToReplace)
        //    {
        //        location.objects.Remove(pair.Key);
        //        location.objects.Add(pair.Key, new CustomWorkbench(pair.Key));
        //    }
        //}

        internal static bool IsTerminalAdjacent(Vector2 tileLocation, GameLocation location)
        {
            foreach (var offset in AdjacentTilesOffsets)
            {
                if (location.objects.TryGetValue(tileLocation + offset, out Object? adjacentObject) && adjacentObject.Name == ModEntry.FarmLinkTerminalName)
                {
                    return true;
                }
            }
            return false;
        }

        // Still use this to unpatch ny CustomWorkbenches if required.
        private static void RevertCustomWorkbenches(GameLocation location)
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
        //private void ConvertCustomWorkbenchesToStandard(GameLocation location)
        //{
        //    List<Vector2> customWorkbenchesToRevert = [];

        //    foreach (var pair in location.objects.Pairs)
        //    {
        //        if (pair.Value is CustomWorkbench)
        //        {
        //            customWorkbenchesToRevert.Add(pair.Key);
        //        }
        //    }

        //    foreach (var tileLocation in customWorkbenchesToRevert)
        //    {
        //        location.objects.Remove(tileLocation);
        //        location.objects.Add(tileLocation, new Workbench(tileLocation));
        //    }
        //}

        // ReSharper disable once MemberCanBeMadeStatic.Local
        /// <summary>
        /// Chests if the <see cref="FarmLinkTerminal"/> in on a tile.
        /// </summary>
        /// <param name="tile">The tile to check against.</param>
        /// <param name="terminalObject">The returned <see cref="FarmLinkTerminal"/> as an <inheritdoc cref="StardewValley.Object"/>.</param>
        /// <returns>Returns true if the tile contains a <see cref="FarmLinkTerminal"/> otherwise false.</returns>
        private bool IsFarmLinkTerminalOnTile(Vector2 tile, [NotNullWhen(true)] out Object? terminalObject)
        {
            return (Game1.currentLocation.objects.TryGetValue(tile,                     out terminalObject) && terminalObject.Name == ModEntry.FarmLinkTerminalName) ||
                   (Game1.currentLocation.objects.TryGetValue(tile + new Vector2(0, 1), out terminalObject) && terminalObject.Name == ModEntry.FarmLinkTerminalName);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        /// <summary>
        /// A method to open the <see cref="FarmLinkTerminalMenu"/>.
        /// </summary>
        private void OpenFarmLinkTerminalMenu()
        {
            var itemTransferManager = new ItemTransferManager(GetAllChests(), new ItemTable(0, 0));
            itemTransferManager.UpdateChestItemsAndSort();
            Game1.activeClickableMenu = new FarmLinkTerminalMenu(GetAllChests());
        }

        /// <summary>
        /// Adds chests to a list from a specified <see cref="GameLocation"/>.
        /// </summary>
        /// <param name="location">The <see cref="GameLocation"/> to check for player chests at.</param>
        private static IEnumerable<Chest> AddChestsFromLocation(GameLocation location)
        {
            // Alle Objekte in der Location durchsuchen
            List<Chest> output = [..location.objects.Values.OfType<Chest>().Where(chest => chest.SpecialChestType is Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.BigChest)];

            // Kühlschrank im Farmhaus prüfen
            if (location is FarmHouse farmHouse && farmHouse.fridge.Value is Chest fridge && farmHouse.upgradeLevel > 0)
            {
                output.Add(fridge);
            }

            // Gebäude in spezifischen Locations durchsuchen
            if (location is Farm || location.Name == "FarmHouse" || location.Name == "Shed" || location.Name.Contains("Cabin"))
            {

                if (location is Farm farm)
                {
                    Instance.Monitor.Log($"location is typeof of farm and the name of the location is \"{location.Name}\".");
                    foreach (var building in farm.buildings)
                    {
                        if (building.indoors.Value is not null)
                        {
                            output.AddRange(AddChestsFromLocation(building.indoors.Value));
                        }
                    }
                }
                else
                {
                    Instance.Monitor.Log($"location is not typeof of farm and the name of the location is \"{location.Name}\".");
                }
            }

            return output;
        }

        /// <summary>
        /// Get all chests and filter every single chest based on their filter stats and if the host's config is using whitelist or not.
        /// </summary>
        /// <returns>List of chests in the various <see cref="GameLocation"/>s.</returns>
        internal static List<Chest> GetAllChests()
        {
            // Durchlaufe alle Locations im Spiel
            return [..Game1.locations.SelectMany(AddChestsFromLocation).Where(chest => chest.IsFiltered() ^ IsNotUsingWhitelist)];
        }

        /// <inheritdoc cref="Game1.playSound(string, int?)"/>
        [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        internal static bool TryPlaySound(string cueName, int? pitch = null)
        {
            try
            {
                Game1.playSound(cueName, pitch);
                return true;
            }
            catch { /* Do nothing... */ }
            return false;
        }

        internal static bool IsMainFarmer(Farmer? farmer)
        {
            return farmer is not null && IsMainFarmer(farmer.UniqueMultiplayerID);
        }

        internal static bool IsMainFarmer(long farmerID)
        {
            return GetMainFarmer() is Farmer farmer && farmer.UniqueMultiplayerID == farmerID;
        }

        internal static Farmer? GetMainFarmer()
        {
            if (!Context.HasRemotePlayers || Context.IsMainPlayer)
            {
                return Game1.player;
            }

            return Game1.getAllFarmers().FirstOrDefault(x => x.IsMainPlayer);
        }

        internal static Farmer? GetFarmer(long farmerID)
        {
            return Game1.getAllFarmers().FirstOrDefault(x => x.UniqueMultiplayerID == farmerID);
        }
    }
}

using StardewModdingAPI;
using StardewValley.Locations;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System.Collections.Generic;
using UltimateStorageSystem.Drawing;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Overrides;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;

#nullable disable

namespace UltimateStorageSystem
{
    [XmlInclude(typeof(CustomWorkbench))]
    public class ModConfig
    {
        public string OpenFarmLinkTerminalHotkey { get; set; } = "";  // Standardwert ist ein leerer String
    }

    public class ModEntry : Mod
    {
        public static ModEntry Instance;
        public static Texture2D basketTexture;
        private ModConfig config;
        private SButton? openTerminalHotkey;  // Nullable SButton für den Hotkey
        private readonly string farmLinkTerminalName = "holybananapants.UltimateStorageSystemContentPack_FarmLinkTerminal";
        private bool justOpenedMenu = false;

        public override void Entry(IModHelper helper)
        {
            Instance = this;

            // Initiales Laden der Konfiguration aus der config.json
            LoadConfig();

            // Laden der Texturen aus dem Assets Ordner            
            basketTexture = helper.ModContent.Load<Texture2D>("Assets/basket.png");

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Player.Warped += OnLocationChanged;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.Saved += OnSaved;            
        }

        private void LoadConfig()
        {
            try
            {
                config = Helper.ReadConfig<ModConfig>();

                // Versuche, den Hotkey zu parsen
                if (string.IsNullOrWhiteSpace(config.OpenFarmLinkTerminalHotkey) ||
                    !Enum.TryParse(config.OpenFarmLinkTerminalHotkey.ToUpper(), true, out SButton parsedHotkey))
                {
                    openTerminalHotkey = null;

                    // Hinweis anzeigen, wenn kein Hotkey gesetzt ist
                    Monitor.Log("No hotkey is set for opening the FarmLink Terminal. You can set a hotkey in the config.json file located in the mod folder if desired.", LogLevel.Info);
                }
                else
                {
                    openTerminalHotkey = parsedHotkey;
                }
            }
            catch
            {
                openTerminalHotkey = null;  // Falls ein Fehler auftritt, setzen Sie den Hotkey auf null
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            foreach (var location in Game1.locations)
            {
                CheckForFarmLinkTerminal(location);
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            foreach (var location in Game1.locations)
            {
                ConvertCustomWorkbenchesToStandard(location);
            }
        }

        private void OnSaved(object sender, SavedEventArgs e)
        {
            foreach (var location in Game1.locations)
            {
                CheckForFarmLinkTerminal(location);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (justOpenedMenu)
            {
                justOpenedMenu = false;
                return;
            }

            // Prüfen, ob der Hotkey definiert ist und gedrückt wurde
            if (openTerminalHotkey.HasValue && Context.IsPlayerFree && e.Button == openTerminalHotkey)
            {
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
                        OpenFarmLinkTerminalMenu();
                        justOpenedMenu = true;
                    }
                }
            }
        }

        private void OnLocationChanged(object sender, WarpedEventArgs e)
        {
            CheckForFarmLinkTerminal(e.NewLocation);
        }

        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (e.Removed.Any(obj => obj.Value.Name == farmLinkTerminalName))
            {
                RevertCustomWorkbenches(e.Location);
            }

            CheckForFarmLinkTerminal(e.Location);
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

        private void CheckForFarmLinkTerminal(GameLocation location)
        {
            List<KeyValuePair<Vector2, Workbench>> workbenchesToReplace = new List<KeyValuePair<Vector2, Workbench>>();

            foreach (var pair in location.objects.Pairs)
            {
                if (pair.Value is Workbench workbench && !(pair.Value is CustomWorkbench))
                {
                    if (IsTerminalAdjacent(pair.Key, location))
                    {
                        workbenchesToReplace.Add(new KeyValuePair<Vector2, Workbench>(pair.Key, workbench));
                    }
                }
            }

            foreach (var pair in workbenchesToReplace)
            {
                location.objects.Remove(pair.Key);
                location.objects.Add(pair.Key, new CustomWorkbench(pair.Key));
            }
        }

        private bool IsTerminalAdjacent(Vector2 tileLocation, GameLocation location)
        {
            foreach (var offset in AdjacentTilesOffsets)
            {
                Vector2 adjacentTile = tileLocation + offset;
                if (location.objects.TryGetValue(adjacentTile, out StardewValley.Object adjacentObject) && adjacentObject.Name == farmLinkTerminalName)
                {
                    return true;
                }
            }
            return false;
        }

        private void RevertCustomWorkbenches(GameLocation location)
        {
            List<Vector2> customWorkbenchesToRevert = new List<Vector2>();

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

        private void ConvertCustomWorkbenchesToStandard(GameLocation location)
        {
            List<Vector2> customWorkbenchesToRevert = new List<Vector2>();

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
            List<Chest> chests = new List<Chest>();

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
                if (location is FarmHouse)
                {
                    Chest fridge = (location as FarmHouse).fridge.Value;
                    if (fridge != null)
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
                            if (building.indoors.Value != null)
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

        private static readonly Vector2[] AdjacentTilesOffsets = new Vector2[]
        {
            new Vector2(-1f, 1f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-1f, -1f),
            new Vector2(0f, -1f),
            new Vector2(1f, -1f)
        };
    }
}

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
using System.Xml.Serialization;

#nullable disable

namespace UltimateStorageSystem
{
    [XmlInclude(typeof(CustomWorkbench))]
    public class ModEntry : Mod
    {
        public static ModEntry Instance;
        private readonly string farmLinkTerminalName = "holybananapants.UltimateStorageSystemContentPack_FarmLinkTerminal";
        private bool justOpenedMenu = false;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Player.Warped += OnLocationChanged;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;  // Event handler for game saving
            helper.Events.GameLoop.Saved += OnSaved;    // Event handler for after game saved
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

            if (Context.IsPlayerFree && e.Button.IsActionButton())
            {
                Vector2 tile = e.Cursor.Tile;
                if (IsFarmLinkTerminalOnTile(tile, out StardewValley.Object terminalObject))
                {
                    if (FarmLinkTerminal.IsPlayerBelowTileAndFacingUp(Game1.player, terminalObject.TileLocation))
                    {
                        var itemTransferManager = new ItemTransferManager(GetAllChests(), new ItemTable(0, 0));
                        itemTransferManager.UpdateChestItemsAndSort();
                        Game1.activeClickableMenu = new FarmLinkTerminalMenu(GetAllChests());
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
                Monitor.Log("FarmLinkTerminal was removed, reverting workbenches if necessary.", LogLevel.Debug);
                RevertCustomWorkbenches(e.Location);
            }

            CheckForFarmLinkTerminal(e.Location);
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
                Monitor.Log($"Replaced Workbench with CustomWorkbench at {pair.Key} due to adjacent FarmLinkTerminal presence", LogLevel.Debug);
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
                Monitor.Log($"Reverted CustomWorkbench to standard Workbench at {tileLocation}", LogLevel.Debug);
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
                Monitor.Log($"Converted CustomWorkbench to standard Workbench at {tileLocation} before saving", LogLevel.Debug);
            }
        }

        private bool IsFarmLinkTerminalOnTile(Vector2 tile, out StardewValley.Object terminalObject)
        {
            return (Game1.currentLocation.objects.TryGetValue(tile, out terminalObject) && terminalObject.Name == farmLinkTerminalName) ||
                   (Game1.currentLocation.objects.TryGetValue(tile + new Vector2(0, 1), out terminalObject) && terminalObject.Name == farmLinkTerminalName);
        }

        private List<Chest> GetAllChests()
        {
            List<Chest> chests = new List<Chest>();
            foreach (var location in Game1.locations)
            {
                foreach (var item in location.objects.Pairs)
                {
                    if (item.Value is Chest chest && chest.playerChest.Value)
                    {
                        chests.Add(chest);
                    }
                }

                if (location is FarmHouse farmHouse)
                {
                    Chest fridge = farmHouse.fridge.Value;
                    if (fridge != null)
                    {
                        chests.Add(fridge);
                    }
                }
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

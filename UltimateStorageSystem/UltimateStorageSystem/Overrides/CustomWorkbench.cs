// CUSTOMWORKBENCH.CS
// This file defines a custom workbench that overrides the default behavior,
// allowing it to interact with all chests in the game while managing global access locks.

using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using StardewValley.Inventories;
using StardewValley.Locations;

#nullable disable

namespace UltimateStorageSystem.Overrides
{
    [Serializable]
    public class CustomWorkbench : Workbench
    {
      /* Whole class is unused we don't need to have anything inside now.
        public static readonly NetMutex globalChestMutex = new NetMutex();

        // Default constructor
        public CustomWorkbench() : base() { }

        // Constructor that sets the position of the workbench
        public CustomWorkbench(Vector2 position) : base(position) { }

        // Overrides the method called when the player interacts with the workbench
        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            GameLocation location = this.Location;
            if (location == null)
                return false;
            if (justCheckingForActivity)
                return true;

            // Creates a list of all chests found
            List<Chest> chestList = new List<Chest>();

            void AddChestsFromLocation(GameLocation location)
            {
                // Alle Objekte in der Location durchsuchen
                foreach (var item in location.objects.Values)
                {
                    if (item is Chest chest &&
                        (chest.SpecialChestType == Chest.SpecialChestTypes.None || chest.SpecialChestType == Chest.SpecialChestTypes.BigChest))
                    {
                        chestList.Add(chest);
                    }
                }

                // Kühlschrank im Farmhaus prüfen
                if (location is FarmHouse)
                {
                    Chest fridge = (location as FarmHouse).fridge.Value;
                    if (fridge != null)
                    {
                        chestList.Add(fridge);
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
            foreach (var gameLocation in Game1.locations)
            {
                AddChestsFromLocation(gameLocation);
            }

            // Creates a list for the inventories of the found chests
            List<IInventory> inventories = new List<IInventory>();

            foreach (Chest chest in chestList)
            {
                // Adds the chest's inventory to the inventory list
                inventories.Add((IInventory)chest.Items);
            }

            // If the global chest lock is not set, executes the following logic
            if (!globalChestMutex.IsLocked())
            {
                // Locks all chests for other players, but the workbench itself can use them
                globalChestMutex.RequestLock(() =>
                {
                    // Centers the crafting menu on the screen
                    Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
                    // Opens the crafting menu with the collected inventories
                    Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, standaloneMenu: true, materialContainers: inventories);
                    // Sets a function to execute when the menu is closed
                    Game1.activeClickableMenu.exitFunction = () =>
                    {
                        // Releases the global chest lock
                        globalChestMutex.ReleaseLock();
                    };
                },
                () =>
                {
                    // If locking the chests fails, displays an error message
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
                });
            }

            return true; // Returns that the action was successful
        }

        // Overrides the method that updates the workbench in the current location
        public override void updateWhenCurrentLocation(GameTime time)
        {
            GameLocation location = this.Location;
            if (location != null)
                globalChestMutex.Update(location);
            base.updateWhenCurrentLocation(time);
        }
        */
    }
}

// WORKBENCHPATCH.CS
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
using HarmonyLib;

namespace UltimateStorageSystem.Patches
{
    [HarmonyPatch(typeof(Workbench), nameof(Workbench.checkForAction))]
    public static class Workbench_checkForAction_Patch
    {
        private static bool IsTerminalAdjacent(Vector2 tileLocation, GameLocation location)
        {
            foreach (var offset in ModEntry.AdjacentTilesOffsets)
            {
                Vector2 adjacentTile = tileLocation + offset;
                if (location.objects.TryGetValue(adjacentTile, out StardewValley.Object adjacentObject) && adjacentObject.Name == ModEntry.FarmLinkTerminalName)
                {
                    return true;
                }
            }
            return false;
        }

        // Overrides the method called when the player interacts with the workbench
        public static bool Prefix(Workbench __instance, ref bool __result, Farmer who, bool justCheckingForActivity /* = false */) {
            GameLocation location = __instance.Location;
            if (location == null) {
                __result = false; // Set the return value of the Workbench.checkForAction method to default. (Probably not needed)
                return true;      // Then continue to the original method.
            }
            if (justCheckingForActivity) {
                __result = false; // Set the return value of the Workbench.checkForAction method to default. (Probably not needed)
                return true;      // Then continue to the original method.
            }
            if (IsTerminalAdjacent(__instance.TileLocation, location)) {
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
                    if (location is FarmHouse farmHouse)
                    {
                        Chest fridge = farmHouse.fridge.Value;
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
                if (!__instance.mutex.IsLocked())
                {
                    // Locks all chests for other players, but the workbench itself can use them
                    __instance.mutex.RequestLock(() =>
                    {
                        // Centers the crafting menu on the screen
                        Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
                        // Opens the crafting menu with the collected inventories
                        Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, standaloneMenu: true, materialContainers: inventories);
                        // Sets a function to execute when the menu is closed
                        Game1.activeClickableMenu.exitFunction = () =>
                        {
                            // Releases the global chest lock
                            __instance.mutex.ReleaseLock();
                        };
                    },
                    () =>
                    {
                        // If locking the chests fails, displays an error message
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
                    });
                }

                __result = true; // Returns that the action was successful // Set the return value of the Workbench.checkForAction method to true. (is for sure needed)
                return false;    // Does not continue the parent function. // Do not continue to the original method.
            }

            __result = false; // Set the return value of the Workbench.checkForAction method to default. (Probably not needed)
            return true;      // Then continue to the original method.
        }
    }

    [HarmonyPatch(typeof(Workbench), nameof(Workbench.updateWhenCurrentLocation))]
    public static class Workbench_updateWhenCurrentLocation_Patch
    {
        // Overrides the method that updates the workbench in the current location
        public static bool Prefix(Workbench __instance)
        {
            GameLocation location = __instance.Location;
            if (location != null)
                __instance.mutex.Update(location);
            return true; // continue to the original method.
        }
    }
}

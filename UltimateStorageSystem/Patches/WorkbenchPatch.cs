// WORKBENCHPATCH.CS
// This file defines a custom workbench that overrides the default behavior,
// allowing it to interact with all chests in the game while managing global access locks.

using StardewValley.Objects;
using StardewValley.Menus;
using StardewValley.Inventories;

namespace UltimateStorageSystem.Patches
{
    [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "UnusedType.Global"), SuppressMessage("ReSharper", "UnusedMember.Global"),]
    [HarmonyPatch(typeof(Workbench), nameof(Workbench.checkForAction))]
    public static class Workbench_checkForAction_Patch
    {
        /// <summary>Overrides the method called when the player interacts with the workbench</summary>
        /// <param name="__instance">The instance of the <see cref="Workbench"/> class.</param>
        /// <param name="__result">The output result of the original method.</param>
        /// <param name="who">(Unused) the <see cref="Farmer"/> currently interacting with the workbench.</param>
        /// <param name="justCheckingForActivity">Determines if we are currently checking for player activity or more.</param>
        /// <returns><see langword="true" /> if we should continue the original method otherwise only use this prefix.</returns>
        public static bool Prefix(Workbench __instance, ref bool __result, Farmer who, bool justCheckingForActivity /* = false */) {
            GameLocation location = __instance.Location;
            if (location is null) {
                __result = false; // Set the return value of the Workbench.checkForAction method to default. (Probably not needed)
                return true;      // Then continue to the original method.
            }
            if (justCheckingForActivity) {
                __result = false; // Set the return value of the Workbench.checkForAction method to default. (Probably not needed)
                return true;      // Then continue to the original method.
            }
            if (ModEntry.IsTerminalAdjacent(__instance.TileLocation, location)) {
                // Creates a list of all chests found
                // Durchlaufe alle Locations im Spiel
                // Creates a list for the inventories of the found chests
                // Adds the chest's inventory to the inventory list
                List<IInventory> inventories = [..ModEntry.GetAllChests().Select(chest => (IInventory)chest.Items)];

                // If the global chest lock is not set, executes the following logic
                if (!__instance.mutex.IsLocked())
                {
                    // Locks all chests for other players, but the workbench itself can use them
                    __instance.mutex.RequestLock(() =>
                    {
                        // Centers the crafting menu on the screen
                        Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + (IClickableMenu.borderWidth * 2), 600 + (IClickableMenu.borderWidth * 2));
                        // Opens the crafting menu with the collected inventories
                        Game1.activeClickableMenu = new CraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + (IClickableMenu.borderWidth * 2), 600 + (IClickableMenu.borderWidth * 2), standaloneMenu: true, materialContainers: inventories);
                        // Sets a function to execute when the menu is closed
                        Game1.activeClickableMenu.exitFunction = () =>
                        {
                            // Releases the global chest lock
                            __instance.mutex.ReleaseLock();
                        };
                    }, () =>
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

    [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "UnusedType.Global"), SuppressMessage("ReSharper", "UnusedMember.Global"),]
    [HarmonyPatch(typeof(Workbench), nameof(Workbench.updateWhenCurrentLocation))]
    public static class Workbench_updateWhenCurrentLocation_Patch
    {
        /// <summary>Overrides the method that updates the workbench in the current location</summary>
        /// <param name="__instance">The instance of the <see cref="Workbench"/> class.</param>
        /// <returns><see langword="true" /> if we should continue the original method otherwise only use this prefix.</returns>
        public static bool Prefix(Workbench __instance)
        {
            GameLocation location = __instance.Location;
            if (location is not null)
                __instance.mutex.Update(location);
            return true; // Continue to the original method.
        }
    }
}

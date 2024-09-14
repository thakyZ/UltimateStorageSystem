using Microsoft.Xna.Framework;
using StardewValley;

namespace UltimateStorageSystem.Tools
{
    internal static class FarmlinkChestManager
    {
        internal static bool HasChestInBlacklistForLocation(GameLocation location, Vector2 tileLocation)
        {
            return ModEntry.Instance.FarmLinkTerminalData?.HasChestInBlacklistForLocation(location, tileLocation) == true;
        }

        internal static void AddChestInBlacklistForLocation(GameLocation location, Vector2 tileLocation)
        {
            ModEntry.Instance.FarmLinkTerminalData?.AddChestInBlacklistForLocation(location, tileLocation);
        }

        internal static void RemoveChestInBlacklistForLocation(GameLocation location, Vector2 tileLocation)
        {
            ModEntry.Instance.FarmLinkTerminalData?.RemoveChestInBlacklistForLocation(location, tileLocation);
        }
    }
}
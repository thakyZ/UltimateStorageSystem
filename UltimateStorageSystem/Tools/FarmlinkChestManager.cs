namespace UltimateStorageSystem.Tools
{
    internal static class FarmLinkChestManager
    {
        internal static bool HasChestInFilterForLocation(GameLocation location, Vector2 tileLocation)
        {
            return ModEntry.Instance.FarmLinkTerminalData?.HasChestInFilterForLocation(location, tileLocation) == true;
        }

        internal static void AddChestInFilterForLocation(GameLocation location, Vector2 tileLocation)
        {
            ModEntry.Instance.FarmLinkTerminalData?.AddChestInFilterForLocation(location, tileLocation);
        }

        internal static void RemoveChestInFilterForLocation(GameLocation location, Vector2 tileLocation)
        {
            ModEntry.Instance.FarmLinkTerminalData?.RemoveChestInFilterForLocation(location, tileLocation);
        }
    }
}
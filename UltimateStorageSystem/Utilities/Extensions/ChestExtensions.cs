using StardewValley.Objects;

using UltimateStorageSystem.Tools;

namespace UltimateStorageSystem.Utilities.Extensions
{
    public static class ChestExtensions
    {
        public static bool IsFiltered(this Chest chest)
        {
            return FarmLinkChestManager.HasChestInFilterForLocation(chest.Location, chest.TileLocation);
        }

        public static bool Equals(this Chest? chest, Chest? other)
        {
            if (chest is null)
                return other is null;
            if (other is null)
                return false;
            return chest.Location.Equals(other: other.Location) && chest.TileLocation.Equals(other.TileLocation) && chest.Items.Equals(other: other.Items);
        }

        public static bool Equals(this Chest? chest, object? obj)
        {
            if (chest is null)
                return obj is null;
            if (obj is Chest other)
                return chest.Equals(other: other);
            return false;
        }

        public static bool HasRoomForItem(this Chest chest, Item? item) {
            if (item is null)
                return false;
            if (chest.Items.Count >= chest.GetActualCapacity()) {
                return chest.Items.Any(i => i.Equals(other: item) && i.Stack < i.maximumStackSize());
            }
            return true;
        }
    }
}

using StardewValley.Objects;

namespace UltimateStorageSystem.Utilities.Extensions
{
    internal static class GameLocationExtensions
    {
        public static GameLocation? ToGameLocation(this string? @string)
        {
            if (@string.TrueIfNullAndPrint()) return null;
            var location = Game1.locations.FirstOrDefault(x => x.NameOrUniqueName.Equals(@string));
            if (location.TrueIfNullAndPrint()) return null;
            return location;
        }

        public static bool Equals(this GameLocation? location, GameLocation? other)
        {
            if (location is null)
                return other is null;
            if (other is null)
                return false;
            return location.NameOrUniqueName.Equals(other.NameOrUniqueName);
        }

        public static bool Equals(this GameLocation? location, object? obj)
        {
            if (location is null)
                return obj is null;
            if (obj is GameLocation other)
                return location.Equals(other: other);
            return false;
        }
    }
}

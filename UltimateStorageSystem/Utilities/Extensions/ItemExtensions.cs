using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley;
using StardewValley.Inventories;

namespace UltimateStorageSystem.Utilities.Extensions
{
    public static class ItemExtensions
    {
        public static bool Equals(this Item? item, Item? other)
        {
            if (item is null)
                return other is null;
            if (other is null)
                return false;
            return item.Quality.Equals(other.Quality) && item.QualifiedItemId.Equals(other.QualifiedItemId) && item.Stack.Equals(item.Stack);
        }

        public static bool Equals(this Item? item, object? obj)
        {
            if (item is null)
                return obj is null;
            if (obj is Item other)
                return item.Equals(other: other);
            return false;
        }
    }
}

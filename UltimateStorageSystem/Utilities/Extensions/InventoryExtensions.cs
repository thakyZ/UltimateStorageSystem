using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley.Inventories;

namespace UltimateStorageSystem.Utilities.Extensions
{
    public static class InventoryExtensions
    {
        public static bool Equals(this Inventory? inventory, Inventory? other)
        {
            if (inventory is null)
                return other is null;
            if (other is null)
                return false;
            return inventory.All(x => other.Any(y => x.Equals(other: y)));
        }

        public static bool Equals(this Inventory? inventory, object? obj)
        {
            if (inventory is null)
                return obj is null;
            if (obj is Inventory other)
                return inventory.Equals(other: other);
            return false;
        }
    }
}

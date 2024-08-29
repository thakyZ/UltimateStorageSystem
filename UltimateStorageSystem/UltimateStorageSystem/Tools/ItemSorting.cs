// ITEMSORTING.CS
// This file provides a method for sorting items in a list
// based on various criteria and sort orders.

using System.Collections.Generic;
using System.Linq;
using UltimateStorageSystem.Utilities;

#nullable disable

namespace UltimateStorageSystem.Tools
{
    public static class ItemSorting
    {
        // Method to sort items based on a criterion and order
        public static List<ItemEntry> SortItems(List<ItemEntry> items, string criteria, bool ascending)
        {
            // Sort the items based on the specified criterion and order
            switch (criteria)
            {
                case "Name":
                    return ascending ? items.OrderBy(e => e.Name).ToList() : items.OrderByDescending(e => e.Name).ToList();
                case "Quantity":
                    return ascending ? items.OrderBy(e => e.Quantity).ToList() : items.OrderByDescending(e => e.Quantity).ToList();
                case "SingleValue":
                    return ascending ? items.OrderBy(e => e.SingleValue).ToList() : items.OrderByDescending(e => e.SingleValue).ToList();
                case "TotalValue":
                    return ascending ? items.OrderBy(e => e.TotalValue).ToList() : items.OrderByDescending(e => e.TotalValue).ToList();
                default:
                    return items;
            }
        }
    }
}

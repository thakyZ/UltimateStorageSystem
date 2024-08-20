// ITEMFILTERING.CS
// This file provides a method for filtering items in a list
// based on a search text.

using System.Collections.Generic;
using System.Linq;
using UltimateStorageSystem.Utilities;

#nullable disable

namespace UltimateStorageSystem.Tools
{
    public static class ItemFiltering
    {
        // Method to filter items based on the search text
        public static List<ItemEntry> FilterItems(List<ItemEntry> items, string searchText)
        {
            // If the search text is empty or contains only whitespace, return all items
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return items;
            }

            // Filter the items that contain the search text (case-insensitive)
            return items.Where(entry => entry.Name.Contains(searchText, System.StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}

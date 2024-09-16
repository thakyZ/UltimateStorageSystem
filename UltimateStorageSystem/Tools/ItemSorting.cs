// ITEMSORTING.CS
// This file provides a method for sorting items in a list
// based on various criteria and sort orders.

namespace UltimateStorageSystem.Tools
{
    public static class ItemSorting
    {
        // Method to sort items based on a criterion and order
        public static List<ItemEntry> SortItems(List<ItemEntry> items, string criteria, bool ascending)
        {
            // Sort the items based on the specified criterion and order
            return criteria switch
            {
                "Name" => ascending ? [.. items.OrderBy(e => e.Name)] : [.. items.OrderByDescending(e => e.Name)],
                "Quantity" => ascending ? [.. items.OrderBy(e => e.Quantity)] : [.. items.OrderByDescending(e => e.Quantity)],
                "SingleValue" => ascending ? [.. items.OrderBy(e => e.SingleValue)] : [.. items.OrderByDescending(e => e.SingleValue)],
                "TotalValue" => ascending ? [.. items.OrderBy(e => e.TotalValue)] : [.. items.OrderByDescending(e => e.TotalValue)],
                _ => items,
            };
        }
    }
}

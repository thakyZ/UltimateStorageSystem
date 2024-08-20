// ITEMENTRY.CS
// This file defines a class that represents an entry of an item with
// properties such as name, quantity, single value, total value, and the associated
// item object.

#nullable disable

namespace UltimateStorageSystem.Utilities
{
    // Represents an entry of an item with name, quantity, single value, total value, and the associated item object.
    public class ItemEntry
    {
        // Properties of the ItemEntry
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int SingleValue { get; set; }
        public int TotalValue { get; set; }
        public StardewValley.Item Item { get; set; } // Field for the associated item

        // Constructor to initialize an ItemEntry object.
        public ItemEntry(string name, int quantity, int singleValue, int totalValue, StardewValley.Item item = null)
        {
            Name = name;
            Quantity = quantity;
            SingleValue = singleValue >= 0 ? singleValue : 0; // Handle negative values
            TotalValue = totalValue >= 0 ? totalValue : 0; // Handle negative values
            Item = item; // Initialize the item field
        }
    }
}

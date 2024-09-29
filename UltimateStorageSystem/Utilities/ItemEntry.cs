// ITEMENTRY.CS
// This file defines a class that represents an entry of an item with
// properties such as name, quantity, single value, total value, and the associated
// item object.

namespace UltimateStorageSystem.Utilities
{
    /// <summary>Represents an entry of an item with name, quantity, single value, total value, and the associated item object.</summary>
    public class ItemEntry
    {
        // Properties of the ItemEntry
        public string Name        { get; }
        public int    Quantity    { get; set; }
        public int    SingleValue { get; }
        public int    TotalValue  { get; set; }
        /// <summary>Field for the associated item</summary>
        public Item?  Item        { get; }

        /// <summary>Constructor to initialize an ItemEntry object.</summary>
        public ItemEntry(string name, int quantity, int singleValue, int totalValue, Item? item = null)
        {
            Name = name;
            Quantity = quantity;
            SingleValue = singleValue >= 0 ? singleValue : 0; // Handle negative values
            TotalValue = totalValue >= 0 ? totalValue : 0; // Handle negative values
            Item = item; // Initialize the item field
        }
    }
}

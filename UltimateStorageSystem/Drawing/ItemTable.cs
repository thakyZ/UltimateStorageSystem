// ITEMTABLE.CS
// This file manages the table that displays the items, including
// functions for drawing, filtering, sorting, and scrolling through the
// item list.

using Microsoft.Xna.Framework.Graphics;
using UltimateStorageSystem.Interfaces; // Import the interface

namespace UltimateStorageSystem.Drawing
{
    public class ItemTable : IFilterableTable, IScrollableTable // Implementing both interfaces
    {
        // Starting position of the table
        public int StartX { get; }
        public int StartY { get; }

        // List of all items displayed in the table
        private List<ItemEntry> allItems;

        // List of filtered items currently displayed in the table
        private List<ItemEntry> filteredItems;

        // The current scroll index indicating the starting position of the displayed items
        public int ScrollIndex { get; set; }

        // Constructor, initializes the table with a starting position
        public ItemTable(int startX, int startY)
        {
            this.StartX = startX;
            this.StartY = startY;
            this.allItems = [];
            this.filteredItems = [];
            this.ScrollIndex = 0;
        }

        // Adds an item to the table
        public void AddItem(ItemEntry item)
        {
            allItems.Add(item);
            filteredItems.Add(item);
        }

        // Returns the currently filtered item entries
        public List<ItemEntry> GetItemEntries()
        {
            return filteredItems;
        }

        // Returns the number of visible rows in the table
        public int GetVisibleRows()
        {
            return 13; // Example: 13 visible rows
        }

        // Returns the number of item entries in the table
        public int GetItemEntriesCount()
        {
            return filteredItems.Count;
        }

        // Draws the table and its contents
        public void Draw(SpriteBatch b)
        {
            // Draws the headers of the table
            ItemTableRenderer.DrawHeaders(b, StartX, StartY, this);

            // Draws each row of the table based on the ScrollIndex
            for (int i = 0; i < GetVisibleRows(); i++)
            {
                int index = ScrollIndex + i;
                if (index >= filteredItems.Count)
                    break;

                // Calculates the Y-position of the current row
                int rowY = StartY + 100 + (32 * (i + 1)) + 10; // Y-position shifted down by 50 pixels
                ItemEntry entry = filteredItems[index];

                // Checks if the mouse is hovering over the current row
                bool isHovered = ItemTableRenderer.IsHoveringOverRow(i);

                // Draws the current row
                ItemTableRenderer.DrawRow(b, StartX + 40, rowY, entry, isHovered);
            }
        }

        // Filters the items in the table based on the search text
        public void FilterItems(string searchText)
        {
            filteredItems = new List<ItemEntry>(allItems);

            if (string.IsNullOrWhiteSpace(searchText))
                return;

            // Filters the items whose names contain the search text
            filteredItems = filteredItems.FindAll(item => item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        // Sorts the items in the table based on the specified column and sort direction
        public void SortItemsBy(string sortBy, bool ascending)
        {
            switch (sortBy)
            {
                case "Name":
                    if (ascending)
                        filteredItems.Sort((a, b) => a.Name.CompareTo(b.Name));
                    else
                        filteredItems.Sort((a, b) => b.Name.CompareTo(a.Name));
                    break;
                case "Quantity":
                    if (ascending)
                        filteredItems.Sort((a, b) => a.Quantity.CompareTo(b.Quantity));
                    else
                        filteredItems.Sort((a, b) => b.Quantity.CompareTo(a.Quantity));
                    break;
                case "SingleValue":
                    if (ascending)
                        filteredItems.Sort((a, b) => a.SingleValue.CompareTo(b.SingleValue));
                    else
                        filteredItems.Sort((a, b) => b.SingleValue.CompareTo(a.SingleValue));
                    break;
                case "TotalValue":
                    if (ascending)
                        filteredItems.Sort((a, b) => a.TotalValue.CompareTo(b.TotalValue));
                    else
                        filteredItems.Sort((a, b) => b.TotalValue.CompareTo(a.TotalValue));
                    break;
            }
        }

        // Refreshes the filtered list of items
        public void Refresh()
        {
            filteredItems = new List<ItemEntry>(allItems);
        }

        // Updates the quantity and value of an item in the table or adds it if it does not exist
        public void UpdateItemList(Item item, int remainingAmount)
        {
            // Searches for the entry corresponding to the specified item
            ItemEntry? entry = allItems.Find(i => i.Item == item);
            if (entry is not null)
            {
                if (remainingAmount <= 0)
                {
                    // Removes the item if there is no quantity left
                    allItems.Remove(entry);
                }
                else
                {
                    // Updates the quantity and total value of the item
                    entry.Quantity = remainingAmount;
                    entry.TotalValue = entry.SingleValue * remainingAmount;
                }
            }
            else if (remainingAmount > 0)
            {
                // Adds a new item to the list if it does not already exist
                allItems.Add(new ItemEntry(item.DisplayName, remainingAmount, item.salePrice(), item.salePrice() * remainingAmount, item));
            }
            Refresh();
        }

        // Processes left-clicks on the table
        [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1163")]
        public void ReceiveLeftClick(int x, int y)
        {
            // Logic for left-clicks on the table
        }

        // Processes hover actions on the table
        [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1163")]
        public void PerformHoverAction(int x, int y)
        {
            ItemTableRenderer.PerformHoverAction(x, y, this);
        }

        // Clears all items in the table
        public void ClearItems()
        {
            allItems.Clear();
            filteredItems.Clear();
        }
    }
}

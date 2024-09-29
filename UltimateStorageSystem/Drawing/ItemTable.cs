// ITEMTABLE.CS
// This file manages the table that displays the items, including
// functions for drawing, filtering, sorting, and scrolling through the
// item list.

using Microsoft.Xna.Framework.Graphics;
using UltimateStorageSystem.Interfaces;

using static System.String; // Import the interface

namespace UltimateStorageSystem.Drawing
{
    public class ItemTable : IFilterableTable, IScrollableTable // Implementing both interfaces
    {
        /// <summary>X-coordinate of the starting position of the table</summary>
        public int StartX { get; }
        /// <summary>Y-coordinate of the starting position of the table</summary>
        public int StartY { get; }

        /// <summary>List of all items displayed in the table</summary>
        private readonly List<ItemEntry> allItems;

        /// <summary>List of filtered items currently displayed in the table</summary>
        private List<ItemEntry> filteredItems;

        /// <summary>The current scroll index indicating the starting position of the displayed items</summary>
        public int ScrollIndex { get; set; }

        /// <summary>Constructor, initializes the table with a starting position</summary>
        /// <param name="startX">X-coordinate for the item table to draw.</param>
        /// <param name="startY">Y-coordinate for the item table to draw.</param>
        public ItemTable(int startX, int startY)
        {
            this.StartX = startX;
            this.StartY = startY;
            this.allItems = [];
            this.filteredItems = [];
            this.ScrollIndex = 0;
        }

        /// <summary>Adds an item to the table</summary>
        /// <param name="item">Entry for the item to display.</param>
        public void AddItem(ItemEntry item)
        {
            allItems.Add(item);
            filteredItems.Add(item);
        }

        /// <summary>Returns the currently filtered item entries</summary>
        /// <returns>List of all item entries.</returns>
        public List<ItemEntry> GetItemEntries()
        {
            return filteredItems;
        }

        /// <summary>Returns the number of visible rows in the table</summary>
        public int GetVisibleRows()
        {
            return 13; // Example: 13 visible rows
        }

        /// <summary>Returns the number of item entries in the table</summary>
        public int GetItemEntriesCount()
        {
            return filteredItems.Count;
        }

        /// <summary>Draws the table and its contents</summary>
        /// <param name="b">The spritebatch of which to draw the contents of.</param>
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

        /// <summary>Filters the items in the table based on the search text</summary>
        /// <param name="searchText">The text to search via</param>
        public void FilterItems(string searchText)
        {
            filteredItems = [..allItems];

            if (IsNullOrWhiteSpace(searchText))
                return;

            // Filters the items whose names contain the search text
            filteredItems = filteredItems.FindAll(item => item.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Sorts the items in the table based on the specified column and sort direction</summary>
        /// <param name="sortBy">Sort by row title.</param>
        /// <param name="ascending">Determines if we should sort ascending or descending.</param>
        public void SortItemsBy(string sortBy, bool ascending)
        {
            switch (sortBy)
            {
                case "Name":
                    if (ascending)
                        filteredItems.Sort((a, b) => CompareOrdinal(a.Name, b.Name));
                    else
                        filteredItems.Sort((a, b) => CompareOrdinal(b.Name, a.Name));
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

        /// <summary>Refreshes the filtered list of items</summary>
        public void Refresh()
        {
            filteredItems = [..allItems];
        }

        /// <summary>Updates the quantity and value of an item in the table or adds it if it does not exist</summary>
        /// <param name="item">An item yo update to the item list.</param>
        /// <param name="remainingAmount">The remaining items to update on the system.</param>
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

        /// <summary>Processes left-clicks on the table</summary>
        /// <param name="x">X-coordinate of the left click on the menu.</param>
        /// <param name="y">Y-coordinate of the left click on the menu.</param>
        [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1163"), SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public void ReceiveLeftClick(int x, int y)
        {
            // Logic for left-clicks on the table
        }

        /// <summary>Processes hover actions on the table</summary>
        /// <param name="x">X-coordinate of the mouse over the menu.</param>
        /// <param name="y">Y-coordinate of the mouse over the menu.</param>
        [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1163")]
        public void PerformHoverAction(int x, int y)
        {
            ItemTableRenderer.PerformHoverAction(x, y, this);
        }

        /// <summary>Clears all items in the table</summary>
        public void ClearItems()
        {
            allItems.Clear();
            filteredItems.Clear();
        }
    }
}

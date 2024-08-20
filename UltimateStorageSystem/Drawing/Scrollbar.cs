// SCROLLBAR.CS
// This file defines a scrollbar used for navigating the
// item table and handles the corresponding user interactions.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

#nullable disable

namespace UltimateStorageSystem.Drawing
{
    public class Scrollbar
    {
        // Areas for the scrollbar
        private readonly Rectangle scrollBarRunner;  // The area within which the scrollbar can move
        private Rectangle scrollBar;  // The actual scrollbar slider
        private bool isScrolling = false;  // Flag to indicate if scrolling is in progress
        private readonly ItemTable itemTable;  // Reference to the table being scrolled

        // Constructor, initializes the scrollbar with position and associated table
        public Scrollbar(int x, int y, ItemTable itemTable)
        {
            this.itemTable = itemTable;
            scrollBarRunner = new Rectangle(x - 5, y + 40, 20, 400);  // Offset the scrollbar 40px down and 5px to the left
            scrollBar = new Rectangle(scrollBarRunner.X, scrollBarRunner.Y, 20, 20);
        }

        // Draws the scrollbar on the screen
        public void Draw(SpriteBatch b)
        {
            // Draws the background of the scrollbar
            b.Draw(Game1.staminaRect, new Rectangle(scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height), Color.Gray);

            // Draws the scrollbar slider
            b.Draw(Game1.staminaRect, scrollBar, Color.White);
        }

        // Handles left-clicks on the scrollbar and starts the scrolling process
        public void ReceiveLeftClick(int x, int y)
        {
            if (scrollBarRunner.Contains(x, y))
            {
                isScrolling = true;
                UpdateScrollBar(y);
            }
        }

        // Handles held left-clicks and adjusts the scrollbar accordingly
        public void LeftClickHeld(int x, int y)
        {
            if (isScrolling)
            {
                UpdateScrollBar(y);
            }
        }

        // Ends the scrolling process when the mouse button is released
        public void ReleaseLeftClick(int _x, int _y)
        {
            isScrolling = false;
        }

        // Handles scroll wheel actions and scrolls the table accordingly
        public void ReceiveScrollWheelAction(int direction)
        {
            int scrollAmount = direction > 0 ? -1 : 1; // Scroll up or down
            int itemCount = itemTable.GetItemEntries().Count;
            int visibleRows = ItemTable.GetVisibleRows();
            itemTable.ScrollIndex = Math.Clamp(itemTable.ScrollIndex + scrollAmount, 0, Math.Max(0, itemCount - visibleRows));
            UpdateScrollBarPosition();
        }

        // Updates the scrollbar position based on the current scroll index of the table
        public void UpdateScrollBarPosition()
        {
            int itemCount = itemTable.GetItemEntries().Count;
            int visibleRows = ItemTable.GetVisibleRows();

            if (itemCount > visibleRows)
            {
                // Calculate the proportion of visible rows to total rows
                float proportionVisible = visibleRows / (float)itemCount;

                // Adjust the height of the scrollbar slider
                scrollBar.Height = (int)(scrollBarRunner.Height * proportionVisible);

                // Calculate where the slider should be positioned
                float percent = itemTable.ScrollIndex / (float)(itemCount - visibleRows);
                scrollBar.Y = scrollBarRunner.Y + (int)(percent * (scrollBarRunner.Height - scrollBar.Height));
            }
            else
            {
                scrollBar.Y = scrollBarRunner.Y;
                scrollBar.Height = scrollBarRunner.Height; // The slider is maximized if there is nothing to scroll
            }
        }

        // Updates the position of the scrollbar based on the mouse position
        private void UpdateScrollBar(int y)
        {
            float percent = (y - scrollBarRunner.Y) / (float)(scrollBarRunner.Height - scrollBar.Height);
            int itemCount = itemTable.GetItemEntries().Count;
            int visibleRows = ItemTable.GetVisibleRows();
            itemTable.ScrollIndex = (int)(percent * Math.Max(0, itemCount - visibleRows));
            itemTable.ScrollIndex = Math.Clamp(itemTable.ScrollIndex, 0, Math.Max(0, itemCount - visibleRows));
            UpdateScrollBarPosition();
        }
    }
}

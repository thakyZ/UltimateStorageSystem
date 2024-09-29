// SCROLLBAR.CS
// This file defines a scrollbar used for navigating the
// item table and handles the corresponding user interactions.

using Microsoft.Xna.Framework.Graphics;
using UltimateStorageSystem.Interfaces; // Import the interface

namespace UltimateStorageSystem.Drawing
{
    /// <summary>
    /// This class defines a scrollbar used for navigating the
    /// item table and handles the corresponding user interactions.
    /// </summary>
    public class Scrollbar
    {
        // Areas for the scrollbar
        /// <summary>The area within which the scrollbar can move</summary>
        private          Rectangle        scrollBarRunner;
        /// <summary>The actual scrollbar slider</summary>
        private          Rectangle        scrollBar;
        /// <summary>Flag to indicate if scrolling is in progress</summary>
        private          bool             isScrolling = false;
        /// <summary>Reference to the table being scrolled</summary>
        private readonly IScrollableTable table;

        /// <summary>Constructor, initializes the scrollbar with position and associated table</summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="table"></param>
        public Scrollbar(int x, int y, IScrollableTable table)
        {
            this.table      = table;
            scrollBarRunner = new Rectangle(x - 5,             y + 40,            20, 400);  // Offset the scrollbar 40px down and 5px to the left
            scrollBar       = new Rectangle(scrollBarRunner.X, scrollBarRunner.Y, 20, GetScrollBarHeight());
        }

        /// <summary>Draws the scrollbar on the screen</summary>
        /// <param name="b"></param>
        public void Draw(SpriteBatch b)
        {
            // Draws the background of the scrollbar
            b.Draw(Game1.staminaRect, new Rectangle(scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height), Color.Gray);

            // Draws the scrollbar slider
            b.Draw(Game1.staminaRect, scrollBar, Color.White);
        }

        /// <summary>Handles left-clicks on the scrollbar and starts the scrolling process</summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void ReceiveLeftClick(int x, int y)
        {
            // ReSharper disable once InvertIf
            if (scrollBarRunner.Contains(x, y))
            {
                isScrolling = true;
                UpdateScrollBar(y);
            }
        }

        /// <summary>Handles held left-clicks and adjusts the scrollbar accordingly</summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1163")]
        public void LeftClickHeld(int x, int y)
        {
            if (isScrolling)
            {
                UpdateScrollBar(y);
            }
        }

        /// <summary>Ends the scrolling process when the mouse button is released</summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("Roslynator", "RCS1163")]
        public void ReleaseLeftClick(int x, int y)
        {
            isScrolling = false;
        }

        /// <summary>Handles scroll wheel actions and scrolls the table accordingly</summary>
        /// <param name="direction"></param>
        public void ReceiveScrollWheelAction(int direction)
        {
            int scrollAmount = direction > 0 ? -1 : 1; // Scroll up or down
            int itemCount = table.GetItemEntriesCount();
            int visibleRows = table.GetVisibleRows();
            table.ScrollIndex = Math.Clamp(table.ScrollIndex + scrollAmount, 0, Math.Max(0, itemCount - visibleRows));
            UpdateScrollBarPosition();
        }

        /// <summary>Gets the height of the scrollbar on startup</summary>
        [SuppressMessage("CodeQuality", "IDE0079"), SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public int GetScrollBarHeight()
        {
            int itemCount   = table.GetItemEntriesCount();
            int visibleRows = table.GetVisibleRows();

            if (itemCount > visibleRows)
            {
                // Calculate the proportion of visible rows to total rows
                float proportionVisible = visibleRows / (float)itemCount;

                // Adjust the height of the scrollbar slider
                return (int)(scrollBarRunner.Height * proportionVisible);
            }

            return scrollBarRunner.Height;
        }

        /// <summary>Updates the scrollbar position based on the current scroll index of the table</summary>
        public void UpdateScrollBarPosition()
        {
            int itemCount = table.GetItemEntriesCount();
            int visibleRows = table.GetVisibleRows();

            // Adjust the height of the scrollbar slider
            scrollBar.Height = GetScrollBarHeight();

            if (itemCount > visibleRows)
            {
                // Calculate where the slider should be positioned
                float percent = table.ScrollIndex / (float)(itemCount - visibleRows);
                scrollBar.Y = scrollBarRunner.Y + (int)(percent * (scrollBarRunner.Height - scrollBar.Height));
            }
            else
            {
                scrollBar.Y = scrollBarRunner.Y;
                scrollBar.Height = scrollBarRunner.Height; // The slider is maximized if there is nothing to scroll
            }
        }

        /// <summary>Updates the position of the scrollbar based on the mouse position</summary>
        /// <param name="y"></param>
        private void UpdateScrollBar(int y)
        {
            float percent = (y - scrollBarRunner.Y) / (float)(scrollBarRunner.Height - scrollBar.Height);
            int itemCount = table.GetItemEntriesCount();
            int visibleRows = table.GetVisibleRows();
            table.ScrollIndex = (int)(percent * Math.Max(0, itemCount - visibleRows));
            table.ScrollIndex = Math.Clamp(table.ScrollIndex, 0, Math.Max(0, itemCount - visibleRows));
            UpdateScrollBarPosition();
        }
    }
}

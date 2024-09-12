// INPUTHANDLER.CS
// This file handles various user interactions such as mouse clicks,
// key presses, and scroll wheel movements within the FarmLink Terminal menu.

using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using UltimateStorageSystem.Drawing;

#nullable disable

namespace UltimateStorageSystem.Tools
{
    public class InputHandler
    {
        // Dependencies of the InputHandler
        private readonly InventoryMenu playerInventoryMenu;
        private readonly Scrollbar scrollbar;
        private readonly SearchBox searchBox;
        private readonly FarmLinkTerminalMenu terminalMenu;

        // Constructor to initialize dependencies
        public InputHandler(InventoryMenu playerInventoryMenu, Scrollbar scrollbar, SearchBox searchBox, FarmLinkTerminalMenu terminalMenu)
        {
            this.playerInventoryMenu = playerInventoryMenu;
            this.scrollbar = scrollbar;
            this.searchBox = searchBox;
            this.terminalMenu = terminalMenu;
        }

        // Method for handling a left-click
        public void ReceiveLeftClick(int x, int y, bool playSound = true)
        {
            // Process the left-click in the player inventory
            playerInventoryMenu.receiveLeftClick(x, y, playSound);
            // Process the left-click on the scrollbar
            scrollbar.ReceiveLeftClick(x, y);
            // Update the search box after a click
            searchBox.Update();
        }

        // Method for handling holding a left-click
        public void LeftClickHeld(int x, int y)
        {
            // Process holding a left-click on the scrollbar
            scrollbar.LeftClickHeld(x, y);
        }

        // Method for handling releasing a left-click
        public void ReleaseLeftClick(int x, int y)
        {
            // Process releasing a left-click on the scrollbar
            scrollbar.ReleaseLeftClick(x, y);
        }

        // Method for handling hovering over an element
        public void PerformHoverAction(int x, int y)
        {
            // Process hovering over the player inventory
            playerInventoryMenu.performHoverAction(x, y);
        }

        // Method for handling a key press
        public void ReceiveKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                // Close the menu when the Escape key is pressed
                terminalMenu.exitThisMenu();
                return;
            }
        }

        // Method for handling a scroll wheel action
        public void ReceiveScrollWheelAction(int direction)
        {
            // Process the scroll wheel action on the scrollbar
            scrollbar.ReceiveScrollWheelAction(direction);
        }
    }
}

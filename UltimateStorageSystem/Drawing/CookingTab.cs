using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace UltimateStorageSystem.Drawing
{
    public class CookingTab : IClickableMenu
    {
        /// <summary>Player inventory menu</summary>
        private readonly InventoryMenu playerInventoryMenu;
        // ReSharper disable ConvertToConstant.Local
        /// <summary>Width of the main container</summary>
        private readonly int containerWidth  = 830;
        /// <summary>Height of the main container</summary>
        private readonly int containerHeight = 900;
        /// <summary>Height of the computer menu</summary>
        private readonly int computerMenuHeight;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        /// <summary>Width of the inventory menu</summary>
        private readonly int inventoryMenuWidth;
        /// <summary>Fixed height for the bottom frame (inventory area)</summary>
        private readonly int inventoryMenuHeight = 280;
        // ReSharper restore ConvertToConstant.Local

        /// <summary>Constructor for the CookingTab</summary>
        /// <param name="xPositionOnScreen">X-coorinate for the position of the menu on the screen.</param>
        /// <param name="yPositionOnScreen">Y-coorinate for the position of the menu on the screen.</param>
        public CookingTab(int xPositionOnScreen, int yPositionOnScreen) : base(xPositionOnScreen, yPositionOnScreen, 800, 1000)
        {
            // Calculate the height of the computer menu.
            computerMenuHeight = containerHeight - inventoryMenuHeight;

            // Calculate the width of the inventory menu based on the number of slots per row.
            int slotsPerRow = 12; // Assumption: 12 slots per row
            int slotSize = 64; // Size of an inventory slot
            inventoryMenuWidth = slotsPerRow * slotSize;

            // Position of the inventory menu.
            int inventoryMenuX = this.xPositionOnScreen + ((containerWidth - inventoryMenuWidth) / 2);
            int inventoryMenuY = this.yPositionOnScreen + computerMenuHeight + 55;
            playerInventoryMenu = new(inventoryMenuX, inventoryMenuY, true);
        }

        /// <summary>Draws the content of the CookingTab</summary>
        /// <param name="b"></param>
        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            // Draw the computer frame
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, containerWidth, computerMenuHeight, Color.White);
            b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 12, this.yPositionOnScreen + 12, containerWidth - 24, computerMenuHeight - 24), Color.Black);

            // Draw the inventory frame
            IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen + computerMenuHeight, containerWidth, inventoryMenuHeight, Color.White);
            playerInventoryMenu.draw(b);

            this.drawMouse(b);
        }

        // Additional logic can be added here for handling interactions within this tab.
    }
}

using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

#nullable disable

namespace UltimateStorageSystem.Drawing
{
    public class CookingTab : IClickableMenu
    {
        private InventoryMenu playerInventoryMenu; // Player inventory menu
        private int containerWidth = 830; // Width of the main container
        private int containerHeight = 900; // Height of the main container
        private int computerMenuHeight; // Height of the computer menu
        private int inventoryMenuWidth; // Width of the inventory menu
        private int inventoryMenuHeight = 280; // Fixed height for the bottom frame (inventory area)

        // Constructor for the CookingTab
        public CookingTab(int xPositionOnScreen, int yPositionOnScreen)
            : base(xPositionOnScreen, yPositionOnScreen, 800, 1000)
        {
            // Calculate the height of the computer menu.
            computerMenuHeight = containerHeight - inventoryMenuHeight;

            // Calculate the width of the inventory menu based on the number of slots per row.
            int slotsPerRow = 12; // Assumption: 12 slots per row
            int slotSize = 64; // Size of an inventory slot
            inventoryMenuWidth = slotsPerRow * slotSize;

            // Position of the inventory menu.
            int inventoryMenuX = this.xPositionOnScreen + (containerWidth - inventoryMenuWidth) / 2;
            int inventoryMenuY = this.yPositionOnScreen + computerMenuHeight + 55;
            playerInventoryMenu = new InventoryMenu(inventoryMenuX, inventoryMenuY, true);
        }

        // Draws the content of the CookingTab
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

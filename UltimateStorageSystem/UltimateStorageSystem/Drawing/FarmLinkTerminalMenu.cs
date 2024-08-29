using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Input;
using UltimateStorageSystem.Tools;
using UltimateStorageSystem.Utilities;
using System.Collections.Generic;
using StardewValley.Objects;

#nullable disable

namespace UltimateStorageSystem.Drawing
{
    public class FarmLinkTerminalMenu : IClickableMenu
    {
        // Definition of GUI components and parameters
        private InventoryMenu playerInventoryMenu; // Player inventory menu
        private int containerWidth = 830; // Width of the main container
        private int containerHeight = 900; // Height of the main container
        private int computerMenuHeight; // Height of the computer menu
        private int inventoryMenuWidth; // Width of the inventory menu
        private int inventoryMenuHeight = 280; // Fixed height for the bottom frame (inventory area)

        private ItemTable itemTable; // Table for displaying items
        private Scrollbar scrollbar; // Scrollbar for the table
        private SearchBox searchBox; // Search box for filtering items
        private InputHandler inputHandler; // Handles input within the menu
        private ItemTransferManager itemTransferManager; // Manager for transferring items

        private List<ClickableTextureComponent> tabs; // Liste der Reiter
        private int selectedTab; // Der aktuell ausgewählte Reiter

        // Flag to ignore the first right-click after opening the menu.
        private bool ignoreNextRightClick = true;

        // Constructor for the menu, initializes the GUI components.
        public FarmLinkTerminalMenu(List<Chest> chests) : base(Game1.viewport.Width / 2 - 400, Game1.viewport.Height / 2 - 500, 800, 1000)
        {
            // Calculate the position of the container relative to the screen.
            this.xPositionOnScreen = (Game1.viewport.Width - this.containerWidth) / 2;
            this.yPositionOnScreen = (Game1.viewport.Height - this.containerHeight) / 2;

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

            // Initialize the item table and scrollbar.
            itemTable = new ItemTable(this.xPositionOnScreen, this.yPositionOnScreen);
            scrollbar = new Scrollbar(this.xPositionOnScreen + containerWidth - 40, this.yPositionOnScreen + 120, itemTable);

            // Initialize the search textbox.
            searchBox = new SearchBox(xPositionOnScreen + 30, yPositionOnScreen + 20, itemTable, scrollbar);

            // Initialize the ItemTransferManager to manage chest items.
            itemTransferManager = new ItemTransferManager(chests, itemTable);
            itemTransferManager.UpdateChestItemsAndSort(); // Updates and sorts the items.

            // Initialize the input handler for the menu.
            inputHandler = new InputHandler(playerInventoryMenu, scrollbar, searchBox, this);

            // Sort the items by name and set the sort icon accordingly.
            itemTable.SortItemsBy("Name", true);
            ItemTableRenderer.SetSortState("Name", true);

            // Reiter initialisieren
            tabs = new List<ClickableTextureComponent>
            {
                new ClickableTextureComponent("Terminal", new Rectangle(xPositionOnScreen + 20, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
                //new ClickableTextureComponent("Tab2", new Rectangle(xPositionOnScreen + 88, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
                //new ClickableTextureComponent("Tab3", new Rectangle(xPositionOnScreen + 156, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
                //new ClickableTextureComponent("Tab4", new Rectangle(xPositionOnScreen + 224, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
            };

            selectedTab = 0; // Standardmäßig den ersten Reiter auswählen
        }

        // Draws the menu and all components.
        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            // Zeichne den Inhalt des ausgewählten Tabs
            if (selectedTab == 0)
            {
                // Inhalt für den ersten Tab zeichnen
                IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, containerWidth, computerMenuHeight, Color.White);
                b.Draw(Game1.staminaRect, new Rectangle(this.xPositionOnScreen + 12, this.yPositionOnScreen + 12, containerWidth - 24, computerMenuHeight - 24), Color.Black);

                // Zeichnet den Titel des Terminal-Menüs fett.
                string title = "ULTIMATE STORAGE SYSTEM";

                // Setze die maximale Breite und den Abstand von rechts
                float maxTitleWidth = 400f;
                float rightPadding = 30f;

                // Berechnet die Skalierung basierend auf der Breite des Textes
                float scale = Math.Min(1f, maxTitleWidth / Game1.dialogueFont.MeasureString(title).X);

                // Berechnet die Position des Titels, rechtsbündig mit 20px Abstand
                Vector2 titlePosition = new Vector2(
                    this.xPositionOnScreen + this.containerWidth - rightPadding - maxTitleWidth,
                    this.yPositionOnScreen + 40
                );

                Color titleColor = Color.Orange;
                Color titleShadowColor = Color.Brown;

                // Zeichnet den Schatten des Textes fett und leicht versetzt
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Game1.spriteBatch.DrawString(
                            Game1.dialogueFont,
                            title,
                            titlePosition + new Vector2(dx + 3, dy + 3),
                            titleShadowColor,
                            0f,
                            Vector2.Zero,
                            scale,
                            SpriteEffects.None,
                            0.86f
                        );
                    }
                }

                // Zeichnet den Titel fett, rechtsbündig
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Game1.spriteBatch.DrawString(
                            Game1.dialogueFont,
                            title,
                            titlePosition + new Vector2(dx, dy),
                            titleColor,
                            0f,
                            Vector2.Zero,
                            scale,
                            SpriteEffects.None,
                            0.86f
                        );
                    }
                }

                searchBox.Draw(b);
                Vector2 magnifyingGlassPosition = new Vector2(xPositionOnScreen + 250, yPositionOnScreen + 36);
                searchBox.DrawMagnifyingGlass(b, magnifyingGlassPosition);
                itemTable.Draw(b);
                IClickableMenu.drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen + computerMenuHeight, containerWidth, inventoryMenuHeight, Color.White);
                playerInventoryMenu.draw(b);
                scrollbar.Draw(b);
            }
            else if (selectedTab == 1)
            {
                // Neuen WorkbenchTab zeichnen
                WorkbenchTab workbenchTab = new WorkbenchTab(this.xPositionOnScreen, this.yPositionOnScreen);
                workbenchTab.draw(b);
            }
            else if (selectedTab == 2)
            {
                // Neuen CookingTab zeichnen
                CookingTab cookingTab = new CookingTab(this.xPositionOnScreen, this.yPositionOnScreen);
                cookingTab.draw(b);
            }
            else if (selectedTab == 3)
            {
                // Neuen ShoppingTab zeichnen
                ShoppingTab shoppingTab = new ShoppingTab(this.xPositionOnScreen, this.yPositionOnScreen);
                shoppingTab.draw(b);
            }

            // Zeichne die Reiter und Icons nach dem Inhalt der Tabs
            foreach (var tab in tabs)
            {
                bool isSelected = tabs.IndexOf(tab) == selectedTab;

                // Wenn aktiv, verschiebe den Reiter um 8 Pixel nach unten
                int yOffset = isSelected ? 8 : 0;

                // Zeichne den Tab-Reiter mit dem entsprechenden Offset
                tab.bounds.Y += yOffset;
                tab.draw(b, Color.White, 0.86f);
                tab.bounds.Y -= yOffset;

                // Icon für jeden Tab zeichnen
                Texture2D chestIcon = Game1.objectSpriteSheet;
                Rectangle sourceRect;
                Vector2 tabIconPosition;

                // Setze das Icon je nach Tab
                if (tabs.IndexOf(tab) == 0)
                {
                    sourceRect = Game1.getSourceRectForStandardTileSheet(chestIcon, 166, 16, 16); // Icon Schatzkiste
                    tabIconPosition = new Vector2(xPositionOnScreen + 36, yPositionOnScreen - 40 + yOffset);
                }
                else if (tabs.IndexOf(tab) == 1)
                {
                    // Beispiel: Mauszeiger-Sprite mit Hammer-Symbol zeichnen
                    Texture2D mouseCursors = Game1.mouseCursors;
                    Rectangle hammerSourceRect = new Rectangle(64, 368, 16, 16);

                    // Neue Position, an der das Symbol gezeichnet wird
                    Vector2 hammerIconPosition = new Vector2(xPositionOnScreen + 88, yPositionOnScreen - 64 + yOffset);

                    // Korrektes Zeichnen des Symbols
                    b.Draw(mouseCursors, hammerIconPosition, hammerSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

                    // Überspringe das Zeichnen des Standard-Icons
                    continue;
                }
                else if (tabs.IndexOf(tab) == 2)
                {
                    sourceRect = Game1.getSourceRectForStandardTileSheet(chestIcon, 241, 16, 16); // Icon Hamburger
                    tabIconPosition = new Vector2(xPositionOnScreen + 172, yPositionOnScreen - 40 + yOffset);
                }
                else // Index == 3
                {
                    // Hier die Zuweisung von basketTexture
                    Texture2D basketTexture = ModEntry.basketTexture;
                    sourceRect = new Rectangle(0, 0, basketTexture.Width, basketTexture.Height);

                    // Position des Basket-Icons anpassen
                    Vector2 basketIconPosition = new Vector2(tab.bounds.X + 16, tab.bounds.Y + 22 + yOffset);
                    b.Draw(basketTexture, basketIconPosition, sourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);

                    // Überspringe das Zeichnen des Standard-Icons
                    continue;
                }

                // Zeichne das Icon
                b.Draw(chestIcon, tabIconPosition, sourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
            }

            this.drawMouse(b);
        }


        // Processes left-clicks on the menu.
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (var tab in tabs)
            {
                if (tab.containsPoint(x, y))
                {
                    selectedTab = tabs.IndexOf(tab); // Setze den ausgewählten Reiter
                    Game1.playSound("smallSelect"); // Spiele einen Soundeffekt ab
                    return; // Beende die Methode, damit nicht versehentlich das Menü interagiert
                }
            }

            // Nur Aktionen verarbeiten, wenn der erste Tab ausgewählt ist
            if (selectedTab == 0)
            {
                if (new Rectangle(searchBox.textBox.X, searchBox.textBox.Y, searchBox.textBox.Width, searchBox.textBox.Height).Contains(x, y))
                {
                    searchBox.Click();
                }
                else
                {
                    inputHandler.ReceiveLeftClick(x, y, playSound);
                    itemTable.ReceiveLeftClick(x, y);

                    Item clickedItem = GetItemAt(x, y, out bool isInInventory);
                    if (clickedItem != null)
                    {
                        bool shiftPressed = Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.oldKBState.IsKeyDown(Keys.RightShift);
                        itemTransferManager.HandleLeftClick(clickedItem, isInInventory, shiftPressed);

                        string searchText = searchBox.textBox.Text.TrimStart(searchBox.nonBreakingSpace);
                        itemTable.FilterItems(searchText);
                    }
                }

                ItemTableRenderer.HandleHeaderClick(x, y, itemTable);
            }
        }

        // Processes right-clicks on the menu.
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (ignoreNextRightClick)
            {
                ignoreNextRightClick = false;
                return;
            }

            if (selectedTab == 0)
            {
                Item clickedItem = GetItemAt(x, y, out bool isInInventory);
                if (clickedItem != null)
                {
                    bool shiftPressed = Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.oldKBState.IsKeyDown(Keys.RightShift);
                    itemTransferManager.HandleRightClick(clickedItem, isInInventory, shiftPressed);

                    string searchText = searchBox.textBox.Text.TrimStart(searchBox.nonBreakingSpace);
                    itemTable.FilterItems(searchText);
                }
            }
        }

        // Retrieves the clicked item from the inventory or table.
        private Item GetItemAt(int x, int y, out bool isInInventory)
        {
            isInInventory = false;

            foreach (ClickableComponent slot in playerInventoryMenu.inventory)
            {
                if (slot.containsPoint(x, y) && playerInventoryMenu.actualInventory.Count > slot.myID)
                {
                    isInInventory = true;
                    return playerInventoryMenu.actualInventory[slot.myID];
                }
            }

            for (int i = 0; i < ItemTable.GetVisibleRows(); i++)
            {
                int index = itemTable.ScrollIndex + i;
                if (index >= itemTable.GetItemEntries().Count)
                    break;

                int startX = itemTable.StartX + 40;
                int startY = itemTable.StartY + 100;
                int rowY = startY + 32 * (i + 1) + 10;
                if (new Rectangle(startX - 20, rowY, 740, 32).Contains(x, y))
                {
                    return itemTable.GetItemEntries()[index].Item;
                }
            }

            return null;
        }

        // Processes holding the left mouse button.
        public override void leftClickHeld(int x, int y)
        {
            inputHandler.LeftClickHeld(x, y);
        }

        // Processes releasing the left mouse button.
        public override void releaseLeftClick(int x, int y)
        {
            inputHandler.ReleaseLeftClick(x, y);
        }

        // Processes hover actions.
        public override void performHoverAction(int x, int y)
        {
            inputHandler.PerformHoverAction(x, y);
            if (selectedTab == 0)
            {
                itemTable.PerformHoverAction(x, y);
            }
        }

        // Processes keyboard inputs.
        public override void receiveKeyPress(Keys key)
        {
            inputHandler.ReceiveKeyPress(key);
        }

        // Processes scroll wheel actions.
        public override void receiveScrollWheelAction(int direction)
        {
            if (selectedTab == 0)
            {
                inputHandler.ReceiveScrollWheelAction(direction);
            }
        }
    }
}

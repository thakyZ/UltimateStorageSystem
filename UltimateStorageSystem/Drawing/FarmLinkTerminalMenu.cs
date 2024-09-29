using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Menus;
using StardewValley.Objects;
using UltimateStorageSystem.Tools;

namespace UltimateStorageSystem.Drawing
{
    public class FarmLinkTerminalMenu : IClickableMenu
    {
        // Definition of GUI components and parameters
        /// <summary>Player inventory menu</summary>
        private readonly InventoryMenu playerInventoryMenu;
        // ReSharper disable ConvertToConstant.Local
        /// <summary>Width of the main container</summary>
        private readonly int           containerWidth  = 830;
        /// <summary>Height of the main container</summary>
        private readonly int           containerHeight = 900;
        /// <summary>Height of the computer menu</summary>
        private readonly int           computerMenuHeight;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        /// <summary>Width of the inventory menu</summary>
        private readonly int           inventoryMenuWidth;
        /// <summary>Fixed height for the bottom frame (inventory area)</summary>
        private readonly int           inventoryMenuHeight = 280;
        // ReSharper restore ConvertToConstant.Local

        /// <summary>Table for displaying items</summary>
        private readonly ItemTable           itemTable;
        /// <summary>Scrollbar for the table</summary>
        private readonly Scrollbar           scrollbar;
        /// <summary>Search box for filtering items</summary>
        private readonly SearchBox           searchBox;
        /// <summary>Handles input within the menu</summary>
        private readonly InputHandler        inputHandler;
        /// <summary>Manager for transferring items</summary>
        private readonly ItemTransferManager itemTransferManager;

        /// <summary>Liste der Reiter</summary>
        private readonly List<ClickableTextureComponent> tabs;
        /// <summary>Der aktuell ausgewählte Reiter</summary>
        private          int                             selectedTab;

        /// <summary>Constructor for the menu, initializes the GUI components.</summary>
        /// <param name="chests">The list of chests in the network.</param>
        public FarmLinkTerminalMenu(List<Chest> chests) : base((Game1.viewport.Width / 2) - 400, (Game1.viewport.Height / 2) - 500, 800, 1000)
        {
            // Calculate the position of the container relative to the screen.
            this.xPositionOnScreen = (Game1.viewport.Width - this.containerWidth) / 2;
            this.yPositionOnScreen = (Game1.viewport.Height - this.containerHeight) / 2;

            // Calculate the height of the computer menu.
            computerMenuHeight = containerHeight - inventoryMenuHeight;

            // Calculate the width of the inventory menu based on the number of slots per row.
            // ReSharper disable ConvertToConstant.Local
            int slotsPerRow = 12; // Assumption: 12 slots per row
            int slotSize = 64; // Size of an inventory slot
            // ReSharper restore ConvertToConstant.Local
            inventoryMenuWidth = slotsPerRow * slotSize;

            // Position of the inventory menu.
            int inventoryMenuX = this.xPositionOnScreen + ((containerWidth - inventoryMenuWidth) / 2);
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
            tabs =
            [
                new ClickableTextureComponent("Terminal", new Rectangle(xPositionOnScreen + 20, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
                //new ClickableTextureComponent("Tab2", new Rectangle(xPositionOnScreen + 88, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
                //new ClickableTextureComponent("Tab3", new Rectangle(xPositionOnScreen + 156, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
                //new ClickableTextureComponent("Tab4", new Rectangle(xPositionOnScreen + 224, yPositionOnScreen - 64, 64, 64), null, null, Game1.mouseCursors, new Rectangle(16, 368, 16, 16), 4f),
            ];

            selectedTab = 0; // Standardmäßig den ersten Reiter auswählen
        }

        /// <summary>Draws the menu and all components.</summary>
        /// <param name="b">The sprite back in which to draw the menu with.</param>
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
                const string title = "ULTIMATE STORAGE SYSTEM";

                // Setze die maximale Breite und den Abstand von rechts
                const float  maxTitleWidth = 400f;
                const float rightPadding  = 30f;

                // Berechnet die Skalierung basierend auf der Breite des Textes
                float scale = Math.Min(1f, maxTitleWidth / Game1.dialogueFont.MeasureString(title).X);

                // Berechnet die Position des Titels, rechtsbündig mit 20px Abstand
                // ReSharper disable once BadIndent
                var titlePosition = new Vector2(
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
                        // ReSharper disable BadIndent, BadParensLineBreaks
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
                        // ReSharper restore BadIndent, BadParensLineBreaks
                    }
                }

                // Zeichnet den Titel fett, rechtsbündig
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        // ReSharper disable BadIndent, BadParensLineBreaks
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
                        // ReSharper restore BadIndent, BadParensLineBreaks
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
                    tabIconPosition = new(xPositionOnScreen + 36, yPositionOnScreen - 40 + yOffset);
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
                    tabIconPosition = new(xPositionOnScreen + 172, yPositionOnScreen - 40 + yOffset);
                }
                else // Index == 3
                {
                    // Hier die Zuweisung von BasketTexture
                    Texture2D? basketTexture = ModEntry.BasketTexture;

                    if (basketTexture is null) continue;
                    sourceRect = new(0, 0, basketTexture.Width, basketTexture.Height);

                    // Position des Basket-Icons anpassen
                    Vector2 basketIconPosition = new(tab.bounds.X + 16, tab.bounds.Y + 22 + yOffset);
                    b.Draw(basketTexture, basketIconPosition, sourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);

                    // Überspringe das Zeichnen des Standard-Icons
                    continue;
                }

                // Zeichne das Icon
                b.Draw(chestIcon, tabIconPosition, sourceRect, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);
            }

            this.drawMouse(b);
        }

        /// <summary>Processes left-clicks on the menu.</summary>
        /// <param name="x">X-coordinate of mouse click.</param>
        /// <param name="y">Y-coordinate of mouse click.</param>
        /// <param name="playSound">Determines if we should play sound.</param>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (ClickableTextureComponent tab in tabs.Where(tab => tab.containsPoint(x, y)))
            {
                selectedTab = tabs.IndexOf(tab); // Setze den ausgewählten Reiter
                if (playSound)
                    Game1.playSound("smallSelect"); // Spiele einen Soundeffekt ab
                return; // Beende die Methode, damit nicht versehentlich das Menü interagiert
            }

            // Nur Aktionen verarbeiten, wenn der erste Tab ausgewählt ist
            if (selectedTab != 0)
                return;

            if (new Rectangle(searchBox.TextBox.X, searchBox.TextBox.Y, searchBox.TextBox.Width, searchBox.TextBox.Height).Contains(x, y))
            {
                searchBox.Click();
            }
            else
            {
                inputHandler.ReceiveLeftClick(x, y, playSound);
                itemTable.ReceiveLeftClick(x, y);

                Item? clickedItem = GetItemAt(x, y, out bool isInInventory);
                if (clickedItem is not null)
                {
                    bool shiftPressed = Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.oldKBState.IsKeyDown(Keys.RightShift);
                    itemTransferManager.HandleLeftClick(clickedItem, isInInventory, shiftPressed);

                    string searchText = searchBox.TextBox.Text.TrimStart(SearchBox.NonBreakingSpace);
                    itemTable.FilterItems(searchText);
                }
            }

            ItemTableRenderer.HandleHeaderClick(x, y, itemTable);
        }

        /// <summary>Processes right-clicks on the menu.</summary>
        /// <param name="x">X-coordinate of mouse click.</param>
        /// <param name="y">Y-coordinate of mouse click.</param>
        /// <param name="playSound">Determines if we should play sound.</param>
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (ModEntry.Instance.IgnoreNextRightClick)
            {
                ModEntry.Instance.IgnoreNextRightClick = false;
                return;
            }

            if (selectedTab != 0)
            {
                return;
            }

            Item? clickedItem = GetItemAt(x, y, out bool isInInventory);

            if (clickedItem is null)
                return;

            bool shiftPressed = Game1.oldKBState.IsKeyDown(Keys.LeftShift) || Game1.oldKBState.IsKeyDown(Keys.RightShift);
            itemTransferManager.HandleRightClick(clickedItem, isInInventory, shiftPressed);

            string searchText = searchBox.TextBox.Text.TrimStart(SearchBox.NonBreakingSpace);
            itemTable.FilterItems(searchText);
        }

        /// <summary>Retrieves the clicked item from the inventory or table.</summary>
        /// <param name="x">X-coordinate of the mouse.</param>
        /// <param name="y">X-coordinate of the mouse.</param>
        /// <param name="isInInventory">Returns a variable determining if the item is already in the player's inventory.</param>
        /// <returns>The fetched item or null if no item is found.</returns>
        private Item? GetItemAt(int x, int y, out bool isInInventory)
        {
            isInInventory = false;

            foreach (ClickableComponent slot in playerInventoryMenu.inventory.Where(slot => slot.containsPoint(x, y) && playerInventoryMenu.actualInventory.Count > slot.myID))
            {
                isInInventory = true;
                return playerInventoryMenu.actualInventory[slot.myID];
            }

            for (int i = 0; i < itemTable.GetVisibleRows(); i++)
            {
                int index = itemTable.ScrollIndex + i;
                if (index >= itemTable.GetItemEntries().Count)
                    break;

                int startX = itemTable.StartX + 40;
                int startY = itemTable.StartY + 100;
                int rowY = startY + (32 * (i + 1)) + 10;
                if (new Rectangle(startX - 20, rowY, 740, 32).Contains(x, y))
                {
                    return itemTable.GetItemEntries()[index].Item;
                }
            }

            return null;
        }

        /// <summary>Processes holding the left mouse button.</summary>
        /// <param name="x">X-coordinate of the mouse.</param>
        /// <param name="y">Y-coordinate of the mouse.</param>
        public override void leftClickHeld(int x, int y)
        {
            inputHandler.LeftClickHeld(x, y);
        }

        /// <summary>Processes releasing the left mouse button.</summary>
        /// <param name="x">X-coordinate of the mouse.</param>
        /// <param name="y">Y-coordinate of the mouse.</param>
        public override void releaseLeftClick(int x, int y)
        {
            inputHandler.ReleaseLeftClick(x, y);
        }

        /// <summary>Processes hover actions.</summary>
        /// <param name="x">X-coordinate of the mouse.</param>
        /// <param name="y">Y-coordinate of the mouse.</param>
        public override void performHoverAction(int x, int y)
        {
            inputHandler.PerformHoverAction(x, y);
            if (selectedTab == 0)
            {
                itemTable.PerformHoverAction(x, y);
            }
        }

        /// <summary>Processes keyboard inputs.</summary>
        /// <param name="key">An enum representing the key on a keyboard.</param>
        public override void receiveKeyPress(Keys key)
        {
            inputHandler.ReceiveKeyPress(key);
        }

        /// <summary>Processes scroll wheel actions.</summary>
        /// <param name="direction">Scrolling up is +1 while down is -1</param>
        /// <remark>The sign of the integer for directions may be backwards.</remark>
        public override void receiveScrollWheelAction(int direction)
        {
            if (selectedTab == 0)
            {
                inputHandler.ReceiveScrollWheelAction(direction);
            }
        }
    }
}

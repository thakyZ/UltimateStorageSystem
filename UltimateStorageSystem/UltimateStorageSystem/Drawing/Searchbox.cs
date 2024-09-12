// SEARCHBOX.CS
// This file implements a search box that allows the user to
// filter the item table based on specific terms.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using UltimateStorageSystem.Interfaces; // Import the interface

#nullable disable

namespace UltimateStorageSystem.Drawing
{
    public class SearchBox
    {
        // Properties of the search box
        public TextBox textBox { get; private set; } // The actual textbox for input
        private readonly IFilterableTable table; // Reference to the table being filtered
        private readonly Scrollbar scrollbar; // Reference to the scrollbar
        private readonly Texture2D whiteTexture; // Simple white texture for drawing the background
        public readonly char nonBreakingSpace = '\u00A0'; // Non-breaking space to prevent empty search queries
        private string previousText; // Variable to store the previous text

        // Constructor, initializes the search box with position and associated table
        public SearchBox(int x, int y, IFilterableTable table, Scrollbar scrollbar)
        {
            this.table = table;
            this.scrollbar = scrollbar;

            // Initializes the textbox with a specific position and size
            textBox = new TextBox(null, null, Game1.smallFont, Color.Blue)
            {
                X = x + 15,  // Offset the search bar 10px to the right
                Y = y + 13,  // Offset the search bar 15px down
                Width = 200, // Width of the search bar
                Height = 40, // Height reduced by 8px (originally 48)
                Text = nonBreakingSpace.ToString()
            };

            textBox.OnEnterPressed += TextBox_OnEnterPressed;
            whiteTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.White });

            // Set the initial keyboard input focus to the textbox
            Game1.keyboardDispatcher.Subscriber = textBox;
            previousText = textBox.Text; // Initialize the previous text
        }

        // Event handler when Enter is pressed in the search box
        private void TextBox_OnEnterPressed(TextBox sender)
        {
            // Updates the filter based on the search text
            string searchText = sender.Text.TrimStart(nonBreakingSpace);
            table.FilterItems(searchText);
            table.ScrollIndex = 0; // Scrolls the view to the top after the search is applied.

            // Also reset the scrollbar to the beginning
            scrollbar.UpdateScrollBarPosition();

            // Apply the current sorting after filtering
            table.SortItemsBy(ItemTableRenderer.GetSortedColumn(), ItemTableRenderer.IsSortAscending());
        }

        // Draws the search box on the screen
        public void Draw(SpriteBatch b)
        {
            // Determine the background and border colors based on the activity of the textbox
            Color backgroundColor = textBox.Selected ? Color.White : Color.LightGray;
            Color borderColor = textBox.Selected ? Color.Orange : Color.Gray;

            // Draw the enlarged background box
            Rectangle backgroundRect = new Rectangle(textBox.X - 10, textBox.Y - 2, 255, textBox.Height + 4);
            b.Draw(whiteTexture, backgroundRect, backgroundColor);

            // Draw the border around the background box
            DrawBorder(b, backgroundRect, 2, borderColor);

            // Draw the background of the textbox
            b.Draw(whiteTexture, new Rectangle(textBox.X, textBox.Y, textBox.Width, textBox.Height), backgroundColor);

            // Draws the text in the textbox
            Vector2 textSize = Game1.smallFont.MeasureString(textBox.Text);
            Vector2 textPosition = new Vector2(textBox.X + 6, textBox.Y + ((textBox.Height - textSize.Y) + 4) / 2);
            b.DrawString(Game1.smallFont, textBox.Text, textPosition, Color.Black);

            // Draws the cursor if the textbox is active
            if (textBox.Selected && (Game1.ticks % 60) < 30)  // Blinks in a 30-tick interval
            {
                float cursorX = textPosition.X + textSize.X;
                if (string.IsNullOrEmpty(textBox.Text.TrimStart(nonBreakingSpace)))
                {
                    cursorX = textBox.X + 6; // Draws the cursor at the beginning if no text is present
                }
                b.Draw(whiteTexture, new Rectangle((int)cursorX, (int)textPosition.Y, 1, (int)textSize.Y), Color.Blue);
            }
        }

        // Helper method for drawing a border around a rectangle
        private void DrawBorder(SpriteBatch spriteBatch, Rectangle rectangle, int thickness, Color color)
        {
            // Top line
            spriteBatch.Draw(whiteTexture, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, thickness), color);

            // Bottom line
            spriteBatch.Draw(whiteTexture, new Rectangle(rectangle.Left, rectangle.Bottom - thickness, rectangle.Width, thickness), color);

            // Left line
            spriteBatch.Draw(whiteTexture, new Rectangle(rectangle.Left, rectangle.Top, thickness, rectangle.Height), color);

            // Right line
            spriteBatch.Draw(whiteTexture, new Rectangle(rectangle.Right - thickness, rectangle.Top, thickness, rectangle.Height), color);
        }

        // Updates the status of the search box to ensure the non-breaking space remains at the beginning
        public void Update()
        {
            // Ensure that the non-breaking space remains at the beginning
            if (!textBox.Text.StartsWith(nonBreakingSpace.ToString()))
            {
                textBox.Text = nonBreakingSpace + textBox.Text.TrimStart();
            }

            // Check if the text has changed and update the filter accordingly
            if (textBox.Text != previousText)
            {
                previousText = textBox.Text;
                string searchText = textBox.Text.TrimStart(nonBreakingSpace);
                table.FilterItems(searchText);
                table.ScrollIndex = 0; // Scrolls the view to the top after the search is applied.

                // Also reset the scrollbar to the beginning
                scrollbar.UpdateScrollBarPosition();

                // Apply the current sorting after filtering
                table.SortItemsBy(ItemTableRenderer.GetSortedColumn(), ItemTableRenderer.IsSortAscending());
            }

            textBox.Update();
        }

        // Processes text input and forwards it to the textbox
        public void RecieveTextInput(char inputChar)
        {
            textBox.RecieveTextInput(inputChar);
        }

        // Draws the magnifying glass icon next to the search box
        public void DrawMagnifyingGlass(SpriteBatch b, Vector2 position)
        {
            // Source of the magnifying glass icon in the cursor sprite sheet
            Rectangle sourceRect = new Rectangle(80, 0, 16, 16); // Example coordinates for the magnifying glass

            // Draws the magnifying glass next to the search box
            float scale = Game1.pixelZoom * 2 / 3f; // Scaling to two-thirds of the size
            b.Draw(
                Game1.mouseCursors, // Cursor sprite sheet
                position,           // Position to draw the magnifying glass
                sourceRect,         // Source rectangle of the magnifying glass
                Color.White,        // Color
                0f,                 // Rotation
                Vector2.Zero,       // Origin
                scale,              // Scaling
                SpriteEffects.None, // Sprite effects
                0f                  // Layer depth
            );
        }

        // Sets focus on the search box and retains the current text
        public void Click()
        {
            // Sets focus on the search box
            Game1.keyboardDispatcher.Subscriber = textBox;
            textBox.Selected = true;
            textBox.Text = textBox.Text; // Retain the current text
        }
    }
}

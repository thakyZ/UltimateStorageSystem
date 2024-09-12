// ITEMTABLERENDERER.CS
// This file contains methods responsible for drawing the columns, rows,
// and headers in the item table.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using UltimateStorageSystem.Utilities;

namespace UltimateStorageSystem.Drawing
{
    public static class ItemTableRenderer
    {
        // Rectangles that define the areas of the column headers
        private static Rectangle itemHeaderRect;
        private static Rectangle qtyHeaderRect;
        private static Rectangle valueHeaderRect;
        private static Rectangle totalHeaderRect;

        // Flags indicating the sort order of the respective columns
        private static bool isItemSortedAscending = true;
        private static bool isQtySortedAscending = false;
        private static bool isValueSortedAscending = false;
        private static bool isTotalSortedAscending = false;

        // The column currently being sorted by
        private static string sortedColumn = "Name"; // Initialized with the column sorted by default

        // Index of the row over which the mouse is hovering
        private static int hoverRowIndex = -1;

        // Limits for the text width in the columns
        private const int MaxItemNameLength = 15;
        private const int MaxQtyLength = 6;
        private const int MaxValueLength = 6;
        private const int MaxTotalLength = 10;

        // Timer control for scrolling text
        private static float scrollTimer = 0f;
        //private static float scrollSpeed = 0.03f; // Scrolling speed

        // Draws the headers of the table
        public static void DrawHeaders(SpriteBatch b, int startX, int startY, ItemTable table)
        {
            int headerY = startY + 100; // Adjusting the Y-position of the headers

            // Draws the headers with icons and stores their positions
            itemHeaderRect = DrawHeaderWithIcon(b, "Item", new Vector2(startX + 40, headerY), Color.White, false, sortedColumn == "Name" ? isItemSortedAscending : (bool?)null, 0);
            qtyHeaderRect = DrawHeaderWithIcon(b, "Qty", new Vector2(startX + 445, headerY), Color.White, true, sortedColumn == "Quantity" ? isQtySortedAscending : (bool?)null, -45);
            valueHeaderRect = DrawHeaderWithIcon(b, "Value", new Vector2(startX + 565, headerY), Color.White, true, sortedColumn == "SingleValue" ? isValueSortedAscending : (bool?)null, -69);
            totalHeaderRect = DrawHeaderWithIcon(b, "Total", new Vector2(startX + 745, headerY), Color.White, true, sortedColumn == "TotalValue" ? isTotalSortedAscending : (bool?)null, -65);

            // Draws a thin line between the column names and the table content
            b.DrawLine(new Vector2(startX + 40, headerY + 35), new Vector2(startX + 750, headerY + 35), Color.White, 1f);
        }

        // Draws a single row in the table
        public static void DrawRow(SpriteBatch b, int startX, int rowY, ItemEntry entry, bool isHovered)
        {
            // 10-pixel spacing between the line and the table content
            rowY += 10;

            // Draws a row highlighter if the mouse is hovering over the row
            if (isHovered)
            {
                b.Draw(Game1.staminaRect, new Rectangle(startX - 20, rowY, 740, 32), Color.White * 0.08f); // Subtle highlight
            }

            Vector2 iconPosition = new Vector2(startX - 20, rowY - 16);

            // Specific adjustments for certain item types
            if (entry.Item is Ring || entry.Item is Boots)
            {
                iconPosition.Y += 10;  // Shift the icon down for rings and boots
                iconPosition.X += 10; // Shift the icon to the right for rings and boots
            }
            else if (entry.Item is StardewValley.Tools.WateringCan)
            {
                iconPosition.Y += 8;  // Shift the icon down for watering cans
            }
            else if (entry.Item.ParentSheetIndex == 597 ||  // Jazz
                     entry.Item.ParentSheetIndex == 593 ||  // Fairy Rose
                     entry.Item.ParentSheetIndex == 376 ||  // Poppy
                     entry.Item.ParentSheetIndex == 595)   // Summer Spangle
            {
                iconPosition.Y += 10;  // Shift the icon down for these items
                iconPosition.X += 10; // Shift the icon to the right for these items
            }

            else if (entry.Item is StardewValley.Object obj &&
                     int.TryParse(obj.preservedParentSheetIndex.Value, out int preservedIndex) &&  // Convert to int
                     preservedIndex != -1)  // Compare the int value
            {
                iconPosition.Y += 10;  // Shift the icon down for these items
                iconPosition.X += 10;  // Shift the icon to the right for these items
            }

            // Draws the item icon in the row
            entry.Item.drawInMenu(b, iconPosition, 0.375f, 1f, 0.86f, StackDrawType.Hide, Color.White, false);

            // Determine color based on the quality level of the item
            Color itemColor = Color.White; // Default: White for normal quality
            if (entry.Item != null)
            {
                switch (entry.Item.Quality)
                {
                    case 1: // Silver
                        itemColor = new Color(169, 169, 169); // Darker gray
                        break;
                    case 2: // Gold
                        itemColor = Color.Gold;
                        break;
                    case 4: // Iridium
                        itemColor = Color.MediumPurple; // Purple for iridium
                        break;
                }
            }

            // Limit and truncate text in columns if necessary
            string itemName = entry.Name.Length > MaxItemNameLength ? entry.Name.Substring(0, MaxItemNameLength) + "..." : entry.Name;
            string quantityText = entry.Quantity.ToString().PadLeft(MaxQtyLength).Substring(0, MaxQtyLength);
            string valueText = entry.SingleValue.ToString().PadLeft(MaxValueLength).Substring(0, MaxValueLength);
            string totalValueText = entry.TotalValue.ToString().PadLeft(MaxTotalLength).Substring(0, MaxTotalLength);

            // Draws the truncated text in the respective columns
            if (isHovered && entry.Name.Length > MaxItemNameLength)
            {
                DrawScrollingText(b, entry.Name, new Vector2(startX + 40, rowY), itemColor, MaxItemNameLength);
            }
            else
            {
                DrawText(b, itemName, new Vector2(startX + 40, rowY), itemColor, alignRight: false);
            }

            if (isHovered && entry.Quantity.ToString().Length > MaxQtyLength)
            {
                DrawScrollingText(b, entry.Quantity.ToString(), new Vector2(startX + 410, rowY), itemColor, MaxQtyLength);
            }
            else
            {
                DrawText(b, quantityText, new Vector2(startX + 410, rowY), itemColor, alignRight: true);
            }

            if (isHovered && entry.SingleValue.ToString().Length > MaxValueLength)
            {
                DrawScrollingText(b, entry.SingleValue.ToString(), new Vector2(startX + 530, rowY), itemColor, MaxValueLength);
            }
            else
            {
                DrawText(b, valueText, new Vector2(startX + 530, rowY), itemColor, alignRight: true);
            }

            if (isHovered && entry.TotalValue.ToString().Length > MaxTotalLength)
            {
                DrawScrollingText(b, entry.TotalValue.ToString(), new Vector2(startX + 710, rowY), itemColor, MaxTotalLength);
            }
            else
            {
                DrawText(b, totalValueText, new Vector2(startX + 710, rowY), itemColor, alignRight: true);
            }
        }

        // Draws the column headers with optional icons
        public static Rectangle DrawHeaderWithIcon(SpriteBatch b, string text, Vector2 position, Color color, bool alignRight, bool? isAscending, int iconOffsetX)
        {
            Vector2 textSize = Game1.smallFont.MeasureString(text);
            DrawText(b, text, position, color, alignRight);

            float scale = 0.5f;
            int clickableWidth = (int)(textSize.X + 10);

            // Draws a sort icon if the column is currently being sorted
            if (isAscending.HasValue)
            {
                Vector2 iconPosition = position;
                if (alignRight)
                {
                    iconPosition.X += textSize.X + iconOffsetX + 5;
                }
                else
                {
                    iconPosition.X += textSize.X + iconOffsetX + 5;
                }
                iconPosition.Y += 5;

                Rectangle sourceRect = isAscending.Value ? new Rectangle(421, 472, 12, 12) : new Rectangle(421, 459, 12, 12);
                b.Draw(Game1.mouseCursors, iconPosition, sourceRect, Color.White, 0f, Vector2.Zero, scale * Game1.pixelZoom, SpriteEffects.None, 0f);

                clickableWidth += (int)(sourceRect.Width * scale * Game1.pixelZoom);
            }

            return new Rectangle((int)(alignRight ? position.X - textSize.X : position.X), (int)position.Y, clickableWidth, (int)(textSize.Y));
        }

        // Draws text in the table
        private static void DrawText(SpriteBatch b, string text, Vector2 position, Color color, bool alignRight)
        {
            Vector2 textSize = Game1.smallFont.MeasureString(text);
            if (alignRight)
            {
                position.X -= textSize.X;
            }
            b.DrawString(Game1.smallFont, text, position, color);
        }

        private static void DrawScrollingText(SpriteBatch b, string fullText, Vector2 position, Color color, int maxVisibleLength)
        {
            // Calculate the length of the text
            int textLength = fullText.Length;

            // If the text is shorter or equal to the maximum visible length, no scrolling is needed
            if (textLength <= maxVisibleLength)
            {
                b.DrawString(Game1.smallFont, fullText, position, color);
                return;
            }

            // Maximum number of steps the text needs to scroll
            int maxScrollIndex = textLength - maxVisibleLength;

            // Increment the timer gradually to control the speed (now twice as fast)
            scrollTimer += (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250; // Speed doubled

            // Calculate the current scroll index
            int scrollIndex = (int)scrollTimer % (maxScrollIndex * 2);

            // Scroll backwards when the end is reached
            if (scrollIndex > maxScrollIndex)
            {
                scrollIndex = maxScrollIndex * 2 - scrollIndex;
            }

            // Determine the visible part of the text
            string visibleText = fullText.Substring(scrollIndex, maxVisibleLength);

            // Draw the visible text
            b.DrawString(Game1.smallFont, visibleText, position, color);
        }

        // Handles clicks on the table headers and sorts the table accordingly
        public static void HandleHeaderClick(int x, int y, ItemTable table)
        {
            // Checks if the item header was clicked
            if (itemHeaderRect.Contains(x, y))
            {
                isItemSortedAscending = !isItemSortedAscending;
                sortedColumn = "Name";
                table.SortItemsBy("Name", isItemSortedAscending);
            }
            else if (qtyHeaderRect.Contains(x, y))
            {
                isQtySortedAscending = !isQtySortedAscending;
                sortedColumn = "Quantity";
                table.SortItemsBy("Quantity", isQtySortedAscending);
            }
            else if (valueHeaderRect.Contains(x, y))
            {
                isValueSortedAscending = !isValueSortedAscending;
                sortedColumn = "SingleValue";
                table.SortItemsBy("SingleValue", isValueSortedAscending);
            }
            else if (totalHeaderRect.Contains(x, y))
            {
                isTotalSortedAscending = !isTotalSortedAscending;
                sortedColumn = "TotalValue";
                table.SortItemsBy("TotalValue", isTotalSortedAscending);
            }
        }

        // Processes hover actions and determines the row over which the mouse is hovering
        public static void PerformHoverAction(int x, int y, ItemTable table)
        {
            // Initialize hoverRowIndex to -1 to signal that the mouse is not over a row
            hoverRowIndex = -1;

            // Update hoverRowIndex based on the current mouse position
            for (int i = 0; i < table.GetVisibleRows(); i++)
            {
                int index = table.ScrollIndex + i;
                if (index >= table.GetItemEntries().Count)
                {
                    break;
                }

                int startX = table.StartX + 40;
                int startY = table.StartY + 100;
                int rowY = startY + 32 * (i + 1) + 10;
                if (new Rectangle(startX - 20, rowY, 740, 32).Contains(x, y))
                {
                    hoverRowIndex = i;
                    break;
                }
            }
        }

        // Returns whether the mouse is hovering over a specific row
        public static bool IsHoveringOverRow(int rowIndex)
        {
            return rowIndex == hoverRowIndex;
        }

        // Sets the current sort state of the table
        public static void SetSortState(string column, bool isAscending)
        {
            sortedColumn = column;
            switch (column)
            {
                case "Name":
                    isItemSortedAscending = isAscending;
                    break;
                case "Quantity":
                    isQtySortedAscending = isAscending;
                    break;
                case "SingleValue":
                    isValueSortedAscending = isAscending;
                    break;
                case "TotalValue":
                    isTotalSortedAscending = isAscending;
                    break;
            }
        }

        // Returns the currently sorted column
        public static string GetSortedColumn()
        {
            return sortedColumn;
        }

        // Returns whether the current sorting is ascending
        public static bool IsSortAscending()
        {
            return sortedColumn switch
            {
                "Name" => isItemSortedAscending,
                "Quantity" => isQtySortedAscending,
                "SingleValue" => isValueSortedAscending,
                "TotalValue" => isTotalSortedAscending,
                _ => true,
            };
        }
    }
}

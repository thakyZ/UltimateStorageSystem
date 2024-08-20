// SPRITEBATCHEXTENSIONS.CS
// This file contains extension methods for the SpriteBatch object,
// including a method for drawing lines.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

#nullable disable

namespace UltimateStorageSystem.Drawing
{
    public static class SpriteBatchExtensions
    {
        // This method draws a line on the screen.
        // Parameters:
        // - spriteBatch: The SpriteBatch object used for drawing.
        // - start: The start point of the line as a Vector2.
        // - end: The end point of the line as a Vector2.
        // - color: The color of the line.
        // - thickness: The thickness of the line.
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
        {
            // Calculate the length of the line
            float length = Vector2.Distance(start, end);
            // Calculate the rotation of the line based on the start and end points
            float rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

            // Draw the line with the given color and thickness
            spriteBatch.Draw(
                Game1.staminaRect, // Use the staminaRect texture from Stardew Valley as a placeholder
                start,             // Start point of the line
                null,              // No source rectangle, the entire texture is used
                color,             // Color of the line
                rotation,          // Rotation of the line
                Vector2.Zero,      // Origin around which the line is rotated
                new Vector2(length, thickness), // Scaling of the line based on its length and thickness
                SpriteEffects.None, // No special sprite effects
                0                  // Layer depth, 0 means the line is drawn in front of other objects
            );
        }
    }
}

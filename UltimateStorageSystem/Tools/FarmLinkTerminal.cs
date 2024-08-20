// FARMLINKTERMINAL.CS
// This file contains a method that checks if the player is standing 
// below a specific tile and is facing upward.

using StardewValley;
using Microsoft.Xna.Framework;

#nullable disable

namespace UltimateStorageSystem.Tools
{
    public static class FarmLinkTerminal
    {
        // Method to check if the player is standing below a specific tile and facing upward
        public static bool IsPlayerBelowTileAndFacingUp(Farmer player, Vector2 tile)
        {
            // Calculate the tile position of the player
            int playerTileX = (int)((player.Position.X + Game1.tileSize / 2 + player.Sprite.SpriteWidth / 2) / Game1.tileSize);
            int playerTileY = (int)((player.Position.Y + player.Sprite.SpriteHeight / 2) / Game1.tileSize);
            Vector2 playerTile = new(playerTileX, playerTileY);

            // Check if the player is facing upward
            bool isFacingUp = player.FacingDirection == 0;

            // Check if the player is directly below the terminal
            bool isBelowTile = playerTile == tile + new Vector2(0, 1);

            // Returns true if the player is below the terminal and facing upward
            return isFacingUp && isBelowTile;
        }
    }
}

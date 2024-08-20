// ITEMTRANSFERMANAGER.CS
// This file manages the transfer of items between the player's inventory
// and various chests in the game. It handles item stacking, transferring,
// and sorting within the FarmLink Terminal system.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using UltimateStorageSystem.Drawing;
using UltimateStorageSystem.Utilities;

#nullable disable

namespace UltimateStorageSystem.Tools
{
    public class ItemTransferManager
    {
        private readonly List<Chest> chests;
        private readonly ItemTable itemTable;

        // Constructor to initialize the ItemTransferManager with a list of chests and an item table
        public ItemTransferManager(List<Chest> chests, ItemTable itemTable)
        {
            this.chests = chests;
            this.itemTable = itemTable;
        }

        // Updated HandleLeftClick method to transfer up to a full stack and, with Shift+Click, transfer up to half the stack
        public void HandleLeftClick(Item item, bool isInInventory, bool shiftPressed)
        {
            int amountToTransfer;

            if (item is Furniture furniture)
            {
                // Check if the furniture is in a chest
                if (!isInInventory)
                {
                    // Find the chest containing the furniture
                    Chest sourceChest = chests.FirstOrDefault(chest => chest.Items.Contains(furniture));
                    if (sourceChest != null)
                    {
                        // Temporarily move the furniture out of the chest
                        Vector2 tempPosition = new Vector2(-1000, -1000); // An invalid position outside the visible area
                        sourceChest.Items.Remove(furniture);

                        // Set position to remove it from the chest
                        furniture.TileLocation = tempPosition;

                        // Transfer the furniture to the player's inventory
                        Game1.player.addItemToInventory(furniture);
                    }
                }
                else
                {
                    // Standard transfer from inventory to chests
                    TransferFromInventoryToChests(item, item.Stack);
                }
            }
            else
            {
                var entry = itemTable.GetItemEntries().FirstOrDefault(e => e.Item == item);
                if (entry != null)
                {
                    int maxStackSize = item.maximumStackSize();
                    int stackSize = shiftPressed ? maxStackSize / 2 : maxStackSize;
                    amountToTransfer = Math.Min(entry.Quantity, stackSize);
                }
                else
                {
                    amountToTransfer = shiftPressed ? item.Stack / 2 : item.Stack;
                }

                if (isInInventory)
                {
                    TransferFromInventoryToChests(item, amountToTransfer);
                }
                else
                {
                    TransferFromChestsToInventory(item, amountToTransfer);
                }
            }

            UpdateChestItemsAndSort();
            itemTable.Refresh();
        }

        // Updated HandleRightClick method to transfer up to a full stack and, with Shift+Click, transfer up to half the stack
        public void HandleRightClick(Item item, bool isInInventory, bool shiftPressed)
        {
            int amountToTransfer = shiftPressed ? 10 : 1;

            var entry = itemTable.GetItemEntries().FirstOrDefault(e => e.Item == item);
            if (entry != null)
            {
                int maxStackSize = item.maximumStackSize();
                amountToTransfer = Math.Min(amountToTransfer, Math.Min(entry.Quantity, maxStackSize - 1));
            }

            if (item is Furniture furniture)
            {
                if (!isInInventory)
                {
                    Chest sourceChest = chests.FirstOrDefault(chest => chest.Items.Contains(furniture));
                    if (sourceChest != null)
                    {
                        Vector2 tempPosition = new Vector2(-1000, -1000);
                        sourceChest.Items.Remove(furniture);
                        furniture.TileLocation = tempPosition;
                        Game1.player.addItemToInventory(furniture);
                    }
                }
                else
                {
                    TransferFromInventoryToChests(item, amountToTransfer);
                }
            }
            else
            {
                if (isInInventory)
                {
                    TransferFromInventoryToChests(item, amountToTransfer);
                }
                else
                {
                    TransferFromChestsToInventory(item, amountToTransfer);
                }
            }

            UpdateChestItemsAndSort();
            itemTable.Refresh();
        }

        // Transfers items from chests to the player's inventory
        private void TransferFromChestsToInventory(Item item, int amount)
        {
            List<Item> collectedItems = CollectItemsFromChests(item, amount);

            foreach (var collectedItem in collectedItems)
            {
                Game1.player.addItemToInventory(collectedItem);
            }
            UpdateChestItemsAndSort();
        }

        // Transfers items from the player's inventory to chests
        private void TransferFromInventoryToChests(Item item, int amount)
        {
            int remainingAmount = amount;

            // Sort the chests by the number of existing items in descending order
            var sortedChests = chests
                .Select(chest => new
                {
                    Chest = chest,
                    ItemCount = chest.Items.Where(i => i != null && i.canStackWith(item)).Sum(i => i.Stack)
                })
                .Where(chestInfo => chestInfo.ItemCount > 0) // Only chests with the item type
                .OrderByDescending(chestInfo => chestInfo.ItemCount)
                .ToList();

            foreach (var chestInfo in sortedChests)
            {
                Chest chest = chestInfo.Chest;

                // Transfer the items to the chest if there is space
                Item remainingItem = item.getOne();
                remainingItem.Stack = remainingAmount;
                Item addedItem = chest.addItem(remainingItem);

                // Determine the amount actually transferred
                if (addedItem == null)
                {
                    remainingAmount = 0;
                }
                else
                {
                    remainingAmount = addedItem.Stack;
                }

                int transferredAmount = amount - remainingAmount;
                item.Stack -= transferredAmount;

                if (item.Stack <= 0)
                {
                    Game1.player.removeItemFromInventory(item);
                    break;
                }

                // Update the remaining amount
                remainingAmount = amount - transferredAmount;
                if (remainingAmount <= 0)
                {
                    break;
                }
            }

            // If not all items could be transferred, they remain in the inventory and a notification is displayed
            if (remainingAmount > 0)
            {
                Game1.addHUDMessage(new HUDMessage(ModEntry.Instance.Helper.Translation.Get("no_storage_space_message"), 3));
            }

            UpdateChestItemsAndSort();
        }

        // Method to collect all items of a type from the chests
        private List<Item> CollectItemsFromChests(Item item, int amount)
        {
            List<Item> collectedItems = new List<Item>();
            int remainingAmount = amount;

            // Sort the chests by the number of existing items in ascending order
            var sortedChests = chests
                .Select(chest => new
                {
                    Chest = chest,
                    ItemCount = chest.Items.Where(i => i != null && i.canStackWith(item)).Sum(i => i.Stack)
                })
                .Where(chestInfo => chestInfo.ItemCount > 0) // Only chests with the item type
                .OrderBy(chest => chest.ItemCount)
                .ToList();

            foreach (var chestInfo in sortedChests)
            {
                var chest = chestInfo.Chest;

                for (int i = chest.Items.Count - 1; i >= 0; i--)
                {
                    Item chestItem = chest.Items[i];

                    if (chestItem != null && chestItem.canStackWith(item))
                    {
                        int transferAmount = Math.Min(chestItem.Stack, remainingAmount);

                        // Ensure that at least one item remains in the chest if it is the last chest
                        if (chestItem.Stack - transferAmount <= 0 && chestItem.Stack > 1 && chest == sortedChests.Last().Chest)
                        {
                            transferAmount = chestItem.Stack - 1;
                        }

                        remainingAmount -= transferAmount;

                        Item collectedItem = chestItem.getOne();
                        collectedItem.Stack = transferAmount;
                        collectedItems.Add(collectedItem);

                        chestItem.Stack -= transferAmount;
                        if (chestItem.Stack <= 0)
                        {
                            chest.Items.RemoveAt(i);
                        }

                        if (remainingAmount <= 0)
                        {
                            break;
                        }
                    }
                }

                if (remainingAmount <= 0)
                {
                    break;
                }
            }

            return collectedItems;
        }

        // Method to update the items in the chests and apply sorting
        public void UpdateChestItemsAndSort()
        {
            itemTable.ClearItems();

            Dictionary<string, ItemEntry> groupedItems = new Dictionary<string, ItemEntry>();

            // Search each chest and group the items
            foreach (var chest in chests)
            {
                foreach (var item in chest.Items)
                {
                    if (item != null)
                    {
                        string key = $"{item.DisplayName}_{item.Category}_{item.Quality}_{item.ParentSheetIndex}";

                        if (groupedItems.ContainsKey(key))
                        {
                            groupedItems[key].Quantity += item.Stack;
                            groupedItems[key].TotalValue += Math.Max(0, item.salePrice()) * item.Stack;
                        }
                        else
                        {
                            groupedItems[key] = new ItemEntry(
                                item.DisplayName,
                                item.Stack,
                                Math.Max(0, item.salePrice()),
                                Math.Max(0, item.salePrice()) * item.Stack,
                                item
                            );
                        }
                    }
                }
            }

            // Add the grouped items to the table
            var sortedItems = ItemSorting.SortItems(
                groupedItems.Values.ToList(),
                ItemTableRenderer.GetSortedColumn(),
                ItemTableRenderer.IsSortAscending()
            );

            foreach (var entry in sortedItems)
            {
                itemTable.AddItem(entry);
            }

            // Refresh the table after adding items
            itemTable.Refresh();
        }
    }
}

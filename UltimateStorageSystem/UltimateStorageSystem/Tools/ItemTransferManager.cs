// ITEMTRANSFERMANAGER.CS
// Diese Datei verwaltet den Transfer von Gegenständen zwischen dem Inventar des Spielers
// und verschiedenen Truhen im Spiel. Sie behandelt das Stapeln von Gegenständen, das Übertragen
// und Sortieren innerhalb des FarmLink-Terminals.

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

        // Konstruktor zur Initialisierung des ItemTransferManagers mit einer Liste von Kisten und einer Item-Tabelle
        public ItemTransferManager(List<Chest> chests, ItemTable itemTable)
        {
            this.chests = chests;
            this.itemTable = itemTable;
        }

        // Methode zum Aktualisieren der Gegenstände in den Truhen und zum Anwenden der Sortierung
        public void UpdateChestItemsAndSort()
        {
            itemTable.ClearItems();

            Dictionary<string, ItemEntry> groupedItems = new Dictionary<string, ItemEntry>();

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

            var sortedItems = groupedItems.Values.ToList();
            sortedItems = ItemSorting.SortItems(sortedItems, ItemTableRenderer.GetSortedColumn(), ItemTableRenderer.IsSortAscending());

            foreach (var entry in sortedItems)
            {
                itemTable.AddItem(entry);
            }

            itemTable.Refresh();
        }

        // Methode zum Sammeln aller Gegenstände eines Typs aus den Truhen
        private List<Item> CollectItemsFromChests(Item item, int amount)
        {
            List<Item> collectedItems = new List<Item>();
            int remainingAmount = amount;

            // Sortiere die Truhen nach der Anzahl vorhandener Gegenstände in aufsteigender Reihenfolge
            var sortedChests = chests
                .Select(chest => new
                {
                    Chest = chest,
                    ItemCount = chest.Items.Where(i => i != null && i.canStackWith(item)).Sum(i => i.Stack)
                })
                .Where(chestInfo => chestInfo.ItemCount > 0) // Nur Truhen mit dem Gegenstandstyp
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

                        // Sicherstellen, dass mindestens ein Item verbleibt, wenn es die letzte Truhe ist
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

        // Methode zum Übertragen von Gegenständen aus dem Inventar des Spielers in Truhen
        private void TransferFromInventoryToChests(Item item, int amount)
        {
            int remainingAmount = amount;

            // Sortiere die Truhen nach der Anzahl vorhandener Gegenstände in absteigender Reihenfolge
            var sortedChests = chests
                .Select(chest => new
                {
                    Chest = chest,
                    ItemCount = chest.Items.Where(i => i != null && i.canStackWith(item)).Sum(i => i.Stack)
                })
                .Where(chestInfo => chestInfo.ItemCount > 0) // Nur Truhen mit dem Item-Typ
                .OrderByDescending(chestInfo => chestInfo.ItemCount)
                .ToList();

            foreach (var chestInfo in sortedChests)
            {
                Chest chest = chestInfo.Chest;

                // Übertrage die Items in die Truhe, falls Platz vorhanden ist
                Item remainingItem = item.getOne();
                remainingItem.Stack = remainingAmount;
                Item addedItem = chest.addItem(remainingItem);

                // Bestimmen der tatsächlich übertragenen Menge
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

                // Aktualisieren der verbleibenden Menge
                remainingAmount = amount - transferredAmount;
                if (remainingAmount <= 0)
                {
                    break;
                }
            }

            // Wenn nicht alle Items übertragen werden konnten, verbleiben sie im Inventar, und eine Benachrichtigung wird angezeigt
            if (remainingAmount > 0)
            {
                Game1.addHUDMessage(new HUDMessage(ModEntry.Instance.Helper.Translation.Get("no_storage_space_message"), 3));
            }

            UpdateChestItemsAndSort();
        }

        // Methode zum Übertragen von Gegenständen aus Truhen ins Inventar des Spielers
        private void TransferFromChestsToInventory(Item item, int amount)
        {
            List<Item> collectedItems = CollectItemsFromChests(item, amount);

            foreach (var collectedItem in collectedItems)
            {
                Game1.player.addItemToInventory(collectedItem);
            }
            UpdateChestItemsAndSort();
        }

        // Methode zur Verarbeitung eines Links-Klicks
        public void HandleLeftClick(Item item, bool isInInventory, bool shiftPressed)
        {
            int amountToTransfer;

            if (item is Furniture furniture)
            {
                if (!isInInventory)
                {
                    // Suche die Truhe, die das Möbelstück enthält
                    Chest sourceChest = chests.FirstOrDefault(chest => chest.Items.Contains(furniture));
                    if (sourceChest != null)
                    {
                        // Temporäres Verschieben des Möbelstücks aus der Truhe
                        Vector2 tempPosition = new Vector2(-1000, -1000);
                        sourceChest.Items.Remove(furniture);

                        // Setze die Position, um es aus der Truhe zu entfernen
                        furniture.TileLocation = tempPosition;

                        // Übertrage das Möbelstück ins Inventar des Spielers
                        Game1.player.addItemToInventory(furniture);
                    }
                }
                else
                {
                    // Standardübertragung vom Inventar in die Truhen
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

        // Methode zur Verarbeitung eines Rechts-Klicks
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
    }
}

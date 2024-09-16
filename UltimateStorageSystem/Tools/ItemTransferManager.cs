// ITEMTRANSFERMANAGER.CS
// Diese Datei verwaltet den Transfer von Gegenständen zwischen dem Inventar des Spielers
// und verschiedenen Truhen im Spiel. Sie behandelt das Stapeln von Gegenständen, das Übertragen
// und Sortieren innerhalb des FarmLink-Terminals.

using StardewValley.Objects;
using StardewValley.Tools;
using UltimateStorageSystem.Drawing;

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

            Dictionary<string, ItemEntry> groupedItems = [];

            foreach (var chest in chests)
            {
                foreach (var item in chest.Items)
                {
                    if (item is not null)
                    {
                        string key = $"{item.DisplayName}_{item.Category}_{item.Quality}_{item.ParentSheetIndex}";

                        int itemPrice = item.sellToStorePrice(-1L); // Verkaufspreis entsprechend dem Vanilla-Inventar

                        if (groupedItems.ContainsKey(key))
                        {
                            groupedItems[key].Quantity += item.Stack;
                            groupedItems[key].TotalValue += Math.Max(0, itemPrice) * item.Stack;
                        }
                        else
                        {
                            groupedItems[key] = new ItemEntry(
                                item.DisplayName,
                                item.Stack,
                                Math.Max(0, itemPrice),
                                Math.Max(0, itemPrice) * item.Stack,
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
            List<Item> collectedItems = [];
            int remainingAmount = amount;

            // Sortiere die Truhen nach der Anzahl vorhandener Gegenstände in aufsteigender Reihenfolge
            var sortedChests = chests
                .Select(chest => new
                {
                    Chest = chest,
                    ItemCount = chest.Items.Where(i => i?.canStackWith(item) == true).Sum(i => i.Stack)
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

                    if (chestItem?.canStackWith(item) == true)
                    {
                        int transferAmount = Math.Min(chestItem.Stack, remainingAmount);

                        // Sicherstellen, dass mindestens ein Item verbleibt, wenn es die letzte Truhe ist
                        // chestItem.Stack - transferAmount <= 0
                        if (chestItem.Stack <= transferAmount && chestItem.Stack > 1 && chest == sortedChests[^1].Chest)
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
                    ItemCount = chest.Items.Where(i => i?.canStackWith(item) == true).Sum(i => i.Stack)
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
                if (addedItem is null)
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
                Game1.addHUDMessage(new HUDMessage(I18n.Message_Warning_NoStorageSpace(), 3));
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

            if (item is Furniture || item is Ring || item is MeleeWeapon || item is Tool || item is Boots)
            {
                if (!isInInventory)
                {
                    // Suche die Truhe, die den Ausrüstungsgegenstand enthält
                    Chest? sourceChest = chests.Find(chest => chest.Items.Contains(item));
                    if (sourceChest is not null)
                    {
                        // Temporäres Verschieben des Gegenstands aus der Truhe
                        sourceChest.Items.Remove(item);

                        // Übertrage den Gegenstand ins Inventar des Spielers
                        Game1.player.addItemToInventory(item);
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
                if (isInInventory)
                {
                    // Bei Shift-Klick: Übertrage die Hälfte des Stacks ins Terminal
                    amountToTransfer = shiftPressed ? Math.Max(1, item.Stack / 2) : item.Stack;
                    TransferFromInventoryToChests(item, amountToTransfer);
                }
                else
                {
                    // Hier ist der Code für den Linksklick mit Umschalttaste zu prüfen
                    var entry = itemTable.GetItemEntries().Find(e => e.Item == item);
                    if (entry is not null)
                    {
                        int maxStackSize = item.maximumStackSize();
                        int stackSize = Math.Min(maxStackSize / 2, entry.Quantity / 2); // Maximale Größe ist ein halbes Stack oder die Hälfte der verfügbaren Menge
                        amountToTransfer = shiftPressed ? stackSize : Math.Min(entry.Quantity, maxStackSize - 1);
                    }
                    else
                    {
                        amountToTransfer = shiftPressed ? Math.Max(1, item.Stack / 2) : item.Stack;
                    }

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

            var entry = itemTable.GetItemEntries().Find(e => e.Item == item);
            if (entry is not null)
            {
                int maxStackSize = item.maximumStackSize();
                amountToTransfer = Math.Min(amountToTransfer, Math.Min(entry.Quantity, maxStackSize - 1));
            }

            if (item is Furniture || item is Ring || item is MeleeWeapon || item is Tool || item is Boots)
            {
                if (!isInInventory)
                {
                    Chest? sourceChest = chests.Find(chest => chest.Items.Contains(item));
                    if (sourceChest is not null)
                    {
                        sourceChest.Items.Remove(item);
                        Game1.player.addItemToInventory(item);
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

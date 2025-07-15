using GTANetworkAPI;
using NeptuneEVO.SDK;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeptuneEVO.Core
{
    public class Bag : Script
    {
        private static int BPSlots = 10;
        public static int BPWeight = 8;

        [RemoteEvent("server::setbackpack")]
        public static void OpenBackPack(Player player)
        {
            try
            {
                nItem item = GetBackpack(player);
                if (item != null)
                    Open(player, item);
            }
            catch (Exception e) { Console.WriteLine("ClientEvent_OPENBAG: " + e.ToString()); }
        }

        public static void Open(Player player, nItem backpack, List<nItem> itemshave = null)
        {
            try
            {
                if (player.HasData("OPENOUT_TYPE") && player.GetData<int>("OPENOUT_TYPE") > 1 && player.GetData<int>("OPENOUT_TYPE") < 10) return;
                List<nItem> items = itemshave == null ? Bag.GetItems(backpack) : itemshave;
                player.SetData("OPENOUT_TYPE", 10);

                InvInterface.OpenOut(player, items, "Рюкзак", 10, BPWeight);
            }
            catch (Exception e) { Console.WriteLine("OPENBAG: " + e.ToString()); }
        }
        #region Get / Set
        public static List<nItem> GetItems(nItem backpack)
        {
            if (backpack.subData == null)
                backpack.subData = new List<nItem>();

            return backpack.subData is JArray ? JArray.FromObject(backpack.subData).ToObject<List<nItem>>() : (List<nItem>)backpack.subData;
        }

        public static void SetItems(nItem backpack, List<nItem> items)
        {
            backpack.subData = items;
        }
        public static nItem GetBackpack(Player player)
        {
            if (player == null || !Main.Players.ContainsKey(player) || !nInventory.Items.ContainsKey(Main.Players[player].UUID)) return null;

            List<nItem> items = nInventory.Items[Main.Players[player].UUID];
            nItem item = items.FirstOrDefault(i => i.Type == ItemType.Bag && i.IsActive);

            return item;
        }
        #endregion

        #region Add / Remove / TryAdd
        public static List<nItem> Add(nItem backpack, nItem item)
        {
            List<nItem> items = Bag.GetItems(backpack);

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type)
                || nInventory.MeleeWeaponsItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                items.Add(item);
            }
            else
            {
                int count = item.Count;
                for (int i = 0; i < items.Count; i++)
                {
                    if (i >= items.Count) break;
                    if (items[i].Type == item.Type && items[i].Count < nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks)
                    {
                        int temp = nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks - items[i].Count;
                        if (count < temp) temp = count;
                        items[i].Count += temp;
                        count -= temp;
                    }
                }

                while (count > 0)
                {
                    if (count >= nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks)
                    {
                        items.Add(new nItem(item.Type, nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks, item.Data));
                        count -= nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks;
                    }
                    else
                    {
                        items.Add(new nItem(item.Type, count, item.Data));
                        count = 0;
                    }
                }
            }

            Bag.SetItems(backpack, items);
            return items;
        }

        public static int TryAdd(nItem backpack, nItem item)
        {
            List<nItem> items = Bag.GetItems(backpack);

            int tail = 0;
            if (nInventory.IsFullWeight(items, BPWeight, item))
                return -1;

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) ||
                item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                if (items.Count >= BPSlots) return -1;
            }
            else
            {
                int count = 0;
                foreach (nItem i in items)
                    if (i.Type == item.Type) count += nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks - i.Count;

                int slots = BPSlots;
                int maxCapacity = (slots - items.Count) * nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks + count;
                if (item.Count > maxCapacity) tail = item.Count - maxCapacity;
            }
            return tail;
        }

        public static int GetCountOfType(nItem backpack, ItemType type)
        {
            List<nItem> items = Bag.GetItems(backpack);
            int count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (i >= items.Count) break;
                if (items[i].Type == type) count += items[i].Count;
            }

            return count;
        }

        public static List<nItem> Remove(nItem backpack, ItemType type, int amount)
        {
            List<nItem> items = Bag.GetItems(backpack);

            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (i >= items.Count) continue;
                if (items[i].Type != type) continue;
                if (items[i].Count <= amount)
                {
                    amount -= items[i].Count;
                    items.RemoveAt(i);
                }
                else
                {
                    items[i].Count -= amount;
                    amount = 0;
                    break;
                }
            }


            Bag.SetItems(backpack, items);
            return items;
        }

        public static List<nItem> Remove(nItem backpack, nItem item)
        {
            List<nItem> items = Bag.GetItems(backpack);

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) ||
                item.Type == ItemType.BagWithDrill || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing)
            {
                items.Remove(item);
            }
            else
            {
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (i >= items.Count) continue;
                    if (items[i].Type != item.Type) continue;
                    if (items[i].Count <= item.Count)
                    {
                        item.Count -= items[i].Count;
                        items.RemoveAt(i);
                    }
                    else
                    {
                        items[i].Count -= item.Count;
                        item.Count = 0;
                        break;
                    }
                }
            }

            Bag.SetItems(backpack, items);
            return items;
        }
#endregion
    }
}

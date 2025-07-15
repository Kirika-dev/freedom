using GTANetworkAPI;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;

namespace NeptuneEVO.Core
{
    public class FastSlots : Script
    {
        public static List<ItemType> IgnoreItems = new List<ItemType>()
        {
            ItemType.IDCard,
            ItemType.NumberPlate,
            ItemType.PopIt,
        };
        [RemoteEvent("server::inventory:setslot")]
        public static void SetItemToSlot(Player player, int index, int slot)
        {
            try
            {
                if (nInventory.Items[Main.Players[player].UUID].Find(x => x.FastSlots == slot) != null) 
                {
                    Notify.Error(player, "Неудалось поставить предмет в быстрый слот, занят");
                    return;
                }
                if (nInventory.Items[Main.Players[player].UUID][index] == null) {
                    Notify.Error(player, "Неудалось поставить предмет в быстрый слот");
                    return;
                }
                nItem item = nInventory.Items[Main.Players[player].UUID][index];
                if (item.IsActive)
                {
                    Notify.Info(player, "Сначала уберите предмет из рук");
                    return;
                }
                if (nInventory.ClothesItems.Contains(item.Type) || nInventory.AmmoItems.Contains(item.Type) || IgnoreItems.Contains(item.Type)) return;
                item.FastSlots = slot;
                InvInterface.sendItems(player);
            }
            catch (Exception e) { Console.WriteLine("RemoveItemFromSlot: " + e.ToString()); }
        }
        [RemoteEvent("server::inventory:removeslot")]
        public static void RemoveItemFromSlot(Player player, int index)
        {
            try
            {
                if (nInventory.Items[Main.Players[player].UUID][index] == null) return;
                nItem item = nInventory.Items[Main.Players[player].UUID][index];
                item.FastSlots = -1;
                InvInterface.sendItems(player);
            }
            catch (Exception e) { Console.WriteLine("RemoveItemFromSlot: " + e.ToString()); }
        }
        public static void RemoveAllFastSlots(Player player)
        {
            try
            {
                var items = nInventory.Items[Main.Players[player].UUID];
                List<nItem> ItemsRemove = new List<nItem>();
                foreach (nItem item in items)
                {
                    if (item.FastSlots > 0)
                    {
                        ItemsRemove.Add(item);
                        Items.onDrop(player, item, null);
                    }
                }
                foreach(nItem item in ItemsRemove)
                {
                    nInventory.Remove(player, item);
                }
            }
            catch (Exception e) { Console.WriteLine("RemoveAllFastSlots: " + e.ToString()); }
        }
        public static int GetIndex(Player player, nItem item, int slot)
        {
            int i = 0;
            foreach(nItem it in nInventory.Items[Main.Players[player].UUID])
            {
                if (it == nInventory.Items[Main.Players[player].UUID].Find(x => x.FastSlots == slot))
                    return i;
                i++;
            }
            return -1;
        } 
        [RemoteEvent("server::inventory:useitemslot")]
        public static void UseItemFromSlot(Player player, int slot)
        {
            try
            {
                if (nInventory.Items[Main.Players[player].UUID].Find(x => x.FastSlots == slot) == null) return;
                nItem item = nInventory.Items[Main.Players[player].UUID].Find(x => x.FastSlots == slot);
                if (nInventory.ClothesItems.Contains(item.Type) || nInventory.AmmoItems.Contains(item.Type) || IgnoreItems.Contains(item.Type)) return;
                Items.onUse(player, item, GetIndex(player, item, slot));
                InvInterface.sendItems(player);
            }
            catch (Exception e) { Console.WriteLine("UseItemFromSlot: " + e.ToString()); }
        } 
        [RemoteEvent("server::inventory:swap")]
        public static void SwapItemFromSlot(Player player)
        {
            try
            {
                foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
                {
                    if (item.IsActive && item.FastSlots > 0)
                    {
                        Items.onUse(player, item, GetIndex(player, item, item.FastSlots));
                    } 
                }
                InvInterface.sendItems(player);
            }
            catch (Exception e) { Console.WriteLine("SwapItemFromSlot: " + e.ToString()); }
        }
    }
}

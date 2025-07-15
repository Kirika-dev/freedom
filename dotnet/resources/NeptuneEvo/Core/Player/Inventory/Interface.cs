using GTANetworkAPI;
using NeptuneEVO.SDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NeptuneEVO.Core.GarbageCans;

namespace NeptuneEVO.Core
{
    public class InvInterface : Script
    {
        private static nLog Log = new nLog("InventoryInterface");
        [RemoteEvent("server::inventory:cases")]
        public void ClientEvent_Inventory(Player player, params object[] arguments)
        {
            try
            {
                if (player == null || !Main.Players.ContainsKey(player)) return;
                if (arguments.Length < 3) return;
                int type = Convert.ToInt32(arguments[0]);
                int index = Convert.ToInt32(arguments[1]);
                string data = Convert.ToString(arguments[2]);
                Log.Debug($"Type: {type} | Index: {index} | Data: {data}");
                Core.Character.Character acc = Main.Players[player];
                List<nItem> items;
                nItem item;
                switch (type)
                {
                    case 0:
                        {// self inventory
                            if (data == "takeground")
                            {
                                try
                                {

                                    GTANetworkAPI.Object itemGround = player.GetData<List<GTANetworkAPI.Object>>("ITEMGROUND")[index];
                                    if (itemGround == null)
                                    {
                                        Notify.Error(player, "Предмета не существует");
                                        sendItems(player);
                                        return;
                                    }
                                    Selecting.objectSelected(player, itemGround);
                                    return;
                                }
                                catch { }
                            }
                            items = nInventory.Items[acc.UUID];
                            item = items[index];
                            if (data == "drop")
                            {//remove one item from player inventory
                                if (item.FastSlots != -1)
                                {
                                    Notify.Info(player, "Сначала достаньте предмет из быстрого слота", 3000);
                                    return;
                                }
                                else if (item.Type == ItemType.IDCard)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя выбрасывать этот предмет", 3000);
                                    return;
                                }
                                else if (nInventory.ClothesItems.Contains(item.Type))
                                {
                                    if (item.IsActive)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны сначала снять эту одежду", 3000);
                                        return;
                                    }
                                    items.RemoveAt(index);
                                    Items.onDrop(player, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData), null);
                                    sendItems(player);
                                    return;
                                }
                                else if (item.Type == ItemType.NumberPlate)
                                {
                                    items.RemoveAt(index);
                                    Items.onDrop(player, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData), null);
                                    sendItems(player);
                                    return;
                                }
                                else if (nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) || item.Type == ItemType.StunGun)
                                {
                                    if (item.IsActive)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны убрать оружие из рук", 3000);
                                        return;
                                    }
                                    items.RemoveAt(index);
                                    Items.onDrop(player, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData), null);
                                    sendItems(player);
                                    return;
                                }
                                else if (item.Type == ItemType.CarKey)
                                {
                                    items.RemoveAt(index);
                                    Items.onDrop(player, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData), null);
                                    sendItems(player);
                                    return;
                                }
                                if (player.IsInVehicle)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя выбрасывать вещи, находясь в машине", 3000);
                                    return;
                                }
                                if (item.Count > 1)
                                {
                                    player.SetData("ITEMTYPE", item.Type);
                                    player.SetData("ITEMINDEX", index);
                                    Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_drop");
                                    return;
                                }
                                nInventory.Remove(player, item.Type, 1);
                                Items.onDrop(player, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData), null);
                            }
                            else if (data == "use")
                            {
                                try
                                {
                                    Log.Debug($"ItemID: {item.ID} | ItemType: {item.Type} | ItemData: {item.Data} | ItemName: {nInventory.InventoryItems.Find(x => x.ID == (int)item.Type).Name}");
                                    if (player.HasData("CHANGE_WITH"))
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Чтобы использовать вещи, нужно закрыть обмен вещами", 3000);
                                        return;
                                    }
                                    Items.onUse(player, item, index);
                                    return;
                                }
                                catch (Exception e)
                                {
                                    Log.Write(e.ToString(), nLog.Type.Error);
                                }
                            }
                            else if (data == "transfer")
                            {
                                if (!player.HasData("OPENOUT_TYPE")) return;
                                if (item.Type == ItemType.IDCard)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя перемещать этот предмет", 3000);
                                    return;
                                }
                                if (nInventory.ClothesItems.Contains(item.Type) && item.IsActive == true)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны сначала снять эту одежду", 3000);
                                    return;
                                }
                                else if ((nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type)) && item.IsActive == true)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы должны убрать оружие из рук", 3000);
                                    return;
                                }
                                switch (player.GetData<int>("OPENOUT_TYPE"))
                                {
                                    case 1:
                                        return;
                                    case 2:
                                        {
                                            Vehicle veh = player.GetData<Vehicle>("SELECTEDVEH");
                                            if (veh is null) return;
                                            if (veh.Dimension != player.Dimension)
                                            {
                                                Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) dimension");
                                                return;
                                            }
                                            if (veh.Position.DistanceTo(player.Position) > 10f)
                                            {
                                                Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) distance");
                                                return;
                                            }

                                            int tryAdd = VehicleInventory.TryAdd(veh, new nItem(item.Type, item.Count));
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В машине недостаточно места", 3000);
                                                return;
                                            }

                                            if (item.Type == ItemType.BagWithDrill)
                                            {
                                                player.SetClothes(5, 0, 0);
                                                player.ResetData("HEIST_DRILL");
                                            }
                                            else if (item.Type == ItemType.BagWithMoney)
                                            {
                                                player.SetClothes(5, 0, 0);
                                                player.ResetData("HAND_MONEY");
                                            }

                                            if (item.Count > 1)
                                            {
                                                //Close(player);
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_toveh");
                                                return;
                                            }
                                            if (item.Type == ItemType.Material)
                                            {
                                                int maxMats = (Fractions.Stocks.maxMats.ContainsKey(veh.DisplayName)) ? Fractions.Stocks.maxMats[veh.DisplayName] : 600;
                                                if (VehicleInventory.GetCountOfType(veh, ItemType.Material) + 1 > maxMats)
                                                {
                                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно загрузить такое кол-во матов", 3000);
                                                    return;
                                                }
                                            }

                                            VehicleInventory.Add(veh, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData));
                                            nInventory.Remove(player, item);
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"vehicle({veh.NumberPlate})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            return;
                                        }
                                    case 3:
                                        {
                                            if (item.Type == ItemType.BagWithDrill || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing || nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.IgnoreItems.Contains(item.Type))
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта вещь не предназначена для этого шкафа", 3000);
                                                return;
                                            }
                                            if (Main.Players[player].InsideHouseID == -1) return;
                                            int houseID = Main.Players[player].InsideHouseID;
                                            int furnID = player.GetData<int>("OpennedSafe");
                                            if (item.Count > 1)
                                            {
                                                //Close(player);
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_tosafe");
                                                return;
                                            }

                                            int tryAdd = Houses.FurnitureManager.TryAdd(houseID, furnID, item);
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                                return;
                                            }
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"itemSafe({furnID} | house: {houseID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            nInventory.Remove(player, item.Type, 1);
                                            sendItems(player);
                                            Houses.FurnitureManager.Add(houseID, furnID, new nItem(item.Type));
                                            return;
                                        }
                                    case 4:
                                        {
                                            if (!nInventory.ClothesItems.Contains(item.Type))
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Шкаф для одежды может хранить только одежду", 3000);
                                                return;
                                            }
                                            if (Main.Players[player].InsideHouseID == -1) return;
                                            int houseID = Main.Players[player].InsideHouseID;
                                            int furnID = player.GetData<int>("OpennedSafe");

                                            int tryAdd = Houses.FurnitureManager.TryAdd(houseID, furnID, item);
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                                return;
                                            }
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"clothSafe({furnID} | house: {houseID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            nInventory.Items[acc.UUID].Remove(item);
                                            sendItems(player);
                                            Houses.FurnitureManager.Add(houseID, furnID, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData));
                                            return;
                                        }
                                    case 5:
                                        {
                                            if (!player.HasData("CHANGE_WITH"))
                                            {
                                                Close(player);
                                                return;
                                            }
                                            Player target = player.GetData<Player>("CHANGE_WITH");
                                            if (!Main.Players.ContainsKey(target) || player.Position.DistanceTo(target.Position) > 2)
                                            {
                                                Close(player);
                                                return;
                                            }

                                            int tryAdd = nInventory.TryAdd(target, new nItem(item.Type, 1));
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У игрока недостаточно места", 3000);
                                                return;
                                            }

                                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[target].UUID, ItemType.BodyArmor) != null)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                                return;
                                            }

                                            if (item.Type == ItemType.BagWithDrill)
                                            {
                                                if (target.HasData("HEIST_DRILL") || target.HasData("HAND_MONEY"))
                                                {
                                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока уже есть дрель или деньги в руках", 3000);
                                                    return;
                                                }

                                                target.SetClothes(5, 41, 0);
                                                target.SetData("HEIST_DRILL", true);
                                                player.SetClothes(5, 0, 0);
                                                player.ResetData("HEIST_DRILL");
                                            }
                                            else if (item.Type == ItemType.BagWithMoney)
                                            {
                                                if (target.HasData("HEIST_DRILL") || target.HasData("HAND_MONEY"))
                                                {
                                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть сумка", 3000);
                                                    return;
                                                }

                                                target.SetClothes(5, 45, 0);
                                                target.SetData("HAND_MONEY", true);
                                                player.SetClothes(5, 0, 0);
                                                player.ResetData("HAND_MONEY");
                                            }

                                            if (item.Count > 1)
                                            {
                                                //Close(player, true);
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_toplayer");
                                                return;
                                            }

                                            nInventory.Add(target, item);
                                            nInventory.Remove(player, item);
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"player({Main.Players[target].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            return;
                                        }
                                    case 6:
                                        {
                                            if (!nInventory.WeaponsItems.Contains(item.Type) && !nInventory.AmmoItems.Contains(item.Type)) return;
                                            int onFraction = player.GetData<int>("ONFRACSTOCK");

                                            if (onFraction == 0) return;

                                            if (Fractions.Stocks.TryAdd(onFraction, new nItem(item.Type, 1)) != 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "На складе недостаточно места", 3000);
                                                return;
                                            }

                                            if (item.Count > 1)
                                            {
                                                // Close(player, true);
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_tofracstock");
                                                return;
                                            }

                                            string serial = (nInventory.WeaponsItems.Contains(item.Type)) ? $"({(string)item.Data})" : "";
                                            GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, $"{nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Name}{serial}", 1, false);
                                            Fractions.Stocks.Add(onFraction, item);
                                            nInventory.Remove(player, item);
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"fracstock({onFraction})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            return;
                                        }
                                    case 7:
                                        {
                                            nItem keyring = nInventory.Items[Main.Players[player].UUID][player.GetData<int>("KEYRING")];
                                            string keysData = Convert.ToString(keyring.Data);
                                            List<string> keys = (keysData.Length == 0) ? new List<string>() : new List<string>(keysData.Split('/'));
                                            if (keys.Count > 0 && string.IsNullOrEmpty(keys[keys.Count - 1]))
                                                keys.RemoveAt(keys.Count - 1);

                                            if (keys.Count >= 5)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Максимум 5 ключей", 3000);
                                                return;
                                            }

                                            if (item.Type != ItemType.CarKey)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Применимо только для ключей", 3000);
                                                return;
                                            }

                                            keys.Add(item.Data);
                                            keysData = "";
                                            foreach (string key in keys)
                                                keysData += $"{key}/";
                                            keyring.Data = keysData; // ¯\_(ツ)_/¯
                                            nInventory.Items[Main.Players[player].UUID][player.GetData<int>("KEYRING")] = keyring;

                                            nInventory.Remove(player, item);

                                            List<nItem> keyringItems = new List<nItem>();
                                            foreach (string key in keys)
                                                keyringItems.Add(new nItem(ItemType.CarKey, 1, key));

                                            player.SetData("KEYRING", nInventory.Items[Main.Players[player].UUID].IndexOf(keyring));
                                            OpenOut(player, keyringItems, "Связка ключей", 7);
                                            return;
                                        }
                                    case 8: // Оружейный сейф
                                        {
                                            if (!nInventory.WeaponsItems.Contains(item.Type) && !nInventory.MeleeWeaponsItems.Contains(item.Type))
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Оружейный сейф может хранить только оружие", 3000);
                                                return;
                                            }
                                            if (Main.Players[player].InsideHouseID == -1) return;
                                            int houseID = Main.Players[player].InsideHouseID;
                                            int furnID = player.GetData<int>("OpennedSafe");

                                            int tryAdd = Houses.FurnitureManager.TryAdd(houseID, furnID, item);
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                                return;
                                            }
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"weapSafe({furnID} | house: {houseID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            nInventory.Items[acc.UUID].Remove(item);
                                            sendItems(player);
                                            Houses.FurnitureManager.Add(houseID, furnID, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData));
                                            return;
                                        }
                                    case 10:
                                        {
                                            nItem backpack = Bag.GetBackpack(player);

                                            if (backpack == null)
                                                return;

                                            int tryAdd = Bag.TryAdd(backpack, new nItem(item.Type, item.Count));
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В рюкзаке недостаточно места", 3000);
                                                return;
                                            }

                                            if (item.Type == ItemType.Bag)
                                            {
                                                Notify.Error(player, "В рюкзак нельзя положить рюкзак!");
                                                return;
                                            }

                                            if (item.Count > 1)
                                            {
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_tobackpack");
                                                return;
                                            }

                                            List<nItem> send = Bag.Add(backpack, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData));
                                            nInventory.Remove(player, item);
                                            Bag.Open(player, backpack, send);
                                            return;
                                        }
                                    case 21:
                                        {
                                            if (!player.HasData("GARBAGECAN")) return;
                                            GarbageCan gc = player.GetData<GarbageCan>("GARBAGECAN");
                                            int tryAdd = GarbageCan.TryAdd(gc, item);
                                            if (tryAdd == -1 || tryAdd > 0)
                                            {
                                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в мусорке", 3000);
                                                return;
                                            }
                                            if (item.Count > 1)
                                            {
                                                player.SetData("ITEMTYPE", item.Type);
                                                player.SetData("ITEMINDEX", index);
                                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_togarbage");
                                                return;
                                            }
                                            GameLog.Items($"player({Main.Players[player].UUID})", $"garbagecan: {gc.ID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                                            nInventory.Items[acc.UUID].Remove(item);
                                            sendItems(player);
                                            GarbageCan.Add(player.GetData<GarbageCan>("GARBAGECAN"), new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData));
                                            gc.Open(player);
                                            return;
                                        }
                                }
                                Close(player);
                                return;
                            }
                            break;
                        }
                    case 1:
                        { // droped items
                          //TODO
                            break;
                        }
                    case 2:
                        { // in car items
                            Vehicle veh = player.GetData<Vehicle>("SELECTEDVEH");
                            if (veh is null) return;
                            if (veh.Dimension != player.Dimension)
                            {
                                Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) dimension");
                                return;
                            }
                            if (veh.Position.DistanceTo(player.Position) > 10f)
                            {
                                Commands.SendToAdmins(3, $"!{{#d35400}}[CAR-INVENTORY-EXPLOIT] {player.Name} ({player.Value}) distance");
                                return;
                            }
                            items = veh.GetData<List<nItem>>("ITEMS");
                            item = items[index];
                            if (item.Type == ItemType.IDCard)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя перемещать этот предмет", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Type == ItemType.BagWithDrill)
                            {
                                if (player.HasData("HEIST_DRILL") || player.HasData("HAND_MONEY"))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть дрель или деньги в руках", 3000);
                                    return;
                                }

                                player.SetClothes(5, 41, 0);
                                player.SetData("HEIST_DRILL", true);
                            }
                            else if (item.Type == ItemType.BagWithMoney)
                            {
                                if (player.HasData("HEIST_DRILL") || NAPI.Data.HasEntityData(player, "HAND_MONEY"))
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть сумка", 3000);
                                    return;
                                }

                                player.SetClothes(5, 45, 0);
                                player.SetData("HAND_MONEY", true);
                            }

                            if (item.Count > 1)
                            {
                                //Close(player);
                                player.SetData("ITEMTYPE", item.Type);
                                player.SetData("ITEMINDEX", index);
                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_fromveh");
                                return;
                            }

                            VehicleInventory.Remove(veh, item);
                            nInventory.Add(player, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData));
                            GameLog.Items($"vehicle({veh.NumberPlate})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            break;
                        }
                    case 3: // Взять
                        {
                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData<int>("OpennedSafe");
                            Houses.HouseFurniture furniture = Houses.FurnitureManager.HouseFurnitures[houseID][furnID];
                            items = Houses.FurnitureManager.FurnituresItems[houseID][furnID];
                            item = items[index];
                            if (item.Type == ItemType.IDCard)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя перемещать этот предмет", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            if (item.Count > 1)
                            {
                                //Close(player);
                                player.SetData("ITEMTYPE", item.Type);
                                player.SetData("ITEMINDEX", index);
                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_fromsafe");
                                return;
                            }
                            GameLog.Items($"itemSafe({furnID} | house: {houseID})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            items.RemoveAt(index);
                            Houses.FurnitureManager.FurnituresItems[houseID][furnID] = items;
                            nInventory.Add(player, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData));
                            sendItems(player);
                            foreach (Player p in NAPI.Pools.GetAllPlayers())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;
                                if ((p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 3) && (Main.Players[p].InsideHouseID != -1 && Main.Players[p].InsideHouseID == houseID) && (p.HasData("OpennedSafe") && p.GetData<int>("OpennedSafe") == furnID))
                                    OpenOut(p, items, furniture.Name, 3);
                            }
                            break;
                        }
                    case 4:
                        {
                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData<int>("OpennedSafe");
                            Houses.HouseFurniture furniture = Houses.FurnitureManager.HouseFurnitures[houseID][furnID];
                            items = Houses.FurnitureManager.FurnituresItems[houseID][furnID];
                            item = items[index];
                            if (item.Type == ItemType.IDCard)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя перемещать этот предмет", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            GameLog.Items($"clothSafe({furnID} | house: {houseID})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            nInventory.Items[Main.Players[player].UUID].Add(item);
                            sendItems(player);

                            items.RemoveAt(index);
                            Houses.FurnitureManager.FurnituresItems[houseID][furnID] = items;
                            foreach (Player p in NAPI.Pools.GetAllPlayers())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;
                                if ((p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 4) && (Main.Players[p].InsideHouseID != -1 && Main.Players[p].InsideHouseID == houseID) && (p.HasData("OpennedSafe") && p.GetData<int>("OpennedSafe") == furnID))
                                    OpenOut(p, items, furniture.Name, 4);
                            }
                            break;
                        }
                    case 6:
                        {
                            if (!player.HasData("ONFRACSTOCK") || player.GetData<int>("ONFRACSTOCK") == 0) return;
                            int onFrac = player.GetData<int>("ONFRACSTOCK");
                            items = Fractions.Stocks.fracStocks[onFrac].Weapons;
                            item = items[index];
                            if (item.Type == ItemType.IDCard)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя перемещать этот предмет", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type, 1));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Count > 1)
                            {
                                //Close(player);
                                player.SetData("ITEMTYPE", item.Type);
                                player.SetData("ITEMINDEX", index);
                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_fromfracstock");
                                return;
                            }

                            nInventory.Add(player, item);
                            Fractions.Stocks.Remove(onFrac, item);
                            string serial = (nInventory.WeaponsItems.Contains(item.Type)) ? $"({(string)item.Data})" : "";
                            GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, $"{nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Name}{serial}", 1, true);
                            GameLog.Items($"fracstock({onFrac})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            break;
                        }
                    case 7:
                        { // keyring items
                            nItem keyring = nInventory.Items[Main.Players[player].UUID][player.GetData<int>("KEYRING")];
                            string keysData = Convert.ToString(keyring.Data);
                            List<string> keys = (keysData.Length == 0) ? new List<string>() : new List<string>(keysData.Split('/'));
                            if (keys.Count > 0 && keys[keys.Count - 1] == "")
                                keys.RemoveAt(keys.Count - 1);

                            item = new nItem(ItemType.CarKey, 1, keys[index]);
                            if (item.Type == ItemType.IDCard)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя перемещать этот предмет", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, item);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно места", 3000);
                                return;
                            }

                            keys.RemoveAt(index);
                            nInventory.Add(player, new nItem(item.Type, 1, item.Data));

                            keysData = "";
                            foreach (string key in keys)
                                keysData += $"{key}/";
                            keyring.Data = keysData; // ¯\_(ツ)_/¯
                            nInventory.Items[Main.Players[player].UUID][player.GetData<int>("KEYRING")] = keyring;

                            List<nItem> keyringItems = new List<nItem>();
                            foreach (string key in keys)
                                keyringItems.Add(new nItem(ItemType.CarKey, 1, key));
                            OpenOut(player, keyringItems, "Связка ключей", 7);
                            break;
                        }
                    case 8: // Взять
                        {
                            if (Main.Players[player].InsideHouseID == -1) return;
                            int houseID = Main.Players[player].InsideHouseID;
                            int furnID = player.GetData<int>("OpennedSafe");
                            Houses.HouseFurniture furniture = Houses.FurnitureManager.HouseFurnitures[houseID][furnID];
                            items = Houses.FurnitureManager.FurnituresItems[houseID][furnID];
                            item = items[index];
                            if (item.Type == ItemType.IDCard)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя перемещать этот предмет", 3000);
                                return;
                            }
                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }

                            if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            GameLog.Items($"weapSafe({furnID} | house: {houseID})", $"player({Main.Players[player].UUID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            nInventory.Items[Main.Players[player].UUID].Add(item);
                            sendItems(player);

                            items.RemoveAt(index);
                            Houses.FurnitureManager.FurnituresItems[houseID][furnID] = items;
                            foreach (Player p in NAPI.Pools.GetAllPlayers())
                            {
                                if (p == null || !Main.Players.ContainsKey(p)) continue;
                                if ((p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 8) && (Main.Players[p].InsideHouseID != -1 && Main.Players[p].InsideHouseID == houseID) && (p.HasData("OpennedSafe") && p.GetData<int>("OpennedSafe") == furnID))
                                    OpenOut(p, items, furniture.Name, 8);
                            }
                            break;
                        }
                    case 10: // Взять
                        {
                            nItem backpack = Bag.GetBackpack(player);

                            if (backpack == null)
                                return;

                            List<nItem> itemsBack = Bag.GetItems(backpack);
                            item = itemsBack[index];

                            int tryAdd = nInventory.TryAdd(player, new nItem(item.Type));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            if (item.Count > 1)
                            {
                                player.SetData("ITEMTYPE", item.Type);
                                player.SetData("ITEMINDEX", index);
                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_frombackpack");
                                return;
                            }
                            List<nItem> send = Bag.Remove(backpack, item.Type, item.Count);
                            nInventory.Add(player, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData));
                            Bag.Open(player, backpack, send);
                            sendItems(player);
                            break;
                        }
                    case 20:
                        {
                            if (Main.Players[player].AdminLVL >= 6 && Main.Players[player].InsideHouseID == -1)
                            {
                                if (!player.HasData("CHANGE_WITH"))
                                {
                                    Close(player);
                                    return;
                                }
                                Player target = player.GetData<Player>("CHANGE_WITH");
                                if (!Main.Players.ContainsKey(target))
                                {
                                    Close(player);
                                    return;
                                }
                                items = nInventory.Items[Main.Players[target].UUID];
                                item = items[index];
                                if (item.Type == ItemType.IDCard)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Нельзя перемещать этот предмет", 3000);
                                    return;
                                }
                                if (nInventory.ClothesItems.Contains(item.Type) && item.IsActive == true)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок должен снять эту одежду", 3000);
                                    return;
                                }
                                else if ((nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type)) && item.IsActive == true)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок должен убрать это оружие из рук", 3000);
                                    return;
                                }
                                int tryAdd1 = nInventory.TryAdd(player, new nItem(item.Type, 1));
                                if (tryAdd1 == -1 || tryAdd1 > 0)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно места", 3000);
                                    return;
                                }
                                if (item.Type == ItemType.BodyArmor && nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor) != null)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                    return;
                                }
                                if (item.Count > 1)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Такие вещи нельзя забрать", 3000);
                                    return;
                                }
                                nInventory.Add(player, item);
                                nInventory.Remove(target, item);
                                Close(player);
                                Commands.CMD_showPlayerStats(player, target.Value); // reopen target inventory
                                GameLog.Admin(player.Name, $"takeItem({item.Type} | {item.Data})", target.Name);
                                return;
                            }
                            break;
                        }
                    case 21:
                        {
                            if (!player.HasData("GARBAGECAN")) return;
                            GarbageCan gc = player.GetData<GarbageCan>("GARBAGECAN");
                            item = gc.Inventory[index];
                            int tryAdd = nInventory.TryAdd(player, item);
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            if (item.Count > 1)
                            {
                                player.SetData("ITEMTYPE", item.Type);
                                player.SetData("ITEMINDEX", index);
                                Trigger.PlayerEvent(player, "openModalRangeSlider", item.Count, "item_transfer_fromgarbage");
                                return;
                            }
                            GameLog.Items($"player({Main.Players[player].UUID})", $"garbagecan: {gc.ID})", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                            gc.Inventory.Remove(item);
                            sendItems(player);
                            nInventory.Add(player, new nItem(item.Type, 1, item.Data, wear: item.Wear, subdata: item.subData));
                            gc.Open(player);
                            break;
                        }
                }
            }
            catch (Exception e) { Log.Write("Inventory: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("server::inventory:open")]
        public void PlayerEvent_openInventory(Player player, bool state)
        {
            try
            {
                if (player.HasData("ARENA")) return;
                if (!state)
                {
                    Close(player);

                }
                else
                    Open(player);
            }
            catch (Exception e) { Log.Write("openInventory: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("server::inventory:close")]
        public void PlayerEvent_closeInventory(Player player, params object[] arguments)
        {
            try
            {
                if (player.HasData("OPENOUT_TYPE") && player.GetData<int>("OPENOUT_TYPE") == 20) sendItems(player);

                if (player.HasData("SELECTEDVEH"))
                {
                    Vehicle vehicle = player.GetData<Vehicle>("SELECTEDVEH");
                    vehicle.SetData("BAGINUSE", false);
                }

                player.ResetData("OPENOUT_TYPE");

                if (player.HasData("CHANGE_WITH") && Main.Players.ContainsKey(player.GetData<Player>("CHANGE_WITH")))
                {
                    Close(player.GetData<Player>("CHANGE_WITH"));
                    NAPI.Data.ResetEntityData(player.GetData<Player>("CHANGE_WITH"), "CHANGE_WITH");
                    player.ResetData("CHANGE_WITH");
                    if (Main.Players[player].AdminLVL != 0) sendStats(player);
                }
            }
            catch (Exception e) { Log.Write($"CloseInventory: " + e.Message, nLog.Type.Error); }
        }

        public static void Close(Player player, bool resetOpenOut = false)
        {
            int data = (resetOpenOut) ? 11 : 1;
            Trigger.PlayerEvent(player, "client::inventory:setinfo", data);
            player.ResetData("OPENOUT_TYPE");
        }
        public static void sendStats(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                List<object> data = GetStatsPlayerInfo(player);

                string json = JsonConvert.SerializeObject(data);
                Log.Debug("data is: " + json.ToString());
                Trigger.PlayerEvent(player, "client::inventory:setinfo", 2, json);
                data.Clear();
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_SENDSTATS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static Task SendStatsAsync(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return Task.CompletedTask; ;
                List<object> data = GetStatsPlayerInfo(player);

                string json = JsonConvert.SerializeObject(data);
                Log.Debug("data is: " + json.ToString());
                Trigger.PlayerEvent(player, "client::inventory:setinfo", 2, json);
                data.Clear();
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_SENDSTATS\":\n" + e.ToString(), nLog.Type.Error);
            }
            return Task.CompletedTask;
        }
        public static void sendItems(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                int UUID = Main.Players[player].UUID;

                if (!nInventory.Items.ContainsKey(UUID)) return;
                List<nItem> items = new List<nItem>(nInventory.Items[UUID]);

                List<object> data = new List<object>();
                foreach (nItem item in items)
                {
                    List<object> idata = GetItemsInfo(item);
                    data.Add(idata);
                }

                string itemsGround = GetItemsGrounds(player);
                string json = JsonConvert.SerializeObject(data);
                Log.Debug(json);
                Trigger.PlayerEvent(player, "client::inventory:setinfo", 3, json, nInventory.GetWeight(nInventory.Items[Main.Players[player].UUID]), itemsGround);

                items.Clear();
                data.Clear();
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_SENDITEMS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static async Task SendItemsAsync(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                int UUID = Main.Players[player].UUID;

                if (!nInventory.Items.ContainsKey(UUID)) return;
                List<nItem> items = new List<nItem>(nInventory.Items[UUID]);

                List<object> data = new List<object>();
                foreach (nItem item in items)
                {
                    List<object> idata = GetItemsInfo(item);
                    data.Add(idata);
                }

                string itemsGround = GetItemsGrounds(player, false);
                string json = JsonConvert.SerializeObject(data);
                await Log.DebugAsync(json);
                NAPI.Task.Run(() => Trigger.PlayerEvent(player, "client::inventory:setinfo", 3, json, nInventory.GetWeight(nInventory.Items[Main.Players[player].UUID]), itemsGround));

                items.Clear();
                data.Clear();
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_SENDITEMS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void Open(Player Player)
        {
            Trigger.PlayerEvent(Player, "client::inventory:setinfo", 0);
            sendItems(Player);
            sendStats(Player);
        }
        public static void OpenOut(Player Player, List<nItem> items, string title, int type = 1, int max = 100)
        {
            try
            {
                if (type == 0) return;
                if (items == null) return;
                List<object> data = new List<object>();
                data.Add(type);
                data.Add(title);
                List<object> Items = new List<object>();
                foreach (nItem item in items)
                {
                    List<object> idata = GetItemsInfo(item);
                    Items.Add(idata);
                }
                if (type == 2)
                    max = VehicleInventory.GetVehicleWeight(Player.GetData<Vehicle>("SELECTEDVEH"));
                data.Add(Items);
                data.Add(max);
                data.Add(nInventory.GetWeight(items));

                string json = JsonConvert.SerializeObject(data);
                Log.Debug(json);
                Player.SetData("OPENOUT_TYPE", type);
                Trigger.PlayerEvent(Player, "client::inventory:setinfo", 4, json);
                Trigger.PlayerEvent(Player, "client::inventory:setinfo", 5, true);
                Trigger.PlayerEvent(Player, "client::inventory:setinfo", 0);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"DASHBOARD_OPENOUT\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void Update(Player Player, nItem item, int index)
        {
            List<object> idata = GetItemsInfo(item);
            string itemsGround = GetItemsGrounds(Player, false);
            string json = JsonConvert.SerializeObject(idata);
            Trigger.PlayerEvent(Player, "client::inventory:setinfo", 6, json, index, nInventory.GetWeight(nInventory.Items[Main.Players[Player].UUID]), itemsGround);
        }
        public static string GetItemsGrounds(Player player, bool data = true)
        {
            List<object> itemsGround = new List<object>();
            itemsGround.Clear();
            List<GTANetworkAPI.Object> itemsGroundObject = new List<GTANetworkAPI.Object>();
            itemsGroundObject.Clear();
            foreach (GTANetworkAPI.Object o in NAPI.Pools.GetAllObjects())
            {
                if (player.Position.DistanceTo(o.Position) < 5)
                {
                    if (o.HasSharedData("TYPE") || o.GetSharedData<string>("TYPE") == "DROPPED" || o.HasData("ITEM"))
                    {
                        nItem itemo = NAPI.Data.GetEntityData(o, "ITEM");
                        List<object> odata = GetItemsInfo(itemo);
                        itemsGround.Add(odata);
                        if (data)
                        {
                            itemsGroundObject.Add(o);
                        }
                    }
                }
            }
            if (data)
            {
                if (player.HasData("ITEMGROUND"))
                {
                    player.ResetData("ITEMGROUND");
                }
                player.SetData("ITEMGROUND", itemsGroundObject);
            }
            return JsonConvert.SerializeObject(itemsGround);
        }
        public static Task UpdateAsync(Player Player, nItem item, int index)
        {
            try
            {
                List<object> idata = GetItemsInfo(item);
                string json = JsonConvert.SerializeObject(idata);
                string itemsGround = GetItemsGrounds(Player, false);
                NAPI.Task.Run(() => Trigger.PlayerEvent(Player, "client::inventory:setinfo", 6, json, index, nInventory.GetWeight(nInventory.Items[Main.Players[Player].UUID]), itemsGround));
            }
            catch (Exception e) { Log.Write("UpdateAsync: " + e.Message); }

            return Task.CompletedTask;
        }
        public static List<object> GetItemsInfo(nItem item)
        {
            string itemData = "";
            if (nInventory.WeaponsItems.Contains(item.Type) || item.Type == ItemType.StunGun)
                itemData = "Серийный номер: " + item.Data;
            else if (item.Type == ItemType.CarKey)
                itemData = item.Data.Split('_')[0];
            else if (item.Type == ItemType.NumberPlate)
                itemData = "Номер: " + item.Data;

            List<object> idata = new List<object>
            {
                item.ID,
                item.Count,
                item.IsActive ? 1 : 0,
                itemData,
                item.Wear,
                item.Data,
                nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Weight,
                item.FastSlots,
            };
            return idata;
        }
        public static List<object> GetStatsPlayerInfo(Player player)
        {
            Core.Character.Character acc = Main.Players[player];
            List<object> data = new List<object>
            {
                acc.Water,
                acc.Eat,
                player.Health
            };
            return data;
        }
    }
}

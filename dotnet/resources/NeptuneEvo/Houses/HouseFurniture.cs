﻿using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using System.Data;
using System.Linq;

namespace NeptuneEVO.Houses
{
    class HouseFurniture
    {
        public string Name { get; }
        public string Model { get; }
        public int ID { get; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public bool IsSet { get; set; }

        [JsonIgnore]
        public GTANetworkAPI.Object obj { get; private set; }

        public HouseFurniture(int id, string name, string model)
        {
            Name = name;
            Model = model;
            ID = id;
            IsSet = false;
        }

        public GTANetworkAPI.Object Create(uint Dimension)
        {
            obj = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(Model), Position, Rotation, 255, Dimension);
            obj.SetSharedData("TYPE", FurnitureManager.nametoname[Model]);
            obj.SetSharedData("PICKEDT", false);
            obj.SetData("ID", ID);
            return obj;
        }
    }

    class FurnitureManager : Script
    {
        private static nLog Log = new nLog("HouseFurniture");
        public static Dictionary<int, Dictionary<int, HouseFurniture>> HouseFurnitures = new Dictionary<int, Dictionary<int, HouseFurniture>>();
        public static Dictionary<int, Dictionary<int, List<nItem>>> FurnituresItems = new Dictionary<int, Dictionary<int, List<nItem>>>();

        public static Dictionary<string, string> nametoname = new Dictionary<string, string> {
            {"prop_cabinet_02b", "SubjectSafe" },
            {"prop_ld_int_safe_01", "WeaponSafe" },
            {"prop_cabinet_01b", "ClothesSafe" },
        };

        public FurnitureManager()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM `furniture`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    Dictionary<int, HouseFurniture> furnitures = JsonConvert.DeserializeObject<Dictionary<int, HouseFurniture>>(Row["furniture"].ToString());
                    Dictionary<int, List<nItem>> items = JsonConvert.DeserializeObject<Dictionary<int, List<nItem>>>(Row["data"].ToString());
                    int id = Convert.ToInt32(Row["uuid"].ToString());
                    HouseFurnitures.Add(id, furnitures);
                    FurnituresItems.Add(id, items);
                }
                Log.Write($"Loaded {HouseFurnitures.Count} players furnitures.", nLog.Type.Success);
            }
            catch (Exception e) { Log.Write("FURNITURE: " + e.ToString(), nLog.Type.Error); }
        }
        public static Dictionary<string, string> NameModels = new Dictionary<string, string>()
        {   
            { "Оружейный сейф", "prop_ld_int_safe_01" },
            { "Шкаф с одеждой", "prop_cabinet_01b" },
            { "Шкаф с предметами", "prop_cabinet_02b" },
        };
        public static void Save()
        {
            foreach (var data in HouseFurnitures)
            {
                string furniture = JsonConvert.SerializeObject(data.Value);
                string items = JsonConvert.SerializeObject(FurnituresItems[data.Key]);
                MySQL.Query($"UPDATE `furniture` SET `furniture`='{furniture}',`data`='{items}' WHERE `uuid`='{data.Key}'");
            }
            Log.Write("Saved!", nLog.Type.Save);
        }
        public static void Create(int id)
        {
            if (!HouseFurnitures.ContainsKey(id))
            {
                HouseFurnitures.Add(id, new Dictionary<int, HouseFurniture>());
                FurnituresItems.Add(id, new Dictionary<int, List<nItem>>());
                MySQL.Query($"INSERT INTO `furniture`(`uuid`,`furniture`,`data`) VALUES ({id},'{JsonConvert.SerializeObject(new Dictionary<int, HouseFurniture>())}','{JsonConvert.SerializeObject(new Dictionary<int, List<nItem>>())}')");
            }
        }

        public static void newFurniture(int id, string name)
        {
            if (!HouseFurnitures.ContainsKey(id)) Create(id);
            int i = HouseFurnitures[id].Count();
            while (HouseFurnitures[id].ContainsKey(i)) i++;
            List<nItem> data = null;
            if (name == "Шкаф с одеждой" || name == "Оружейный сейф" || name == "Шкаф с предметами") data = new List<nItem>();
            HouseFurniture furn = new HouseFurniture(i, name, NameModels[name]);
            FurnituresItems[id].Add(furn.ID, data);
            HouseFurnitures[id].Add(i, furn);
            string furniture = JsonConvert.SerializeObject(HouseFurnitures[id]);
            string items = JsonConvert.SerializeObject(FurnituresItems[id]);
            MySQL.Query($"UPDATE `furniture` SET `furniture`='{furniture}',`data`='{items}' WHERE `uuid`='{id}'");
        }

        [RemoteEvent("acceptEdit")]
        public void PlayerEvent_acceptEdit(Player player, float X, float Y, float Z, float XX, float YY, float ZZ)
        {
            try
            {
                if (!player.HasData("IS_EDITING")) return;
                House house = HouseManager.GetHouse(player, true);
                if (house == null)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                    return;
                }
                int id = player.GetData<int>("EDIT_ID");
                HouseFurnitures[house.ID][id].IsSet = true;
                Vector3 pos = new Vector3(X, Y, Z);
                Vector3 rot = new Vector3(XX, YY, ZZ);
                
                HouseFurnitures[house.ID][id].Position = pos;
                HouseFurnitures[house.ID][id].Rotation = rot;
                house.DestroyFurnitures();
                house.CreateAllFurnitures();
                player.ResetData("IS_EDITING");

                string furniture = JsonConvert.SerializeObject(HouseFurnitures[house.ID]);
                string items = JsonConvert.SerializeObject(FurnituresItems[house.ID]);
                MySQL.Query($"UPDATE `furniture` SET `furniture`='{furniture}',`data`='{items}' WHERE `uuid`='{house.ID}'");

                return;
            } catch (Exception e) { Log.Write("acceptEdit: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("cancelEdit")]
        public void PlayerEvent_cancelEdit(Player player, params object[] arguments)
        {
            try
            {
                player.ResetData("IS_EDITING");
                return;
            }
            catch (Exception e) { Log.Write("cancelEdit: " + e.Message, nLog.Type.Error); }
        }

        #region Safes Inventory
        public static Dictionary<string, int> SafesType = new Dictionary<string, int>()
        {
            { "Шкаф с предметами", 3 },
            { "Шкаф с одеждой", 4 },
            { "Оружейный сейф", 8 },
        };

        public static void Add(int houseID, int furnID, nItem item)
        {
            HouseFurniture furniture = HouseFurnitures[houseID][furnID];
            var type = SafesType[furniture.Name];

            var items = FurnituresItems[houseID][furnID];

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || item.Type == ItemType.CarKey || nInventory.IgnoreItems.Contains(item.Type))
            {
                items.Add(item);
            }
            else
            {
                var count = item.Count;
                for (int i = 0; i < items.Count; i++)
                {
                    if (i >= items.Count) break;
                    if (items[i].Type == item.Type && items[i].Count < nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks)
                    {
                        var temp = nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks - items[i].Count;
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

            FurnituresItems[houseID][furnID] = items;
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if ((p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == type) && (Main.Players[p].InsideHouseID != -1 && Main.Players[p].InsideHouseID == houseID) && (p.HasData("OpennedSafe") && p.GetData<int>("OpennedSafe") == furnID))
                    InvInterface.OpenOut(p, items, furniture.Name, type);
            }

            MySQL.Query($"UPDATE `furniture` SET `data`='{items}' WHERE `uuid`='{houseID}'");
        }

        public static int TryAdd(int houseID, int furnID, nItem item)
        {
            HouseFurniture furniture = HouseFurnitures[houseID][furnID];
            var items = FurnituresItems[houseID][furnID];

            var tail = 0;
            if (nInventory.ClothesItems.Contains(item.Type) || item.Type == ItemType.CarKey || nInventory.IgnoreItems.Contains(item.Type))
            {
                if (items.Count >= 25) return -1;
            }
            else
            {
                var count = 0;
                foreach (var i in items)
                    if (i.Type == item.Type) count += nInventory.InventoryItems.Find(x => x.ItemType == i.Type).Stacks - i.Count;

                var slots = 25;
                var maxCapacity = (slots - items.Count) * nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks + count;
                if (item.Count > maxCapacity) tail = item.Count - maxCapacity;
            }
            return tail;
        }

        public static int GetCountOfType(int houseID, int furnID, ItemType type)
        {
            HouseFurniture furniture = HouseFurnitures[houseID][furnID];
            var items = FurnituresItems[houseID][furnID];
            var count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (i >= items.Count) break;
                if (items[i].Type == type) count += items[i].Count;
            }

            return count;
        }

        public static void Remove(int houseID, int furnID, ItemType type, int amount)
        {
            HouseFurniture furniture = HouseFurnitures[houseID][furnID];
            var safeType = SafesType[furniture.Name];
            var items = FurnituresItems[houseID][furnID];

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
            FurnituresItems[houseID][furnID] = items;
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if ((p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == safeType) && (Main.Players[p].InsideHouseID != -1 && Main.Players[p].InsideHouseID == houseID) && (p.HasData("OpennedSafe") && p.GetData<int>("OpennedSafe") == furnID))
                    InvInterface.OpenOut(p, items, furniture.Name, safeType);
            }
            MySQL.Query($"UPDATE `furniture` SET `data`='{JsonConvert.SerializeObject(FurnituresItems[houseID])}' WHERE `uuid`='{houseID}'");
        }

        public static void Remove(int houseID, int furnID, nItem item)
        {
            HouseFurniture furniture = HouseFurnitures[houseID][furnID];
            var safeType = SafesType[furniture.Name];
            var items = FurnituresItems[houseID][furnID];

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || item.Type == ItemType.CarKey || nInventory.IgnoreItems.Contains(item.Type))
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
            FurnituresItems[houseID][furnID] = items;
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if ((p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == safeType) && (Main.Players[p].InsideHouseID != -1 && Main.Players[p].InsideHouseID == houseID) && (p.HasData("OpennedSafe") && p.GetData<int>("OpennedSafe") == furnID))
                    InvInterface.OpenOut(p, items, furniture.Name, safeType);
            }
            MySQL.Query($"UPDATE `furniture` SET `data`='{JsonConvert.SerializeObject(FurnituresItems[houseID])}' WHERE `uuid`='{houseID}'");
        }
        #endregion
    }
}

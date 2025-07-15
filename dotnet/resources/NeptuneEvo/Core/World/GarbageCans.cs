using GTANetworkAPI;
using System;
using System.Linq;
using NeptuneEVO.SDK;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data;

namespace NeptuneEVO.Core
{
    class GarbageCans : Script
    {
        public static Dictionary<int, GarbageCan> List = new Dictionary<int, GarbageCan>();
        private static int LastID = 0;
        private static nLog Log = new nLog("GarbageCan");
        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM garbagecans");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB GarbageCans return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    int id = Convert.ToInt32(Row["id"].ToString());
                    Vector3 pos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());
                    new GarbageCan(pos);
                    LastID = id;
                }
                Log.Write("Loaded", nLog.Type.Info);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }
        [Command("creategc")]
        public static void CMD_CreateGarbageCan(Player player)
        {
            LastID++;
            Vector3 pos = player.Position - new Vector3(0, 0, 1.25);
            new GarbageCan(pos);
            MySQL.Query($"INSERT INTO `garbagecans` (`id`, `pos`, `items`) VALUES({LastID},'{JsonConvert.SerializeObject(pos)}','{JsonConvert.SerializeObject(new List<nItem>())}')");
        }
        public static GarbageCan GetNearestGarbageCan(Vector3 pos)
        {
            GarbageCan neareset = List[0];
            foreach (var v in List.Values)
            {
                if (pos.DistanceTo(v.Pos) < pos.DistanceTo(neareset.Pos))
                    neareset = v;
            }
            return neareset;
        }
        public static void RemoveAllInventory()
        {
            foreach(GarbageCan gc in List.Values)
            {
                gc.Inventory = new List<nItem>();
            }
        }
        public class GarbageCan
        {
            public int ID { get; set; }
            public Vector3 Pos { get; set; }
            public List<nItem> Inventory = new List<nItem>();
            [JsonIgnore]
            public GTANetworkAPI.ColShape Shape { get; set; }
            [JsonIgnore]
            public Blip Blip { get; set; }
            public GarbageCan(Vector3 pos)
            {
                ID = LastID; Pos = pos; Inventory = new List<nItem>();
                Shape = NAPI.ColShape.CreateCylinderColShape(Pos, 2f, 2f, 0);
                //Blip = NAPI.Blip.CreateBlip(318, pos, 0.5f, 5, "Мусорка", 255, false);
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        if (entity.IsInVehicle) return;
                        entity.SetData("INTERACTIONCHECK", 943);
                        entity.SetData("GARBAGECAN", this);
                        Trigger.PlayerEvent(entity, "client::showhintHUD", true, "Открыть мусорку");
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", -1);
                        entity.ResetData("GARBAGECAN");
                        Trigger.PlayerEvent(entity, "client::showhintHUD", false, "");
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                List.Add(ID, this);
                LastID++;
            }
            public void Open(Player player)
            {
                if (player.IsInVehicle) return;
                if (player.HasData("GARBAGECAN"))
                {
                    GarbageCan gc = player.GetData<GarbageCan>("GARBAGECAN");
                    InvInterface.OpenOut(player, gc.Inventory, "Мусорка", 21, 10000);
                    if (player.HasData("GARBAGE_LASTID") && player.GetData<int>("GARBAGE_LASTID") != gc.ID)
                    {
                        BattlePass.AddProgressToQuest(player, 5, 1);
                    }
                    player.SetData<int>("GARBAGE_LASTID", gc.ID);
                }
            }
            public static int TryAdd(GarbageCan gc, nItem item)
            {
                var items = gc.Inventory;

                var tail = 0;
                if (nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type) || nInventory.AmmoItems.Contains(item.Type) || nInventory.IgnoreItems.Contains(item.Type))
                {
                    if (items.Count >= 50) return -1;
                }
                else
                {
                    var count = 0;
                    foreach (var i in items)
                        if (i.Type == item.Type) count += nInventory.InventoryItems.Find(x => x.ItemType == i.Type).Stacks - i.Count;

                    var slots = 50;
                    var maxCapacity = (slots - items.Count) * nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks + count;
                    if (item.Count > maxCapacity) tail = item.Count - maxCapacity;
                }
                return tail;
            }
            public static void Add(GarbageCan gc, nItem item)
            {

                var items = gc.Inventory;

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
            }
        }

    }
}

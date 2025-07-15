using GTANetworkAPI;
using System.Collections.Generic;
using System;
using NeptuneEVO.GUI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using Newtonsoft.Json;
using System.Data;

namespace NeptuneEVO.Core
{
    public class Market : Script
    {
        private static nLog Log = new nLog("Market");
        public static List<List<MItem>> ItemsBuy = new List<List<MItem>>()
        {
            new List<MItem> //Carrier
            {
                new MItem(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.PickAxe).Name, (int)ItemType.PickAxe, 159999, false),
            },
            new List<MItem> //Sawmill
            {
                new MItem(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Axe).Name, (int)ItemType.Axe, 130899, false),
            },
            new List<MItem> {}, //Sell Items Ore and Pile
        };
        public static List<List<MItem>> ItemsSell = new List<List<MItem>>()
        {
            new List<MItem> {}, //Carrier
            new List<MItem> {}, //Sawmill
            new List<MItem> //Sell Items Ore and Pile
            {
               new MItem(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Stone).Name, (int)ItemType.Stone, 2000, true),
               new MItem(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.CuprumOre).Name, (int)ItemType.CuprumOre, 2000, true),
               new MItem(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.IronOre).Name, (int)ItemType.IronOre, 2000, true),
               new MItem(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.SilverOre).Name, (int)ItemType.SilverOre, 2000, true),
               new MItem(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.GoldOre).Name, (int)ItemType.GoldOre, 2000, true),
               new MItem(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.WoodPile).Name, (int)ItemType.WoodPile, 2000, true),
            },
        };
        public static List<List<MItem>> ItemsOrder = new List<List<MItem>>()
        {
            new List<MItem> {}, //Carrier
            new List<MItem> {}, //Sawmill
            new List<MItem> {}, //Sell Items Ore and Pile
        };
        [RemoteEvent("server::market:buy")]
        public static void ClientEvent_BuyMarket(Player player, int id, int type)
        {
            if (ItemsBuy[type] == null)
            {
                Notify.Error(player, "Предмет не найден");
                return;
            }
            var item = ItemsBuy[type];
            string result = "";
            switch (item[id].Type)
            {
                case 0:
                    if (Main.Players[player].Money < item[id].Price)
                    {
                        Notify.Error(player, "Недостаточно средств");
                        return;
                    }
                    result = "$";
                    MoneySystem.Wallet.Change(player, -item[id].Price);
                    break;
            }
            var tryAdd = nInventory.TryAdd(player, new nItem((ItemType)item[id].ID, 1));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 2000);
                return;
            }
            nInventory.Add(player, new nItem((ItemType)item[id].ID, 1));
            Notify.Succ(player, $"Вы купили {item[id].Name} за {String.Format("{0:n0}", item[id].Price)} {result}");
        }
        [RemoteEvent("server::market:sell")]
        public static void ClientEvent_SellMarket(Player player, int id, int type)
        {
            if (ItemsSell[type] == null)
            {
                Notify.Error(player, "Предмет не найден");
                return;
            }
            var item = ItemsSell[type];
            string result = "";
            switch (item[id].Type)
            {
                case 0:
                    result = "$";
                    break;
                case 1:
                    result = "MC";
                    break;
                case 2:
                    result = "мат.";
                    break;
            }
            if (nInventory.Find(Main.Players[player].UUID, (ItemType)item[id].ID) == null)
            {
                Notify.Error(player, $"У вас нет предмета {item[id].Name}");
                return;
            }
            nInventory.Remove(player, new nItem((ItemType)item[id].ID, 1));
            if ((ItemType)item[id].ID == ItemType.WoodPile)
            {
                BattlePass.AddProgressToQuest(player, 4, 1);
            }
            Notify.Succ(player, $"Вы продали {item[id].Name} за {String.Format("{0:n0}", item[id].Price)} {result}");
        }
        [RemoteEvent("server::market:order")]
        public static void ClientEvent_OrderMarket(Player player, int id, int type)
        {
            if (ItemsOrder[type] == null)
            {
                Notify.Error(player, "Предмет не найден");
                return;
            }
            var item = ItemsOrder[type];
            string result = "";
            switch (item[id].Type)
            {
                case 0:
                    if (Main.Players[player].Money < item[id].Price)
                    {
                        Notify.Error(player, "Недостаточно средств");
                        return;
                    }
                    result = "$";
                    MoneySystem.Wallet.Change(player, -item[id].Price);
                    break;
            }
            var tryAdd = nInventory.TryAdd(player, new nItem((ItemType)item[id].ID, 1));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 2000);
                return;
            }
            nInventory.Add(player, new nItem((ItemType)item[id].ID, 1));
            Notify.Succ(player, $"Вы купили {item[id].Name} за {String.Format("{0:n0}", item[id].Price)} {result}");
        }
    }
    internal class MarketNPC
    {
        public int ID { get; set; }
        public string NameNPC { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Vector3 Position { get; set; }
        [JsonIgnore]
        public GTANetworkAPI.ColShape Shape { get; set; }
        public MarketNPC(int id, string namenpc, string name, string desc, Vector3 pos)
        {
            ID = id; NameNPC = namenpc; Name = name; Description = desc; Position = pos;
            Shape = NAPI.ColShape.CreateCylinderColShape(Position, 2, 2, 0);
            Shape.OnEntityEnterColShape += (s, entity) =>
            {
                try
                {
                    entity.SetData("INTERACTIONCHECK", 942);
                    Trigger.PlayerEvent(entity, "client::showhintHUD", true, $"{Name}");
                    entity.SetData("MarketNPC", this);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
            Shape.OnEntityExitColShape += (s, entity) =>
            {
                try
                {
                    Trigger.PlayerEvent(entity, "client::showhintHUD", false, "");
                    entity.SetData("INTERACTIONCHECK", -1);
                    entity.ResetData("MarketNPC");
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
        }
        public void Open(Player player)
        {
            try
            {
                MarketNPC MNpc = player.GetData<MarketNPC>("MarketNPC");
                Trigger.PlayerEvent(player, "client::market:open", MNpc.ID, MNpc.NameNPC, MNpc.Name, MNpc.Description, JsonConvert.SerializeObject(Market.ItemsBuy[MNpc.ID]), JsonConvert.SerializeObject(Market.ItemsSell[MNpc.ID]), JsonConvert.SerializeObject(Market.ItemsOrder[MNpc.ID]));
            }
            catch (Exception e) { Console.WriteLine("MarketNPC_ClientEvent_Open: " + e.Message); }
        }
    }
    public class MItem
    {
        public string Name { get; set; }
        public int ID { get; set; }
        public int Price { get; set; }
        public bool UpdatePrice { get; set; }
        public int Type { get; set; }
        public MItem(string name, int id, int price, bool update, int type = 0)
        {
            Name = name;
            ID = id;
            Price = price;
            UpdatePrice = update;
            Type = type;
        }
    }
}

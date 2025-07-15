using System;
using System.Collections.Generic;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using static NeptuneEVO.Core.Quests;
using NeptuneEVO.Houses;
using Newtonsoft.Json;

namespace NeptuneEVO.Jobs
{
    class DeliveryClub : Script
    {
        public static int LastID = 1;
        public static nLog Log = new nLog("DeliveryClub");
        private static int _JobWorkID = 1;
        public static List<Order> Orders = new List<Order>();
        public static int MaxOrders = 30;

        public static Vector3 pos = new Vector3(-1037.7709, -1397.0588, 4.4331913);
        public static List<Position> TransportPositions = new List<Position>()
        {
            new Position(new Vector3(-1042.8815, -1400.2216, 5.408169),new Vector3(0.9384119, -11.933413, 41.43419)),
            new Position(new Vector3(-1043.3743, -1402.0767, 5.4080443),new Vector3(1.572466, -9.428407, 40.826378)),
            new Position(new Vector3(-1043.8203, -1403.9728, 5.40283),new Vector3(1.467003, -13.079054, 50.506416)),
        };

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {       
                NAPI.Blip.CreateBlip(120, pos, 0.9f, 4, Main.StringToU16("Работа: Delivery Club"), 255, 0, true, 0, 0);
                var shape = NAPI.ColShape.CreateCylinderColShape(pos, 1.2f, 2, 0); shape.OnEntityEnterColShape += (shape, player) => { try { Trigger.PlayerEvent(player, "client::showhintHUD", true, "Брэндон Кремер"); player.SetData("INTERACTIONCHECK", 1013); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } }; shape.OnEntityExitColShape += (shape, player) => { try { Trigger.PlayerEvent(player, "client::showhintHUD", false, ""); player.SetData("INTERACTIONCHECK", 0); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } };
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static List<List<object>> GetItemsInMarket()
        {
            List<List<object>> items = new List<List<object>>();
            foreach (Businesses.ShopI.Product prod in Businesses.ShopI.DeliveryClubItems)
            {
                List<object> item = new List<object>();
                item.Add(prod.ID);
                item.Add(prod.Name);
                item.Add(prod.ItemTInt);
                item.Add(prod.Price);
                items.Add(item);
            }
            return items;
        }
        [RemoteEvent("server::phone::deliveryclub:ordersset")]
        public static void SetOrdersToPhone(Player player)
        {
            Trigger.PlayerEvent(player, "client::phone::deliveryclub:ordersset", JsonConvert.SerializeObject(Orders));
        }
        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == _JobWorkID)
                {
                    if (player.HasData("DELIVERYCLUB_ORDER") && player.GetData<Order>("DELIVERYCLUB_ORDER") != null)
                    {
                        player.GetData<Order>("DELIVERYCLUB_ORDER").Taken = false;
                        player.SetData<Order>("DELIVERYCLUB_ORDER", null);
                    }
                    if (player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK"))
                    {
                        player.SetData("ON_WORK", false);
                        player.SetSharedData("ON_WORK", false);
                        Trigger.PlayerEvent(player, "deleteCheckpoint", 15);
                        Trigger.PlayerEvent(player, "deleteWorkBlip");
                        player.SetSharedData("DELIVERYCLUB_ORDER_TAKEN", false);

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили работу", 3000);
                    }
                    if (player.GetData<Vehicle>("WORK") != null)
                    {
                        NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("WORK"));
                        player.SetData<Vehicle>("WORK", null);
                    }
                    if (player.HasData("DELIVERY_CLUB_FOODINHANDS") && player.GetData<bool>("DELIVERY_CLUB_FOODINHANDS"))
                        BasicSync.DetachObject(player);
                    player.SetData<bool>("DELIVERY_CLUB_FOODINHANDS", false);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }
        public static void OpenMenu(Player player)
        {
            if (Main.Players[player].WorkID != _JobWorkID)
            {
                Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Брэндон Кремер", "Работодатель", "Приветсвую! Ищешь несложный заработок?", (new QuestAnswer("Что нужно делать?", 55), new QuestAnswer("Устроиться", 54), new QuestAnswer("В следующий раз", 2)));
                return;
            }
            else if (Main.Players[player].WorkID == _JobWorkID && player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK") == false)
            {
                Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Брэндон Кремер", "Работодатель", "Приветсвую! Мы уже болтали, не хочешь поработать?", (new QuestAnswer("Что нужно делать?", 55), new QuestAnswer("Уволиться", 54), new QuestAnswer("Начать рабочий день", 56), new QuestAnswer("В следующий раз", 2)));
                return;
            }
            else if (Main.Players[player].WorkID == _JobWorkID && player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK") == true)
            {
                Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Брэндон Кремер", "Работодатель", "Привет, уже закончил с развозкой?", (new QuestAnswer("Что нужно делать?", 55), new QuestAnswer("Уволиться", 54), new QuestAnswer("Закончить день", 56), new QuestAnswer("В следующий раз", 2)));
                return;
            }
        }
        public static void JobState(Player player)
        {
            if (Main.Players[player].WorkID != _JobWorkID)
            {
                if (Main.Players[player].WorkID != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Вы уже работаете: {Jobs.WorkManager.JobStats[Main.Players[player].WorkID - 1]}", 3000);
                    return;
                }
                Main.Players[player].WorkID = _JobWorkID;
                Notify.Succ(player, "Вы устроились на работу доставщиком Delivery Club");
            }
            else
            {
                if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Вы должны сначала закончить работу", 3000);
                    return;
                }
                if (Main.Players[player].WorkID != 0)
                {
                    if (Main.Players[player].WorkID == _JobWorkID)
                    {
                        Main.Players[player].WorkID = 0;
                        Notify.Succ(player, "Вы уволились с работы");
                    }
                }
            }
        }
        [RemoteEvent("server::deliveryclub:takefood")]
        public static void TakeFood(Player player, Vehicle veh)
        {
            if (player.HasData("WORK") && player.GetData<Vehicle>("WORK") != veh) return;
            if (player.HasSharedData("DELIVERY_CLUB_FOODINHANDS") && player.GetSharedData<bool>("DELIVERY_CLUB_FOODINHANDS") == true)
            {
                Main.StopSyncAnimation(player);
                BasicSync.DetachObject(player);
                player.SetSharedData("DELIVERY_CLUB_FOODINHANDS", false);
                return;
            }
            var rnd = new Random().Next(0, 3);
            switch (rnd)
            {
                case 0:
                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_pizza_box_01"), 57005, new Vector3(0.2, 0, -0.175), new Vector3(250, 70, -20));
                    Main.PlaySyncAnimation(player, "anim@heists@box_carry@", "idle", 49);
                    break;
                case 1:
                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("ng_proc_food_bag01a"), 57005, new Vector3(0.1, -0.3, 0), new Vector3(90, 0, 0));
                    Main.PlaySyncAnimation(player, "amb@world_human_janitor@male@base", "base", 49);
                    break;
                case 2:
                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("hei_prop_heist_box"), 57005, new Vector3(0.1, 0.15, -0.25), new Vector3(250, 70, -20));
                    Main.PlaySyncAnimation(player, "anim@heists@box_carry@", "idle", 49);
                    break;
                default:
                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("ng_proc_food_bag01a"), 57005, new Vector3(0.1, -0.3, 0), new Vector3(90, 0, 0));
                    Main.PlaySyncAnimation(player, "amb@world_human_janitor@male@base", "base", 49);
                    break;
            }
            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Burger);
            player.SetSharedData("DELIVERY_CLUB_FOODINHANDS", true);
        }
        public static void WorkState(Player player)
        {
            if (Main.Players[player].WorkID != _JobWorkID)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не работаете строителем", 3000);
                return;
            }
            if (player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK"))
            {
                if (player.HasData("DELIVERYCLUB_ORDER") && player.GetData<Order>("DELIVERYCLUB_ORDER") != null)
                {
                    player.GetData<Order>("DELIVERYCLUB_ORDER").Taken = false;
                    player.SetData<Order>("DELIVERYCLUB_ORDER", null);
                }
                player.SetData("ON_WORK", false);
                player.SetSharedData("ON_WORK", false);
                Trigger.PlayerEvent(player, "deleteWorkBlip");
                player.SetSharedData("DELIVERYCLUB_ORDER_TAKEN", false);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили работу", 3000);
                if (player.GetData<Vehicle>("WORK") != null)
                {
                    NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("WORK"));
                    player.SetData<Vehicle>("WORK", null);
                }
                if (player.HasSharedData("DELIVERY_CLUB_FOODINHANDS") && player.GetSharedData<bool>("DELIVERY_CLUB_FOODINHANDS") == true)
                    BasicSync.DetachObject(player);
                player.SetSharedData("DELIVERY_CLUB_FOODINHANDS", false);
            }
            else
            {
                if (player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK"))
                {
                    Notify.Error(player, "Вы уже работаете");
                    return;
                }
                player.SetData("ON_WORK", true);
                player.SetSharedData("ON_WORK", true);
                player.SetSharedData("DELIVERYCLUB_ORDER_TAKEN", false);
                player.SetSharedData("DELIVERY_CLUB_FOODINHANDS", false);
                SpawnWorkVehicle(player);
                Notify.Succ(player, $"Вы устроились на работу доставщиком Delivery Club! Количество заказов {Orders.Count}");
            }
        }
        public static House GetHouseForOrder()
        {
            var next = -1;
            do
            {
                next = WorkManager.rnd.Next(0, HouseManager.Houses.Count - 1);
            }
            while (Houses.HouseManager.Houses[next].Position.Y > 1500);
            return Houses.HouseManager.Houses[next];
        }
        [Command("getallorders")]
        public static void CMD_GetAllOrder(Player player)
        {
            foreach(Order order in Orders)
                NAPI.Chat.SendChatMessageToPlayer(player, $"Заказ: #{order.ID}; Дистанция: {order.Distance} метров; Цена: {order.Price}");
        }
        public static void AddOrder()
        {
            House house = GetHouseForOrder();
            int price = Convert.ToInt32(pos.DistanceTo2D(house.Position) * 2.5f);
            Orders.Add(new Order(house.Position, price, 4, Math.Round(pos.DistanceTo2D(house.Position), 1)));
        }
        [RemoteEvent("server::phone::delivery:buyall")]
        public static void BuyAllBasket(Player player, string json)
        {
            House house = HouseManager.GetNearestHouse(player);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                return;
            }
            List<List<object>> items = JsonConvert.DeserializeObject<List<List<object>>>(json);
            int total = 0;
            foreach (List<object> prod in items)
            {
                total += Convert.ToInt32(prod[3]) * Convert.ToInt32(prod[4]);
            }
            if (MoneySystem.Bank.Accounts[Main.Players[player].Bank].Balance < total)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств на банковском счете", 3000);
                return;
            }
            foreach (List<object> prod in items)
            {
                ItemType itemproduct = (ItemType)Convert.ToInt32(prod[2]);
                var item = new PostalItem("Delivery Club", $"{prod[1]} x{prod[4]}", $"{DateTime.Now.Day}.{DateTime.Now.Month}.{DateTime.Now.Year}", new nItem(itemproduct, Convert.ToInt32(prod[4])), 5);
                BattlePass.AddProgressToQuest(player, 14, Convert.ToInt32(prod[4]));
                house.Postal.Add(item);
            }
            MoneySystem.Bank.Change(Main.Players[player].Bank, -total, false);
            house.Save();
            Notify.Succ(player, "Вы успешно оплатили заказ, в течении 5 минут его вас доставят на ваш дом");
            Trigger.PlayerEvent(player, "client::phone::delivery:resetbasket");
        }
        [RemoteEvent("server::phone::deliveryclub:takeorder")]
        public static void TakeOrder(Player player, int id)
        {
            if (Main.Players[player].WorkID != _JobWorkID) return;
            if (!player.GetData<bool>("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            if (player.HasData("DELIVERYCLUB_ORDER") && player.GetData<Order>("DELIVERYCLUB_ORDER") != null)
            {
                Notify.Error(player, "Вы уже взяли заказ");
                return;
            }
            Order order = Orders.Find(x => x.ID == id);
            if (order == null || order.Taken)
            {
                Notify.Error(player, "Этот заказ уже взяли");
                return;
            }
            order.Taken = true;
            player.SetData<Order>("DELIVERYCLUB_ORDER", order);
            player.SetSharedData("DELIVERYCLUB_ORDER_TAKEN", true);

            Trigger.PlayerEvent(player, "createWaypoint", order.Position.X, order.Position.Y);
            Trigger.PlayerEvent(player, "createWorkBlip", order.Position);
            Notify.Succ(player, $"Вы взяли заказ #{order.ID}, он в {Math.Round(player.Position.DistanceTo2D(order.Position), 1)} метрах от вас", 4000);
        }
        public static void SpawnWorkVehicle(Player player)
        {
            if (Main.Players[player].WorkID != _JobWorkID)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете доставщиком Delivery Club", 3000);
                return;
            }
            if (!player.GetData<bool>("ON_WORK"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }
            if (player.GetData<Vehicle>("WORK") != null)
            {
                NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("WORK"));
                player.ResetData("WORK");
                return;
            }
            var rnd = new Random().Next(0, TransportPositions.Count);
            var veh = API.Shared.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey("faggios"), TransportPositions[rnd].Pos, TransportPositions[rnd].Rot, 92, 92, "DELIVERY");
            player.SetData("WORK", veh);
            veh.SetSharedData("DELIVERY_CAR", true);
            player.SetIntoVehicle(veh, 0);
            veh.SetData("ACCESS", "WORK");

            Core.VehicleStreaming.SetEngineState(veh, true);
        }
        public class Order
        {
            public int ID { get; set; }
            public int Price { get; set; }
            public double Distance { get; set; }
            public Vector3 Position { get; set; }
            public int Time { get; set; }
            public bool Taken { get; set; }
            public Order(Vector3 pos, int price, int time, double distance)
            {
                ID = LastID; Position = pos; Price = price; Time = time; Distance = distance; Taken = false;
                LastID++;
            }
        }
        public class Position
        {
            public Vector3 Pos { get; set; }
            public Vector3 Rot { get; set; }
            public Position(Vector3 p, Vector3 r)
            {
                Pos = p; Rot = r;
            }
        }
    }
}
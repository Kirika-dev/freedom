using GTANetworkAPI;
using System.Collections.Generic;
using System;
using NeptuneEVO.GUI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.Houses;
using Newtonsoft.Json;

namespace NeptuneEVO.Jobs
{
    class Gopostal : Script
    {
        //private static int JobMultiper = 3;
        private static nLog Log = new nLog("GoPostal");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(Coords[0], 1, 2, 0)); // start work
                Cols[0].OnEntityEnterColShape += gp_onEntityEnterColShape;
                Cols[0].OnEntityExitColShape += gp_onEntityExitColShape;
                Cols[0].SetData("INTERACT", 28);

                Cols.Add(2, NAPI.ColShape.CreateCylinderColShape(Coords[1], 1, 2, 0)); // Parcel Menu
                Cols[2].OnEntityEnterColShape += gp_onEntityEnterColShape;
                Cols[2].OnEntityExitColShape += gp_onEntityExitColShape;
                Cols[2].SetData("INTERACT", 940);

            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static int checkpointPayment = 25000;

        public static List<Vector3> Coords = new List<Vector3>()
        {
            new Vector3(81.65353, 135.72043, 79.41348), // start work
            new Vector3(80.95601, 133.47609, 79.41348), // GoPostal
        };
        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        // Postal items (objects) //
        public static List<uint> GoPostalObjects = new List<uint>
        {
            NAPI.Util.GetHashKey("prop_drug_package_02"),
        };
        [RemoteEvent("server::postal:card")]
        public static void CreatePostCard(Player player, string text, string to, int style)
        {
            var itemTake = nInventory.Find(Main.Players[player].UUID, ItemType.Note);
            if (itemTake == null)
            {
                Notify.Error(player, "Предмет не найден");
                return;
            }
            nInventory.Remove(player, new nItem(ItemType.Note, 1));
            List<object> Data = new List<object>()
            {
               text, Convert.ToString(player.Name.Replace("_", " ")), to, style
            };
            nInventory.Remove(player, itemTake);
            nInventory.Add(player, new nItem(ItemType.DoneNote, 1, Data));
            Notify.Succ(player, "Вы написали записку");
        }
        [RemoteEvent("server::gopostal:takeparcel")]
        public static void TakePostalParcel(Player player, int id)
        {
            var gpi = Main.Players[player].GoPostalInventory;
            if (gpi[id] == null)
            {
                Notify.Error(player, "Предмет не найден");
                return;
            }
            int tryAdd = nInventory.TryAdd(player, gpi[0].Item);
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                return;
            }
            nInventory.Add(player, gpi[0].Item);
            gpi.Remove(gpi[id]);
            Notify.Succ(player, "Вы забрали с почты посылку");
            Trigger.PlayerEvent(player, "client::postalmenu:updateitems", JsonConvert.SerializeObject(gpi));
            Trigger.PlayerEvent(player, "client::postalmenu:close");
        }
        public static void onPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != 2) return;
                if (player.GetData<Vehicle>("WORK") != null)
                {
                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    player.SetData("PAYMENT", 0);
                    NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("WORK"));
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != 2) return;
                if (NAPI.Data.GetEntityData(player, "ON_WORK") && NAPI.Data.GetEntityData(player, "PACKAGES") != 0)
                {
                    int x = WorkManager.rnd.Next(0, GoPostalObjects.Count);
                    BasicSync.AttachObjectToPlayer(player, GoPostalObjects[x], 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
                }
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == 2 && NAPI.Data.GetEntityData(player, "ON_WORK"))
                {
                    NAPI.Data.SetEntityData(player, "ON_WORK", false);
                    Customization.ApplyCharacter(player);
                    MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));

                    Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 3000);
                    player.SetData("PAYMENT", 0);
                    player.SetData("PACKAGES", 0);
                    Trigger.PlayerEvent(player, "client::postal:hud", player.GetData<int>("PACKAGES"));
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void GoPostal_onEntityEnterColShape(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (HouseManager.Houses.Count == 0) return;
                if (Main.Players[player].WorkID != 2 || !NAPI.Data.GetEntityData(player, "ON_WORK")) return;
                if (player.HasData("NEXTHOUSE") && player.HasData("HOUSEID") && NAPI.Data.GetEntityData(player, "NEXTHOUSE") == player.GetData<int>("HOUSEID"))
                {
                    if (NAPI.Player.IsPlayerInAnyVehicle(player))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Покиньте транспортное средство", 3000);
                        return;
                    }
                    if (player.GetData<int>("PACKAGES") == 0) return;
                    else if (player.GetData<int>("PACKAGES") > 1)
                    {
                        if (player.Position.DistanceTo(player.GetData<Vehicle>("WORK").Position) > 50)
                        {
                            Notify.Error(player, "Почтовая машина далеко от вас");
                            return;
                        }
                        player.SetData("PACKAGES", player.GetData<int>("PACKAGES") - 1);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"У Вас осталось {player.GetData<int>("PACKAGES")} посылок", 3000);
                        Trigger.PlayerEvent(player, "client::postal:hud", player.GetData<int>("PACKAGES"));

                        var coef = Convert.ToInt32(player.Position.DistanceTo2D(player.GetData<Vector3>("W_LASTPOS")) / (100 * 5));
                        var payment = Convert.ToInt32(coef * checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl] * Main.oldconfig.PaydayMultiplier);

                        int level = Main.Players[player].LVL > 5 ? 60 : 10 * Main.Players[player].LVL;

                        DateTime lastTime = player.GetData<DateTime>("W_LASTTIME");
                        if (DateTime.Now < lastTime.AddSeconds(coef * 2))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Хозяина нет дома, попробуйте позже", 3000);
                            return;
                        }

                        
                        player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment + level);
                        Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));
                        BasicSync.DetachObject(player);

                        var nextHouse = player.GetData<int>("NEXTHOUSE");
                        var next = -1;
                        do
                        {
                            next = WorkManager.rnd.Next(0, HouseManager.Houses.Count - 1);
                        }
                        while (Houses.HouseManager.Houses[next].Position.DistanceTo2D(player.Position) < 200);
                        player.SetData("W_LASTPOS", player.Position);
                        player.SetData("W_LASTTIME", DateTime.Now);
                        player.SetData("NEXTHOUSE", HouseManager.Houses[next].ID);

                        player.SendNotification($"Посылка: ~h~~g~+{payment + level}$", true);

                        Trigger.PlayerEvent(player, "createCheckpoint", 1, 1, HouseManager.Houses[next].Position, 1, 0, 254, 169, 66);
                        Trigger.PlayerEvent(player, "createWaypoint", HouseManager.Houses[next].Position.X, HouseManager.Houses[next].Position.Y);
                        Trigger.PlayerEvent(player, "createWorkBlip", HouseManager.Houses[next].Position);
                        BattlePass.AddProgressToQuest(player, 11, 1);
                        NAPI.Player.PlayPlayerAnimation(player, 49, "anim@heists@narcotics@trash", "drop_side");
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null)
                                {
                                    player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                }
                            }
                            catch { };
                        }, 1500);
                    }
                    else
                    {
                        var coef = Convert.ToInt32(player.Position.DistanceTo2D(player.GetData<Vector3>("W_LASTPOS")) / (100 * 5));
                        var payment = Convert.ToInt32(coef * checkpointPayment * Group.GroupPayAdd[Main.Accounts[player].VipLvl]);

                        int level = Main.Players[player].LVL > 5 ? 60 : 10 * Main.Players[player].LVL;

                        DateTime lastTime = player.GetData<DateTime>("W_LASTTIME");
                        if (DateTime.Now < lastTime.AddSeconds(coef * 2))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Хозяина нет дома, попробуйте позже", 3000);
                            return;
                        }

                        player.SetData("PAYMENT", player.GetData<int>("PAYMENT") + payment + level);

                        player.SendNotification($"Посылка: ~h~~g~+{payment + level}$", true);
                        Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                        Trigger.PlayerEvent(player, "deleteWorkBlip");
                        Trigger.PlayerEvent(player, "createWaypoint", Coords[1].X, Coords[1].Y);

                        BasicSync.DetachObject(player);

                        Trigger.PlayerEvent(player, "deleteCheckpoint", 1, 0);
                        NAPI.Player.PlayPlayerAnimation(player, 49, "anim@heists@narcotics@trash", "drop_side");
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null)
                                {
                                    player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                }
                            }
                            catch { };
                        }, 1500);
                        player.SetData("PACKAGES", 0);
                        Trigger.PlayerEvent(player, "client::postal:hud", player.GetData<int>("PACKAGES"));
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"У Вас не осталось посылок, отправляйтесь в офис GoPostal и возьмите новые на складе", 3000);
                    }
                }
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"GoPostal\":\n" + e.ToString(), nLog.Type.Error); }
        }
        private void gp_onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
                Trigger.PlayerEvent(entity, "client::showhintHUD", true, "Нажмите для взаимодействия");
            }
            catch (Exception ex) { Log.Write("gp_onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }
        private void gp_onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                Trigger.PlayerEvent(entity, "client::showhintHUD", false, "");
            }
            catch (Exception ex) { Log.Write("gp_onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void onPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seatid)
        {
            try
            {
                BasicSync.DetachObject(player);
            }
            catch (Exception e) { Log.Write("PlayerEnterVehicle: " + e.Message, nLog.Type.Error); }
        }

        public static void getGoPostalCar(Player player)
        {
            if (Main.Players[player].WorkID != 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете почтальоном", 3000);
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
            var rnd = new Random().Next(0, 3);
            var veh = API.Shared.CreateVehicle(VehicleHash.Boxville2, GoPostalPosition[rnd], new Vector3(0, 0, 158), 134, 77, "GOPOSTAL");
            player.SetData("WORK", veh);
            player.SetIntoVehicle(veh, 0);
            veh.SetData("ACCESS", "WORK");

            Core.VehicleStreaming.SetEngineState(veh, true);
        }
        public static Vector3[] GoPostalPosition = new Vector3[]
        {
            new Vector3(59.693256, 126.94754, 79.647194),  //1
            new Vector3(62.38499, 126.02823, 79.6056),     //2
            new Vector3(65.034966, 125.129326, 79.571686),          //3
            new Vector3(67.449326, 124.297676, 79.56457),            //4
        };
    }
}

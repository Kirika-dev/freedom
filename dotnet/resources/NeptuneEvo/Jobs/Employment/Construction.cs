using System;
using System.Collections.Generic;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using static NeptuneEVO.Core.Quests;

namespace NeptuneEVO.Jobs
{
    class Builder : Script
    {
        public static Vector3 pos = new Vector3(-169.25494, -1026.9185, 26.172077);
        public static nLog Log = new nLog("Builder");
        public static int JobPayment = 80000;
        private static Vector3 _posElevator = new Vector3(-158.08932, -940.567, 29.286668);
        private static Vector3 _posRoof = new Vector3(-154.2388, -942.58405, 269.13498);
        private static int _JobWorkID = 11;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {                           
                NAPI.Blip.CreateBlip(566, pos, 0.8f, 4, Main.StringToU16("Работа: Строитель"), 255, 0, true, 0, 0);
                var shape = NAPI.ColShape.CreateCylinderColShape(pos, 1.2f, 2, 0); shape.OnEntityEnterColShape += (shape, player) => { try { Trigger.PlayerEvent(player, "client::showhintHUD", true, "Прораб Рон"); player.SetData("INTERACTIONCHECK", 1010); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } }; shape.OnEntityExitColShape += (shape, player) => { try { Trigger.PlayerEvent(player, "client::showhintHUD", false, ""); player.SetData("INTERACTIONCHECK", 0); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } };
                var _shapeElevator = NAPI.ColShape.CreateCylinderColShape(_posElevator, 1.2f, 2, 0); _shapeElevator.OnEntityEnterColShape += (shape, player) => { try { Trigger.PlayerEvent(player, "client::showhintHUD", true, "Лифт на крышу"); player.SetData("INTERACTIONCHECK", 1011); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } }; _shapeElevator.OnEntityExitColShape += (shape, player) => { try { Trigger.PlayerEvent(player, "client::showhintHUD", false, ""); player.SetData("INTERACTIONCHECK", 0); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } };
                var _shapeRoof = NAPI.ColShape.CreateCylinderColShape(_posRoof, 1.2f, 2, 0); _shapeRoof.OnEntityEnterColShape += (shape, player) => { try { Trigger.PlayerEvent(player, "client::showhintHUD", true, "Лифт на стройку"); player.SetData("INTERACTIONCHECK", 1012); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } }; _shapeRoof.OnEntityExitColShape += (shape, player) => { try { Trigger.PlayerEvent(player, "client::showhintHUD", false, ""); player.SetData("INTERACTIONCHECK", 0); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } };

                NAPI.Marker.CreateMarker(0, _posElevator, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.8f, new Color(67, 140, 239, 200), false, 0);
                NAPI.Marker.CreateMarker(0, _posRoof, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.8f, new Color(67, 140, 239, 200), false, 0);
                int i = 0;
                foreach (var CheckStop in ChecksStop)
                {
                    var col = NAPI.ColShape.CreateCylinderColShape(CheckStop.Position, 2, 2, 0);
                    col.SetData("NUMBER2", i);
                    col.OnEntityEnterColShape += EnterShapePutContAndTakeNew;
                    col.OnEntityExitColShape += ExitColshape;
                    i++;
                }
                int to = 0;
                foreach (var Check in Checks)
                {
                    var col = NAPI.ColShape.CreateCylinderColShape(Check.Position, 2, 2, 0);
                    col.SetData("NUMBER", to);
                    col.OnEntityEnterColShape += EnterShapeTakeNewCont;
                    col.OnEntityExitColShape += ExitColshape;
                    to++;
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void ExitColshape(ColShape shape, Player player)
        {
            player.SetData("INTERACTIONCHECK", -1);
            player.SetData("WorkColshape", -1);
            Trigger.PlayerEvent(player, "client::showhintHUD", false, "Взять мешок");
        }
        public static void Event_PlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID == _JobWorkID && player.GetData<bool>("ON_WORK"))
                {
                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);
                    player.SetSharedData("ON_WORK", false);
                    player.SetData("ShapeBuilderState", false);
                    player.SetSharedData("ShapeBuilderState", false);
                    Trigger.PlayerEvent(player, "deleteCheckpoint", 15);
                    Trigger.PlayerEvent(player, "deleteWorkBlip");

                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили работу", 3000);

                    player.StopAnimation();
                    Main.OffAntiAnim(player);
                    BasicSync.DetachObject(player);
                }
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }
        public static void SetWorkId(Player player)
        {
            if (Main.Players[player].WorkID != _JobWorkID)
            {
                if (Main.Players[player].WorkID != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Вы уже работаете: {Jobs.WorkManager.JobStats[Main.Players[player].WorkID - 1]}", 3000);
                    return;
                }
                Main.Players[player].WorkID = _JobWorkID;
                Notify.Succ(player, "Вы устроились на работу строителем");
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
                        Notify.Succ(player, "Вы уволились с работы строителя");
                    }
                }
            }
        }
       public static void OpenMenu(Player player)
        {
            if (Main.Players[player].WorkID != _JobWorkID)
            {
                Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Рон", "Прораб", "Привет, не хочешь заработать немного зеленых, нужно всего лишь таскать мешки", (new QuestAnswer("Как тут работать?", 51), new QuestAnswer("Устроиться", 50), new QuestAnswer("В следующий раз", 2)));
                return;
            }
            else if (Main.Players[player].WorkID == _JobWorkID && player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK") == false)
            {
                Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Рон", "Прораб", "Привет, не хочешь заработать немного зеленых, нужно всего лишь таскать мешки", (new QuestAnswer("Как тут работать?", 51), new QuestAnswer("Уволиться", 50), new QuestAnswer("Начать рабочий день", 52), new QuestAnswer("В следующий раз", 2)));
                return;
            }
            else if (Main.Players[player].WorkID == _JobWorkID && player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK") == true)
            {
                Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Рон", "Прораб", "Привет, не хочешь заработать немного зеленых, нужно всего лишь таскать мешки", (new QuestAnswer("Как тут работать?", 51), new QuestAnswer("Уволиться", 50), new QuestAnswer("Закончить день", 52), new QuestAnswer("В следующий раз", 2)));
                return;
            }
        }
        public static void SetWorkState(Player player)
        {
            if (Main.Players[player].WorkID != _JobWorkID)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не работаете строителем", 3000);
                return;
            }
            else
            {
                if (player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK"))
                {
                    Customization.ApplyCharacter(player);
                    player.SetData("ON_WORK", false);
                    player.SetSharedData("ON_WORK", false);
                    Trigger.PlayerEvent(player, "deleteCheckpoint", 15);
                    Trigger.PlayerEvent(player, "deleteWorkBlip");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                    if (player.HasData("ShapeBuilderState") && player.GetData<bool>("ShapeBuilderState"))
                    {
                        BasicSync.DetachObject(player);
                    }
                    return;
                }
                else
                {
                    Customization.ClearClothes(player, Main.Players[player].Gender);
                    if (Main.Players[player].Gender)
                    {
                        player.SetClothes(3, 61, 0);
                        player.SetClothes(4, 36, 0);
                        player.SetClothes(6, 12, 0);
                        player.SetClothes(8, 59, 1);
                        player.SetClothes(11, 57, 0);
                        player.SetClothes(0, 0, 0);
                    }
                    else
                    {
                        player.SetClothes(3, 62, 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 26, 0);
                        player.SetClothes(8, 36, 1);
                        player.SetClothes(11, 50, 0);
                        player.SetClothes(0, 0, 0);
                    }
                    var check = WorkManager.rnd.Next(0, Checks.Count - 1);
                    player.SetData("WORKCHECK", check);
                    Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checks[check].Position, 1, 0, 67, 140, 239);
                    Trigger.PlayerEvent(player, "createWorkBlip", Checks[check].Position);

                    player.SetData("ON_WORK", true);
                    player.SetSharedData("ON_WORK", true);
                    Notify.Succ(player, "Вы начали рабочий день строителя. Метка установлена на GPS");
                }
            }
        }
        public static void WorkFirstCont(Player player)
        {
            var shape = player.GetData<ColShape>("WorkColshape");
            player.SetData("ShapeBuilderState", true);
            player.SetSharedData("ShapeBuilderState", true);
            NAPI.Entity.SetEntityPosition(player, Checks[shape.GetData<int>("NUMBER")].Position + new Vector3(0, 0, 1.2));
            NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, Checks[shape.GetData<int>("NUMBER")].Heading));
            player.SetData("INTERACTIONCHECK", -1);
            player.SetData("WorkColshape", -1);

            Main.OnAntiAnim(player);

            player.PlayAnimation("anim@mp_snowball", "pickup_snowball", 1);
            player.PlayAnimation("anim@heists@box_carry@", "idle", 49);
            NAPI.Task.Run(() =>
            {
                if (player != null)
                {
                    player.PlayAnimation("anim@heists@box_carry@", "idle", 49);
                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_feed_sack_01"), 26611, new Vector3(0, -0.3, 0.075), new Vector3(-45, 20, 120));
                    NAPI.Task.Run(() =>
                    {
                        if (player != null)
                        {
                            player.StopAnimation();
                            player.PlayAnimation("anim@heists@box_carry@", "idle", 49);
                            var nextCheck = WorkManager.rnd.Next(0, ChecksStop.Count - 1);
                            while (nextCheck == player.GetData<int>("WORKCHECK"))
                                nextCheck = WorkManager.rnd.Next(0, ChecksStop.Count - 1);
                            player.SetData("WORKCHECK", nextCheck);
                            Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, ChecksStop[nextCheck].Position, 1, 0, 67, 140, 239);
                            Trigger.PlayerEvent(player, "createWorkBlip", ChecksStop[nextCheck].Position);
                            Trigger.PlayerEvent(player, "client::showhintHUD", false, "Взять мешок");
                        }
                    }, 500);
                }
            }, 1500);
            player.SetData("INTERACTIONCHECK", -1);
            player.SetData("WorkColshape", -1);
        }
        private static void EnterShapeTakeNewCont(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != _JobWorkID || !player.GetData<bool>("ON_WORK") || shape.GetData<int>("NUMBER") != player.GetData<int>("WORKCHECK")) return;
                if (player.HasData("ShapeBuilderState") && player.GetData<bool>("ShapeBuilderState")) return;

                Trigger.PlayerEvent(player, "client::showhintHUD", true, "Взять мешок");
                player.SetData("INTERACTIONCHECK", 931);
                player.SetData("WorkColshape", shape);

            }
            catch (Exception e)
            {
                Log.Write("PlayerEnterCheckpoint: " + e.Message, nLog.Type.Error);
            }
        }
        public static void WorkPutCont(Player player)
        {
            var shape = player.GetData<ColShape>("WorkColshape");

            player.SetData("ShapeBuilderState", false);
            player.SetSharedData("ShapeBuilderState", false);
            NAPI.Entity.SetEntityPosition(player, ChecksStop[shape.GetData<int>("NUMBER2")].Position + new Vector3(0, 0, 1.2));
            NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, ChecksStop[shape.GetData<int>("NUMBER2")].Heading));
            player.SetData("INTERACTIONCHECK", -1);
            player.SetData("WorkColshape", -1);
            Main.OnAntiAnim(player);

            player.PlayAnimation("anim@mp_snowball", "pickup_snowball", 1);
            player.PlayAnimation("anim@heists@box_carry@", "idle", 49);
            NAPI.Task.Run(() =>
            {
                if (player != null)
                {
                    player.StopAnimation();
                    BasicSync.DetachObject(player);
                    MoneySystem.Wallet.Change(player, JobPayment);
                    var nextCheck = WorkManager.rnd.Next(0, Checks.Count - 1);
                    while (nextCheck == player.GetData<int>("WORKCHECK"))
                        nextCheck = WorkManager.rnd.Next(0, Checks.Count - 1);
                    player.SetData("WORKCHECK", nextCheck);
                    Trigger.PlayerEvent(player, "client::showhintHUD", false, "Поставить мешок");
                    Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checks[nextCheck].Position, 1, 0, 255, 0, 0);
                    Trigger.PlayerEvent(player, "createWorkBlip", Checks[nextCheck].Position);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили {JobPayment}$, следующая точка установлена", 3000);
                }
            }, 2000);
            player.SetData("INTERACTIONCHECK", -1);
            player.SetData("WorkColshape", -1);
        }
        private static void EnterShapePutContAndTakeNew(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].WorkID != _JobWorkID || !player.GetData<bool>("ON_WORK") || shape.GetData<int>("NUMBER2") != player.GetData<int>("WORKCHECK")) return;
                if (player.HasData("ShapeBuilderState") && !player.GetData<bool>("ShapeBuilderState")) return;

                Trigger.PlayerEvent(player, "client::showhintHUD", true, "Поставить мешок");
                player.SetData("INTERACTIONCHECK", 932);
                player.SetData("WorkColshape", shape);

            }
            catch (Exception e)
            {
                Log.Write("PlayerEnterCheckpoint: " + e.Message, nLog.Type.Error);
            }
        }
        #region Elevator
        public static void TeleportToRoof(Player player)
        {
            if (player.IsInVehicle) return;
            if (player == null) return;
            Trigger.PlayerEvent(player, "showHUD", false);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        Trigger.PlayerEvent(player, "screenFadeOut", 400);
                    }
                }
                catch { }
            }, 100);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        NAPI.Entity.SetEntityPosition(player, _posRoof);
                        NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -115));
                        Trigger.PlayerEvent(player, "screenFadeIn", 400);
                        Trigger.PlayerEvent(player, "showHUD", true);
                    }
                }
                catch { }
            }, 1000);
        }
        public static void TeleportToGround(Player player)
        {
            if (player.IsInVehicle) return;
            if (player == null) return;
            Trigger.PlayerEvent(player, "showHUD", false);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        Trigger.PlayerEvent(player, "screenFadeOut", 400);
                    }
                }
                catch { }
            }, 100);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        NAPI.Entity.SetEntityPosition(player, _posElevator);
                        Trigger.PlayerEvent(player, "screenFadeIn", 400);
                        Trigger.PlayerEvent(player, "showHUD", true);
                    }
                }
                catch { }
            }, 1000);
        }
        #endregion
        #region ClientEvents
        [RemoteEvent("serverplayerPlayAnimBuilder")]
        public static void serverplayerPlayAnimBuilder(Player player)
        {
            player.PlayAnimation("anim@heists@box_carry@", "idle", 49);
        }
        [RemoteEvent("serverplayerstopboxBuilder")]
        public static void playerstopboxBuilder(Player player)
        {
            player.SetData("ShapeBuilderState", false);
            player.SetSharedData("ShapeBuilderState", false);
            BasicSync.DetachObject(player);
            player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
            var nextCheck = WorkManager.rnd.Next(0, Checks.Count - 1);
            while (nextCheck == player.GetData<int>("WORKCHECK"))
                nextCheck = WorkManager.rnd.Next(0, Checks.Count - 1);
            player.SetData("WORKCHECK", nextCheck);
            Trigger.PlayerEvent(player, "createCheckpoint", 15, 1, Checks[nextCheck].Position, 1, 0, 67, 140, 239);
            Trigger.PlayerEvent(player, "createWorkBlip", Checks[nextCheck].Position);
            Notify.Error(player, "Вы уронили мешок, следующая точка установлена");
        }
        #endregion
        #region Constructor
        private static List<Points> Checks = new List<Points>()
        {
            new Points(new Vector3(-154.47127, -1081.0221, 20.56526), -102),
            new Points(new Vector3(-154.85541, -1080.6384, 29.019403), -103),
            new Points(new Vector3(-169.78923, -1064.0565, 29.019417), -109),
            new Points(new Vector3(-176.9737, -1073.2231, 29.019411), 175),
            new Points(new Vector3(-162.01611, -1075.9753, 35.019356), 66),
            new Points(new Vector3(-176.87253, -1073.3542, 35.019363), 173),
            new Points(new Vector3(-155.9621, -1081.8391, 41.01925), -107),
            new Points(new Vector3(-169.38747, -1049.082, 41.01925), 72),
            new Points(new Vector3(-97.54516, -965.6552, 20.156845), 79),
        };
        private static List<Points> ChecksStop = new List<Points>()
        {
            new Points(new Vector3(-141.82927, -1066.7698, 20.565266), -23),
            new Points(new Vector3(-170.1911, -1071.4585, 29.019405), 157),
            new Points(new Vector3(-178.41185, -1099.4235, 29.019419), -1),
            new Points(new Vector3(-154.96272, -1080.6786, 35.019363), -117),
            new Points(new Vector3(-169.83168, -1064.1018, 35.01936), -109),
            new Points(new Vector3(-184.85513, -1072.8644, 41.049744), 74),
            new Points(new Vector3(-160.83797, -1051.9121, 41.01927), 151),
            new Points(new Vector3(-167.72087, -1099.1355, 41.019363), -14),
            new Points(new Vector3(-163.58441, -985.6222, 20.156843), 56),
        };
        internal class Points
        {
            public Vector3 Position { get; }
            public double Heading { get; }

            public Points(Vector3 pos, double rot)
            {
                Position = pos;
                Heading = rot;
            }
        }
        #endregion
    }
}
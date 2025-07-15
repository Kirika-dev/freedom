using GTANetworkAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using Newtonsoft.Json;
using static NeptuneEVO.Core.Quests;

namespace NeptuneEVO.Jobs
{
    class WorkManager : Script
    {
        private static nLog Log = new nLog("WorkManager");
        public static Random rnd = new Random();

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {				
                NAPI.Blip.CreateBlip(89, Jobs.Gopostal.Coords[1], 1f, 4, Main.StringToU16("Главный офис GoPostal"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(650, Miner.Position, 0.8f, 17, Main.StringToU16("Карьер"), 255, 0, true, 0, 0);


                new MarketNPC(2, "MNPC_SellerOrePile", "Этан Стоукс", "Продажа ресурсов", new Vector3(182.11086, 2790.9521, 44.502293));
                NAPI.Blip.CreateBlip(467, new Vector3(182.11086, 2790.9521, 44.502293), 0.8f, 1, Main.StringToU16("Продажа ресурсов"), 255, 0, true, 0, 0);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        private static SortedDictionary<int, ColShape> Cols = new SortedDictionary<int, ColShape>();
        public static List<string> JobStats = new List<string>
        {
            "Доставщик Delivery Club",  //
            "Почтальон",
            "Таксист",
            "Водитель автобуса",
            //"Газонокосильщик",
            "Дальнобойщик",
            "Инкассатор",
			//"Автомеханик",
			//"Тракторист",  //
            //"Мусоровозчик",//
            "Строитель",
            //"Каменщик",
			//"Поисковик",
            //"Уборка штата"  //
        };
        public static SortedList<int, Vector3> Points = new SortedList<int, Vector3>
        {
            {0, new Vector3(-1029.837, -1402.057, 4.437821) },  // Трудоустройство
            {1, new Vector3(724.9625, 133.9959, 79.83643) },  // Electrician job
            {2, new Vector3(105.4633, -1568.843, 28.60269) },  // Postal job
            {3, new Vector3(903.3215,-191.7,73.40494) },      // Taxi job
            {4, new Vector3(406.2858, -649.6152, 28.49641) }, // Bus driver job
            {5, new Vector3(-1331.475, 53.58579, 53.53268) },  // Газонокосильщик
            {6, new Vector3(588.2037, -3037.641, 6.303829) },  // Trucker job
            {7, new Vector3(915.9069, -1265.255, 25.52912) },  // Collector job
            //{8, new Vector3(473.9508, -1275.597, 29.60513) },  // AutoMechanic job
			{9, new Vector3(2923.87, 4679.295, 48.71234) },  // Тракторист
            {10, new Vector3(1090.414, -2234.13, 30.18403) },  // Мусоровозчик
            {14, new Vector3(906.3733, -1516.33, 29.29401) },  // Уборка штата
        };
        private static SortedList<int, string> JobList = new SortedList<int, string>
        {
            {1, "доставщиком" },
            {2, "почтальоном" },
            {3, "таксистом" },
            {4, "водителем автобуса" },
            {5, "газонокосильщиком" },
            {6, "дальнобойщиком" },
            {7, "инкассатором" },
            {8, "автомехаником" },
			{9, "тракторист" },
            {10, "мусоровозщик" },
            {11, "Строитель" },
            {12, "Каменщик" },
            {13, "Поисковик" },
            {14, "уборщик" }
        };
        private static SortedList<int, int> JobsMinLVL = new SortedList<int, int>()
        {
            { 1, 0 },
            { 2, 1 },
            { 3, 1 },
            { 4, 2 },
            { 5, 0 },
            { 6, 5 },
            { 7, 4 },
            { 8, 4 },
			{ 9, 1 },
            { 10, 3},
            { 11, 1},
            { 12, 1},
            { 13, 1},
            { 14, 1}
        };

        public static void Layoff(Player player)
        {
            if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны сначала закончить рабочий день", 3000);
                return;
            }
            if (Main.Players[player].WorkID != 0)
            {
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы уволились с работы", 3000);
                Main.Players[player].WorkID = 0;
                Trigger.PlayerEvent(player, "showJobMenu", Main.Players[player].LVL, Main.Players[player].WorkID);
            }
            else
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы безработный", 3000);
        }
        public static void JobJoin(Player player, int job)
        {

            if (Main.Players[player].WorkID != 0)
            {
                if (NAPI.Data.GetEntityData(player, "ON_WORK") == true)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны сначала закончить рабочий день", 3000);
                    return;
                }
                Layoff(player);
            }

            if (Main.Players[player].WorkID == job)
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже работаете {JobList[job]}", 3000);
            else
            {
                if (Main.Players[player].LVL < JobsMinLVL[job])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Необходим как минимум {JobsMinLVL[job]} уровень", 3000);
                    return;
                }
                if ((job == 3 || job == 8) && !Main.Players[player].Licenses[1])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории B", 3000);
                    return;
                }
                if ((job == 4 || job == 6 || job == 7 || job == 10) && !Main.Players[player].Licenses[2])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет лицензии категории C", 3000);
                    return;
                }
                Main.Players[player].WorkID = job;
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы устроились работать {JobList[job]}. Доберитесь до точки начала работы", 3000);
                Trigger.PlayerEvent(player, "createWaypoint", Points[job].X, Points[job].Y);
                Trigger.PlayerEvent(player, "showJobMenu", Main.Players[player].LVL, Main.Players[player].WorkID);
            }
        }
        // REQUIRED REFACTOR //
        public static void load(Player player)
        {
            NAPI.Data.SetEntityData(player, "ON_WORK", false);
            NAPI.Data.SetEntityData(player, "PAYMENT", 0);
            NAPI.Data.SetEntityData(player, "BUS_ONSTOP", false);
            NAPI.Data.SetEntityData(player, "IS_CALL_TAXI", false);
            NAPI.Data.SetEntityData(player, "IS_REQUESTED", false);
            NAPI.Data.SetEntityData(player, "IN_WORK_CAR", false);
            player.SetData("PACKAGES", 0);
            NAPI.Data.SetEntityData(player, "WORK", null);
            NAPI.Data.SetEntityData(player, "WORKWAY", -1);
            NAPI.Data.SetEntityData(player, "IS_PRICED", false);
            NAPI.Data.SetEntityData(player, "ON_DUTY", false);
            NAPI.Data.SetEntityData(player, "CUFFED", false);
            NAPI.Data.SetEntityData(player, "IN_CP_MODE", false);
            NAPI.Data.SetEntityData(player, "WANTED", 0);
            NAPI.Data.SetEntityData(player, "REQUEST", "null");
            player.SetData("IS_IN_ARREST_AREA", false);
            player.SetData("PAYMENT", 0);
            player.SetData("INTERACTIONCHECK", 0);
            player.SetData("IN_HOSPITAL", false);
            player.SetData("MEDKITS", 0);
            player.SetData("GANGPOINT", -1);
            player.SetData("CUFFED_BY_COP", false);
            player.SetData("CUFFED_BY_MAFIA", false);
            player.SetData("IS_CALL_MECHANIC", false);
            NAPI.Data.SetEntityData(player, "CARROOM_CAR", null);
        }

        #region Jobs
        #region Jobs Selecting
        public static void openJobsSelecting(Player player, int id)
        {
            Trigger.PlayerEvent(player, "showJobMenu", Main.Players[player].LVL, Main.Players[player].WorkID, id);
        }
        [RemoteEvent("jobjoin")]
        public static void callback_jobsSelecting(Player Player, int act)
        {
            try
            {
                switch (act)
                {
                    case -1:
                        Layoff(Player);
                        return;
                    default:
                        JobJoin(Player, act);
                        return;
                }
            }
            catch (Exception e) { Log.Write("jobjoin: " + e.Message, nLog.Type.Error); }
        }
        #endregion
        #region GoPostal Job
        public static void openGoPostalStart(Player player)
        {
            Trigger.PlayerEvent(player, "NPC.cameraOn", "GOPOSTAL", 1000);
            if (Main.Players[player].WorkID != 2)
            {
                Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Джэймс Фостер", "Работа", "Приветсвую, хочешь порабоать почтальоном? Помоги мне немного тут, получишь деньги?", (new QuestAnswer("Как тут работать?", 20), new QuestAnswer("Устроиться", 21), new QuestAnswer("В другой раз", 2)));
                return;
            }
            else if (Main.Players[player].WorkID == 2 && player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK") == false)
            {
                Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Джэймс Фостер", "Работа", "Приветсвую, хочешь порабоать почтальоном? Помоги мне немного тут, получишь деньги?", (new QuestAnswer("Как тут работать?", 20), new QuestAnswer("Уволиться", 21), new QuestAnswer("Начать работу", 22), new QuestAnswer("В другой раз", 2)));
                return;
            }
            else if (Main.Players[player].WorkID == 2 && player.HasData("ON_WORK") && player.GetData<bool>("ON_WORK") == true)
            {
                Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Джэймс Фостер", "Работа", "Приветсвую, хочешь порабоать почтальоном? Помоги мне немного тут, получишь деньги?", (new QuestAnswer("Как тут работать?", 20), new QuestAnswer("Уволиться", 21), new QuestAnswer("Закончить работу", 24), new QuestAnswer("Взять посылки", 23), new QuestAnswer("В другой раз", 2)));
                return;
            }
        }
        public static void SetWorkId(Player player)
        {
            if (Main.Players[player].WorkID != 2)
            {
                if (Main.Players[player].WorkID != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Вы уже работаете: {Jobs.WorkManager.JobStats[Main.Players[player].WorkID - 1]}", 3000);
                    return;
                }
                Main.Players[player].WorkID = 2;
                Notify.Succ(player, "Вы устроились на работу почтальоном");
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
                    if (Main.Players[player].WorkID == 2)
                    {
                        Main.Players[player].WorkID = 0;
                        Notify.Succ(player, "Вы уволились с работы почтальона");
                    }
                }
            }
        }
        public static void callback_gpStartMenu(Player player, int id)
        {
            switch (id)
            {
                case 0:
                    if (Main.Players[player].WorkID == 2)
                    {
                        if (!NAPI.Data.GetEntityData(player, "ON_WORK"))
                        {
                            if (Houses.HouseManager.Houses.Count == 0) return;
                            player.SetData("PACKAGES", 15);
                            Trigger.PlayerEvent(player, "client::postal:hud", player.GetData<int>("PACKAGES"));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили 15 посылок, развезите их по домам", 3000);
                            player.SetData("ON_WORK", true);

                            player.SetData("PAYMENT", 0);
                            Trigger.PlayerEvent(player, "JobStatsInfo", player.GetData<int>("PAYMENT"));

                            player.SetData("W_LASTPOS", player.Position);
                            player.SetData("W_LASTTIME", DateTime.Now);
                            var next = Jobs.WorkManager.rnd.Next(0, Houses.HouseManager.Houses.Count - 1);
                            while (Houses.HouseManager.Houses[next].Position.DistanceTo2D(player.Position) < 200)
                                next = Jobs.WorkManager.rnd.Next(0, Houses.HouseManager.Houses.Count - 1);

                            player.SetData("NEXTHOUSE", Houses.HouseManager.Houses[next].ID);
                            Trigger.PlayerEvent(player, "createCheckpoint", 1, 1, Houses.HouseManager.Houses[next].Position, 1, 0, 254, 169, 66);
                            Trigger.PlayerEvent(player, "createWaypoint", Houses.HouseManager.Houses[next].Position.X, Houses.HouseManager.Houses[next].Position.Y);
                            Trigger.PlayerEvent(player, "createWorkBlip", Houses.HouseManager.Houses[next].Position);

                            Jobs.Gopostal.getGoPostalCar(player);
                            int x = Jobs.WorkManager.rnd.Next(0, Gopostal.GoPostalObjects.Count);
                            BasicSync.AttachObjectToPlayer(player, Jobs.Gopostal.GoPostalObjects[x], 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
                        }
                        else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже начали рабочий день", 3000);
                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете почтальоном", 3000);
                    return;
                case 1:
                    {
                        if (Main.Players[player].WorkID != 2)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете почтальоном", 3000);
                            return;
                        }
                        if (!player.GetData<bool>("ON_WORK"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не начали рабочий день", 3000);
                            return;
                        }
                        if (player.GetData<int>("PACKAGES") != 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не раздали все посылки. У Вас осталось ещё {player.GetData<int>("PACKAGES")} штук", 3000);
                            return;
                        }
                        if (Houses.HouseManager.Houses.Count == 0) return;
                        player.SetData("PACKAGES", 15);
                        Trigger.PlayerEvent(player, "client::postal:hud", player.GetData<int>("PACKAGES"));
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили 15 посылок. Развезите их по домам", 3000);

                        player.SetData("W_LASTPOS", player.Position);
                        player.SetData("W_LASTTIME", DateTime.Now);
                        var next = Jobs.WorkManager.rnd.Next(0, Houses.HouseManager.Houses.Count);
                        while (Houses.HouseManager.Houses[next].Position.DistanceTo2D(player.Position) < 200)
                            next = Jobs.WorkManager.rnd.Next(0, Houses.HouseManager.Houses.Count);

                        player.SetData("NEXTHOUSE", Houses.HouseManager.Houses[next].ID);

                        Trigger.PlayerEvent(player, "createCheckpoint", 1, 1, Houses.HouseManager.Houses[next].Position, 1, 0, 254, 169, 66);
                        Trigger.PlayerEvent(player, "createWaypoint", Houses.HouseManager.Houses[next].Position.X, Houses.HouseManager.Houses[next].Position.Y);
                        Trigger.PlayerEvent(player, "createWorkBlip", Houses.HouseManager.Houses[next].Position);

                        int y = Jobs.WorkManager.rnd.Next(0, Jobs.Gopostal.GoPostalObjects.Count);
                        BasicSync.AttachObjectToPlayer(player, Jobs.Gopostal.GoPostalObjects[y], 60309, new Vector3(0.03, 0, 0.02), new Vector3(0, 0, 50));
                        return;
                    }
                case 2:
                    if (Main.Players[player].WorkID == 2)
                    {
                        if (NAPI.Data.GetEntityData(player, "ON_WORK"))
                        {
                            Trigger.PlayerEvent(player, "deleteCheckpoint", 1, 0);
                            BasicSync.DetachObject(player);
                            Trigger.PlayerEvent(player, "deleteWorkBlip");

                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили зарплату в размере: {player.GetData<int>("PAYMENT")}$", 4000);
                            MoneySystem.Wallet.Change(player, player.GetData<int>("PAYMENT"));
                            Trigger.PlayerEvent(player, "CloseJobStatsInfo", player.GetData<int>("PAYMENT"));

                            player.SetData("PAYMENT", 0);
                            /* Customization.ApplyCharacter(client);*/
                            if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                            else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);

                            player.SetData("PACKAGES", 0);
                            Trigger.PlayerEvent(player, "client::postal:hud", player.GetData<int>("PACKAGES"));
                            player.SetData("ON_WORK", false);

                            if (player.GetData<Vehicle>("WORK") != null)
                            {
                                NAPI.Entity.DeleteEntity(player.GetData<Vehicle>("WORK"));
                                player.SetData<Vehicle>("WORK", null);
                            }
                        }
                        else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете", 3000);

                    }
                    else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не работаете почтальоном", 3000);
                    return;
            }
        }
        #endregion

        public class WorkPoint
        {
            public int Job { get; set; }
            public Vector3 Pos { get; set; }
            public int Heading { get; set; }
            public string Name { get; set; }
            public int Type { get; set; }
            public bool Interact { get; set; }
            [JsonIgnore]
            public ColShape Shape { get; set; }

            public WorkPoint(int job, Vector3 pos, int heading, string name, bool interact = false)
            {
                Pos = pos;Heading = heading;Job = job;Name = name;Interact = interact;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"~b~{Name}"), pos + new Vector3(0, 0, 2.2f), 10F, 0.3F, 0, new Color(255, 255, 255));
                NAPI.Marker.CreateMarker(27, Pos + new Vector3(0, 0, 0.12f), new Vector3(), new Vector3(), 1f, new Color(0, 86, 214, 220), false, 0);
                Shape = NAPI.ColShape.CreateCylinderColShape(pos, 1.2f, 3);
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        if (!Main.Players.ContainsKey(entity)) return;
                        if (Interact) entity.SetData("INTERACTIONCHECK", 75);
                        else entity.SetData("INTERACTIONCHECK", 56);
                        entity.SetData("JOBID", Job);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        
                        entity.SetData("INTERACTIONCHECK", 0);
                        entity.SetData("JOBID", -1);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };

            }
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;

namespace NeptuneEVO.Fractions
{
    class GangsCapture : Script
    {
        private static nLog Log = new nLog("GangCapture");
        //private static Config config = new Config("GangCapture");
        public static Dictionary<int, GangPoint> gangPoints = new Dictionary<int, GangPoint>();
        public static bool captureIsGoing = false;
        public static bool captureStarting = false;
        private static string captureTimer;
        private static string toStartCaptureTimer;
        private static int attackersFracID = -1;
        private static int defendersFracID = -1;
        public static Vector3 captureRegion = new Vector3();
        private static int timerCount = 0;
        private static int timerExitCountDef = 0;
        private static int timerExitCountAt = 0;
        private static int attackersSt = 0;
        private static int defendersSt = 0;
        private static bool defendersWas = false;
        private static bool attackersWas = false;
        private static bool smbTryCapture = false;
        public static Dictionary<int, int> gangPointsColor = new Dictionary<int, int>
        {
            { 1, 11 }, // families
            { 2, 50 }, // ballas
            { 3, 70 }, // vagos
            { 4, 53 }, // marabunta
            { 5, 59 }, // blood street
        };
        private static Dictionary<int, string> pictureNotif = new Dictionary<int, string>
        {
            { 1, "CHAR_MP_FAM_BOSS" }, // families
            { 2, "CHAR_MP_GERALD" }, // ballas
            { 3, "CHAR_ORTEGA" }, // vagos
            { 4, "CHAR_MP_ROBERTO" }, // marabunta
            { 5, "CHAR_MP_SNITCH" }, // blood street
        };
        private static Dictionary<int, DateTime> nextCaptDate = new Dictionary<int, DateTime>
        {
            { 1, DateTime.Now },
            { 2, DateTime.Now },
            { 3, DateTime.Now },
            { 4, DateTime.Now },
            { 5, DateTime.Now },
        };
        private static Dictionary<int, DateTime> protectDate = new Dictionary<int, DateTime>
        {
            { 1, DateTime.Now },
            { 2, DateTime.Now },
            { 3, DateTime.Now },
            { 4, DateTime.Now },
            { 5, DateTime.Now },
        };
        public static List<Vector3> gangZones = new List<Vector3>()
        {
           new Vector3(1481.6602, -1435.6931, 67.9863),// МЕРОР респа / ТУпик мерора
            new Vector3(1481.6602, -1575.6931, 67.9863),//МЕРОР Слево от квадрата снизу
            new Vector3(1341.6602, -1575.6931, 67.9863),//Мерор Правый крайний квадрат
            new Vector3(1061.6602, -1715.6931, 67.9863),// МЕРОР Титульник / ТУпик мерора
            new Vector3(1201.6602, -1715.6931, 67.9863),// мерор
            new Vector3(1341, -1715, 67.9863),// meror Park
            new Vector3(1201.6602, -1575.6931, 67.9863),
            new Vector3(1201, -2415, 67.9863),
            new Vector3(1341, -1855.6931, 67.9863),
            new Vector3(1341, -1995, 67.9863),// мерор
            new Vector3(1341, -2135.6931, 67.9863),// meror Park
            new Vector3(1201.6602, -1855.6931, 67.9863),
            new Vector3(1201.6602, -1995.6931, 67.9863),
            new Vector3(1201.6602, -2135.6931, 30.18104),
            new Vector3(1061.6602, -1855.6931, 30.18104),
            new Vector3(1061.6602, -1995.6931, 30.18104),
            new Vector3(1061.6602, -2275.6931, 30.36028),
            new Vector3(1201.6602, -2275.8256, 30.36028),
            new Vector3(1061.6602, -2135.6931, 30.18104),
            new Vector3(1061.6602, -2415.6931, 30.36028),
            new Vector3(921.6602, -2415.6931, 30.36028),
            new Vector3(921.6602, -2135.6931, 30.36028),
            new Vector3(921.6602, -1855.6931, 30.36028),
            new Vector3(921.6602, -1995.6931, 30.36028),
            new Vector3(921.6602, -1715.6931, 30.36028),
            new Vector3(921.6602, -1575.6931, 30.36028),
            new Vector3(921.6602, -2275.6931, 30.36028),
            new Vector3(921.6602, -1435.6931, 30.36028),
            new Vector3(781.6602, -2415.6931, 67.30145),
            new Vector3(781.6602, -2135.6931, 67.30145),
            new Vector3(781.6602, -1855.6931, 67.30145),
            new Vector3(781.6602, -1995.6931, 67.30145),
            new Vector3(781.6602, -1715.6931, 67.30145),
            new Vector3(781.6602, -1575.6931, 67.30145),
            new Vector3(781.6602, -2275.6931, 67.30145),
            new Vector3(781.6602, -1435.6931, 67.30145),
            new Vector3(511, -1315, 28.146452),
            new Vector3(511, -1455, 28.146452),
            new Vector3(511, -1595, 28.146452),
            new Vector3(511, -1735, 28.146452),
            new Vector3(511, -1875, 28.146452),
            new Vector3(511, -2015, 28.146452),
            new Vector3(511, -2155, 28.146452),
            new Vector3(371, -1315, 28.146452),
            new Vector3(371, -1455, 28.146452),
            new Vector3(371, -1595, 28.146452),
            new Vector3(371, -1735, 28.146452),
            new Vector3(371, -1875, 28.146452),
            new Vector3(371, -2015, 28.146452),
            new Vector3(371, -2155, 28.146452),
            new Vector3(231, -1315, 28.146452),
            new Vector3(231, -1455, 28.146452),
            new Vector3(231, -1595, 28.146452),
            new Vector3(231, -1735, 28.146452),
            new Vector3(231, -1875, 28.146452),
            new Vector3(231, -2015, 28.146452),
            new Vector3(231, -2155, 28.146452),
            new Vector3(91, -1455, 28.146452),
            new Vector3(91, -1595, 28.146452),
            new Vector3(91, -1735, 28.146452),
            new Vector3(91, -1875, 28.146452),
            new Vector3(91, -2015, 28.146452),
            new Vector3(91, -2155, 28.146452),
            new Vector3(-49, -1455, 28.146452),
            new Vector3(-49, -1595, 28.146452),
            new Vector3(-49, -1735, 28.146452),
            new Vector3(-49, -1875, 28.146452),
            new Vector3(-189, -1455, 28.146452),
            new Vector3(-189, -1595, 28.146452),
            new Vector3(-189, -1735, 28.146452),
        };

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead("SELECT * FROM gangspoints");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow Row in result.Rows)
                {
                    var data = new GangPoint();
                    data.ID = Convert.ToInt32(Row["id"]);
                    data.GangOwner = Convert.ToInt32(Row["gangid"]);
                    data.IsCapture = false;

                    if (data.ID >= gangZones.Count) break;
                    gangPoints.Add(data.ID, data);
                }
                foreach (var gangpoint in gangPoints.Values)
                {
                    var colShape = NAPI.ColShape.Create2DColShape(gangZones[gangpoint.ID].X, gangZones[gangpoint.ID].Y, 140, 140);
                    colShape.OnEntityEnterColShape += onPlayerEnterGangPoint;
                    colShape.OnEntityExitColShape += onPlayerExitGangPoint;
                    colShape.SetData("ID", gangpoint.ID);
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT\"FRACTIONS_CAPTURE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static void CMD_startCapture(Player player)
        {
            if (!Manager.canUseCommand(player, "capture")) return;
            if (player.GetData<int>("GANGPOINT") == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не находитесь ни на одном из регионов", 3000);
                return;
            }
            GangPoint region = gangPoints[player.GetData<int>("GANGPOINT")];
            if (region.GangOwner == Main.Players[player].FractionID)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете напасть на свою территорию", 3000);
                return;
            }
            if (DateTime.Now.Hour < 14 || DateTime.Now.Hour > 23)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы можете напасть только с 14:00 до 23:00", 3000);
                return;
            }
            if (DateTime.Now < nextCaptDate[Main.Players[player].FractionID])
            {
                DateTime g = new DateTime((nextCaptDate[Main.Players[player].FractionID] - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы сможете начать захват только через {min}:{sec}", 3000);
                return;
            }
            if (DateTime.Now < protectDate[region.GangOwner])
            {
                DateTime g = new DateTime((protectDate[region.GangOwner] - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы сможете начать захват территории этой банды только через {min}:{sec}", 3000);
                return;
            }
            if (Manager.countOfFractionMembers(region.GangOwner) < 3)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточный онлайн в банде противников", 3000);
                return;
            }
            if (smbTryCapture) return;
            smbTryCapture = true;
            if (captureStarting || captureIsGoing)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Захват территории уже идёт", 3000);
                smbTryCapture = false;
                return;
            }
            timerCount = 0;
            timerExitCountDef = 0;                                                                

            timerExitCountAt = 0;
            attackersSt = 0;
            defendersSt = 0;
            region.IsCapture = true;
            attackersFracID = Main.Players[player].FractionID;
            defendersFracID = region.GangOwner;
            captureRegion = new Vector3(gangZones[region.ID].X, gangZones[region.ID].Y, gangZones[region.ID].Z);
            toStartCaptureTimer = Timers.StartOnce(600000, () => timerStartCapture(region));
            Main.PlayerEventToAll("setZoneFlash", region.ID, true, gangPointsColor[region.GangOwner]);

            captureStarting = true;
            smbTryCapture = false;

            Manager.sendFractionMessage(region.GangOwner, $"Внимание! Сбор в течении 10 минут! {Manager.getName(attackersFracID)} решили отхватить нашу территорию");
            Manager.sendFractionMessage(attackersFracID, "Стреляй! Отжимай! Примерно через 10 минут подлетят противники");
        }

        private static void timerStartCapture(GangPoint region)
        {
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p) || p.Position.DistanceTo(captureRegion) > 65) continue;
                if (Main.Players[p].FractionID == region.GangOwner || Main.Players[p].FractionID == attackersFracID)
                {
                    Trigger.PlayerEvent(p, "sendCaptureInformation", 0, 0, 0, 0); // отправляем первый раз кол-во киллов
                    Trigger.PlayerEvent(p, "captureHud", true);
                    Trigger.PlayerEvent(p, "sendGangName", Manager.getName(attackersFracID), Manager.getName(defendersFracID));
                }
            }
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p) || p.Position.DistanceTo(captureRegion) > 65) continue;
                if (Main.Players[p].FractionID == region.GangOwner || Main.Players[p].FractionID == attackersFracID)
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            NAPI.Entity.SetEntityDimension(p, (uint)region.ID + 1000);// перемещаем всех в другой дименшен
                            p.SetData("ISCAPTURE", true);
                        }
                        catch { }
                    }, 100);
                }
            }
            captureIsGoing = true;
            captureStarting = false;
            captureTimer = Timers.Start(1000, () => timerUpdate(region, region.ID));

            Manager.sendFractionMessage(region.GangOwner, $"Внимание! На нас напали! {Manager.getName(attackersFracID)} решили отхватить нашу территорию");
            Manager.sendFractionMessage(attackersFracID, "Стреляй! Отжимай! Вы начали войну за территорию");
        }

        private static void timerUpdate(GangPoint region, int id)
        {
            try
            {
                var attackers = 0;
                var defenders = 0;

                var allplayers = NAPI.Pools.GetAllPlayers();
                foreach (var p in allplayers)
                {
                    if (!Main.Players.ContainsKey(p) || p.Position.DistanceTo(captureRegion) > 65) continue;
                    if (Main.Players[p].FractionID == region.GangOwner) defenders++;
                    else if (Main.Players[p].FractionID == attackersFracID) attackers++;
                }
                if (!defendersWas && defenders != 0)
                    defendersWas = true;
                if (!attackersWas && defendersWas)
                    attackersWas = true;

                if (defenders != 0) timerExitCountDef = 0;
                if (attackers != 0) timerExitCountAt = 0;

                if (defendersWas && defenders == 0)
                {
                    timerExitCountDef++;
                    if (timerExitCountDef >= 60)
                    {
                        endCapture(region, 0, 1);
                        return;
                    }
                }
                if (attackersWas && attackers == 0)
                {
                    timerExitCountAt++;
                    if (timerExitCountAt >= 60)
                    {
                        endCapture(region, 1, 0);
                        return;
                    }
                }

                if (timerCount >= 480 && !defendersWas)
                    endCapture(region, defenders, attackers);

                timerCount++;
                foreach (var p in allplayers)
                {
                    if (!Main.Players.ContainsKey(p) || p.Position.DistanceTo(captureRegion) > 65) continue;
                    if (Main.Players[p].FractionID == region.GangOwner || Main.Players[p].FractionID == attackersFracID)
                    {
                        int minutes = timerCount / 60;
                        int seconds = timerCount % 60;
                        Trigger.PlayerEvent(p, "sendCaptureInformation", attackersSt, defendersSt, minutes, seconds);
                    }
                }
            }
            catch (Exception e) { Log.Write("GangCapture: " + e.Message, nLog.Type.Error); }
        }

        private static void endCapture(GangPoint region, int defenders, int attackers)
        {
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p) || p.Position.DistanceTo(captureRegion) > 65) continue;
                if (Main.Players[p].FractionID == region.GangOwner || Main.Players[p].FractionID == attackersFracID)
                {
                    NAPI.Task.Run(() => {
                        try
                        {
                            NAPI.Entity.SetEntityDimension(p, 0);// перемещаем всех в другой дименшен
                            p.SetData("ISCPATURE", false);
                        }
                        catch { }
                    }, 100);
                }
            }
            Timers.Stop(captureTimer);
            NAPI.Task.Run(() => Main.PlayerEventToAll("captureHud", false));
            protectDate[region.GangOwner] = DateTime.Now.AddMinutes(20);
            protectDate[attackersFracID] = DateTime.Now.AddMinutes(20);
            if (attackers <= defenders)
            {
                Manager.sendFractionMessage(region.GangOwner, $"Обсосы сбежали! Вы дали им под хвост! Вы отстояли территорию");
                Manager.sendFractionMessage(attackersFracID, "Вы лохонулись! Враги были сильнее! Вы не смогли захватить территорию");
                foreach (var m in Manager.Members.Keys)
                {
                    if (Main.Players[m].FractionID == region.GangOwner)
                    {
                        MoneySystem.Wallet.Change(m, 500000);
                        GameLog.Money($"server", $"player({Main.Players[m].UUID})", 500000, $"winCapture");
                    }
                }
            }
            else if (attackers > defenders)
            {
                Manager.sendFractionMessage(region.GangOwner, $"Вы прошляпили территорию");
                Manager.sendFractionMessage(attackersFracID, "Шугнули их как детей! Вы захватили территорию");
                region.GangOwner = attackersFracID;
                MySQL.Query($"UPDATE `gangspoints` SET `gangid`='{attackersFracID}' WHERE `id`='{region.ID}'");
                foreach (var m in Manager.Members.Keys)
                {
                    if (Main.Players[m].FractionID == attackersFracID)
                    {
                        MoneySystem.Wallet.Change(m, 250000);
                        GameLog.Money($"server", $"player({Main.Players[m].UUID})", 250000, $"winCapture");
                    }
                }
            }
            DateTime nextcapt = DateTime.Now.AddMinutes(30);
            nextCaptDate[attackersFracID] = nextcapt;
            region.IsCapture = false;
            captureIsGoing = false;
            NAPI.Task.Run(() =>
            {
                Main.PlayerEventToAll("setZoneFlash", region.ID, false);
                Main.PlayerEventToAll("setZoneColor", region.ID, gangPointsColor[region.GangOwner]);
            });
        }

        private static void onPlayerEnterGangPoint(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.Dimension == 0 && captureIsGoing) return;
                if (Main.Players[player].FractionID >= 1 && Main.Players[player].FractionID <= 5 || Main.Players[player].AdminLVL > 0)
                {

                    //Log.Write($"Gangsta {player.Name} entered gangPoint");
                    player.SetData("GANGPOINT", (int)shape.GetData<int>("ID"));
                    GangPoint region = gangPoints[(int)shape.GetData<int>("ID")];
                    if (region.IsCapture && captureIsGoing && (Main.Players[player].FractionID == attackersFracID || Main.Players[player].FractionID == region.GangOwner))
                    {
                        int minutes = timerCount / 60;
                        int seconds = timerCount % 60;
                        Trigger.PlayerEvent(player, "sendCaptureInformation", attackersSt, defendersSt, minutes, seconds);
                        Trigger.PlayerEvent(player, "captureHud", true);
                        Trigger.PlayerEvent(player, "sendGangName", Manager.getName(attackersFracID), Manager.getName(defendersFracID));
                    }
                }
            }
            catch (Exception ex) { Log.Write("onPlayerEnterGangPoint: " + ex.Message, nLog.Type.Error); }
        }

        private static void onPlayerExitGangPoint(ColShape shape, Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].FractionID >= 1 && Main.Players[player].FractionID <= 5 || Main.Players[player].AdminLVL > 0)
                {

                    //Log.Write($"Gangsta {player.Name} exited gangPoint");
                    if (shape.GetData<int>("ID") == player.GetData<int>("GANGPOINT"))
                        player.SetData("GANGPOINT", -1);

                    GangPoint region = gangPoints[(int)shape.GetData<int>("ID")];
                    if (region.IsCapture && (Main.Players[player].FractionID == attackersFracID || Main.Players[player].FractionID == region.GangOwner))
                        Trigger.PlayerEvent(player, "captureHud", false);
                }
            }
            catch (Exception ex) { Log.Write("onPlayerExitGangPoint: " + ex.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void onPlayerDeathHandler(Player player, Player killer, uint weapon)
        {
            try
            {
                if (captureIsGoing)
                {
                    if (Convert.ToString(killer) != "")
                    {
                        if (Main.Players[player].FractionID == attackersFracID && Main.Players[killer].FractionID == defendersFracID || Main.Players[player].FractionID == defendersFracID && Main.Players[killer].FractionID == attackersFracID)
                        {
                            if (player.Position.DistanceTo(captureRegion) < 60 && killer.Position.DistanceTo(captureRegion) < 60)
                            {
                                var deadplayerName = $"{Main.Players[player].FirstName}_{Main.Players[player].LastName}";
                                var killerName = $"{Main.Players[killer].FirstName}_{Main.Players[killer].LastName}";
                                string frac1 = "fraction" + Main.Players[killer].FractionID;
                                string frac2 = "fraction" + Main.Players[player].FractionID;
                                object data = new
                                {
                                    killer = killerName,
                                    frac1 = frac1,
                                    deadplayerName = deadplayerName,
                                    frac2 = frac2,
                                    weapon = weapon
                                };
                                //Console.WriteLine(weapon);
                                if (Main.Players[killer].FractionID == attackersFracID)
                                {
                                    attackersSt++;
                                    Console.WriteLine(attackersSt);
                                }
                                else if (Main.Players[killer].FractionID == defendersFracID)
                                {
                                    defendersSt++;
                                    Console.WriteLine(defendersSt);
                                }
                                foreach (Player p in NAPI.Pools.GetAllPlayers())
                                {
                                    if (!Main.Players.ContainsKey(p) || !p.HasData("GANGPOINT")) continue;
                                    Trigger.PlayerEvent(p, "sendkillinfo", Newtonsoft.Json.JsonConvert.SerializeObject(data));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Main.Players[player].FractionID == attackersFracID || Main.Players[player].FractionID == defendersFracID)
                        {
                            if (player.Position.DistanceTo(captureRegion) < 60)
                            {
                                string frac2 = "fraction" + Main.Players[player].FractionID;
                                var deadplayerName = $"{Main.Players[player].FirstName}_{Main.Players[player].LastName}";
                                object data = new
                                {
                                    killer = deadplayerName,
                                    frac1 = frac2,
                                    deadplayerName = "null",
                                    frac2 = "null",
                                    weapon = weapon
                                };
                                if (Main.Players[killer].FractionID == attackersFracID)
                                {
                                    defendersSt++;
                                    Console.WriteLine(attackersSt);
                                }
                                else if (Main.Players[killer].FractionID == defendersFracID)
                                {
                                    attackersSt++;
                                    Console.WriteLine(defendersSt);
                                }
                                foreach (Player p in NAPI.Pools.GetAllPlayers())
                                {
                                    if (!Main.Players.ContainsKey(p) || !p.HasData("GANGPOINT")) continue;
                                    Trigger.PlayerEvent(p, "sendkillinfo", Newtonsoft.Json.JsonConvert.SerializeObject(data));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) { Log.Write("captureDeath: " + e.Message, nLog.Type.Error); }
        }
        public static void SavingRegions()
        {
            foreach (var region in gangPoints.Values)
                MySQL.Query($"UPDATE gangspoints SET gangid={region.GangOwner} WHERE id={region.ID}");
            Log.Write("Saved!", nLog.Type.Save);
        }

        public static void LoadBlips(Player player)
        {
            var colors = new List<int>();
            foreach (var g in gangPoints.Values)
                colors.Add(gangPointsColor[g.GangOwner]);

            Trigger.PlayerEvent(player, "loadCaptureBlips", Newtonsoft.Json.JsonConvert.SerializeObject(colors));

            if (captureIsGoing || captureStarting) Trigger.PlayerEvent(player, "setZoneFlash", gangPoints.FirstOrDefault(g => g.Value.IsCapture == true).Value.ID, true);
        }
        public static void UnLoadBlips(Player player)
        {
            if (Main.Players[player].AdminLVL > 0 || Manager.FractionTypes[Main.Players[player].FractionID] == 1) return;
            Trigger.PlayerEvent(player, "unloadCaptureBlips");
        }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop()
        {
            try
            {
                SavingRegions();
            }
            catch (Exception e) { Log.Write("ResourceStop: " + e.Message, nLog.Type.Error); }
        }

        public class GangPoint
        {
            public int ID { get; set; }
            public int GangOwner { get; set; }
            public bool IsCapture { get; set; }
        }
    }
}

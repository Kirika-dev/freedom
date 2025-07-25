﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Data;
using NeptuneEVO.GUI;
using NeptuneEVO.SDK;
using NeptuneEVO.Businesses;
using MySqlConnector;

namespace NeptuneEVO.Core
{
    class Admin : Script
    {
        private static nLog Log = new nLog("Admin");
        public static bool IsServerStoping = false;

        [ServerEvent(Event.ResourceStart)]
        public void Event_ResourceStart()
        {
            ColShape colShape = NAPI.ColShape.CreateCylinderColShape(DemorganPosition, 2500, 50, 1337);
            colShape.OnEntityExitColShape += (s, e) =>
            {
                if (!Main.Players.ContainsKey(e)) return;
                if (Main.Players[e].DemorganTime > 0) NAPI.Entity.SetEntityPosition(e, DemorganPosition + new Vector3(0, 0, 1.5));
            };
            Group.LoadCommandsConfigs();
        }

        [RemoteEvent("openAdminPanel")]
        public static void OpenAdminPanel(Player player)
        {
            CharacterData acc = Main.Players[player];
            List<Group.GroupCommand> cmds = new List<Group.GroupCommand>();
            List<object> players = new List<object>();
            if (acc.AdminLVL > 0)
            {
                foreach (Group.GroupCommand item in Group.GroupCommands)
                {
                    if (item.IsAdmin)
                    {
                        if (item.MinLVL <= acc.AdminLVL)
                        {
                            cmds.Add(item);
                        }
                    }
                }
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (Main.Players.ContainsKey(p) || p == null)
                    {
                        string[] data = { Main.Players[p].AdminLVL.ToString(), p.Value.ToString(), p.Name.ToString(), p.Ping.ToString() };
                        players.Add(data);
                    }
                }


                string json = Newtonsoft.Json.JsonConvert.SerializeObject(cmds);
                string json2 = Newtonsoft.Json.JsonConvert.SerializeObject(players);
                Trigger.PlayerEvent(player, "openAdminPanel", json, json2);
            }
            cmds.Clear();
            players.Clear();
        }
        public static Dictionary<int, string> AdminRanks = new Dictionary<int, string>()
        {
            {100, "[~p~Project Team~w~]"},
            {11, "[~p~Technical Administrator~w~]"},
            {10, "[~r~Cheif Administrator~w~]"},
            {9, "[~r~Dep Cheif Administrator~w~]"},
            {8, "[~o~Cheif Curator~w~]"},
            {7, "[~o~Curator~w~]"},
            {6, "[~p~Community-Manager~w~]"},
            {5, "[~o~Curator of the Fraction~w~]"},
            {4, "[~b~Senior Administrator~w~]"},
            {3, "[~b~Administrator~w~]"},
            {2, "[~g~Jr.Administrator~w~]"},
            {1, "[~q~Helper~w~]"},
        };

        public static void sendRedbucks(Player player, Player target, int amount)
        {
            if (!Group.CanUseCmd(player, "giveup")) return;

            if (Main.Accounts[target].RedBucks + amount < 0) amount = 0;
            Main.Accounts[target].RedBucks += amount;
            Trigger.PlayerEvent(target, "redset", Main.Accounts[target].RedBucks);

            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "update `accounts` set `redbucks`=@redbucks where `login`=@login";
            cmd.Parameters.AddWithValue("@redbucks", Main.Accounts[target].RedBucks);
            cmd.Parameters.AddWithValue("@login", Main.Accounts[target].Login);
            MySQL.Query(cmd);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы отправили {target.Name} {amount} MC", 3000);
            Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"+{amount} MC", 3000);

            GameLog.Admin(player.Name, $"givereds({amount})", target.Name);
        }


        public static void stopServer(string reason = "Сервер выключен.")
        {
            IsServerStoping = true;
            GameLog.Admin("server", $"stopServer({reason})", "");

            Log.Write("Force saving database...", nLog.Type.Warn);
            BCore.SaveBusinesses();
            Fractions.GangsCapture.SavingRegions();
            Houses.HouseManager.SavingHouses();
            Houses.FurnitureManager.Save();
            nInventory.SaveAll();
            Fractions.Stocks.saveStocksDic();
            // Weapons.SaveWeaponsDB(); 
            Log.Write("All data has been saved!", nLog.Type.Success);
            Log.Write("Force kicking players...", nLog.Type.Warn);
            foreach (Player player in NAPI.Pools.GetAllPlayers())
                NAPI.Task.Run(() => NAPI.Player.KickPlayer(player, reason));
            Log.Write("All players has kicked!", nLog.Type.Success);
            NAPI.Task.Run(() =>
            {
                Environment.Exit(0);
            }, 60000);
        }

        public static void saveCoords(Player player, string msg)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            Vector3 pos = NAPI.Entity.GetEntityPosition(player);
            pos.Z -= 1.12f;
            Vector3 rot = NAPI.Entity.GetEntityRotation(player);
            if (NAPI.Player.IsPlayerInAnyVehicle(player))
            {
                Vehicle vehicle = player.Vehicle;
                pos = NAPI.Entity.GetEntityPosition(vehicle) + new Vector3(0, 0, 0.5);
                rot = NAPI.Entity.GetEntityRotation(vehicle);
            }
            try
            {

                StreamWriter saveCoords = new StreamWriter("coords.txt", true, Encoding.UTF8);
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                saveCoords.Write($"{msg}   Координаты: new Vector3({pos.X}, {pos.Y}, {pos.Z}),    JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(pos)}      \r\n");
                saveCoords.Write($"{msg}   Вращение: new Vector3({rot.X}, {rot.Y}, {rot.Z}),     JSON: {Newtonsoft.Json.JsonConvert.SerializeObject(rot)}    \r\n");
                saveCoords.Close();
            }

            catch (Exception error)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Exeption: " + error);
            }

            finally
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Coords: " + NAPI.Entity.GetEntityPosition(player));
            }
        }
        public static void setPlayerAdminGroup(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "setadmin")) return;
            if (Main.Players[target].AdminLVL >= 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока уже есть админ. прав", 3000);
                return;
            }
            Main.Players[target].AdminLVL = 1;
            target.SetSharedData("IS_ADMIN", true);
            target.SetSharedData("ALVL", 1);
            Fractions.GangsCapture.LoadBlips(target);

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы Выдали админ. права игроку {target.Name}", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} Выдал Вам админ. права", 3000);
            GameLog.Admin($"{player.Name}", $"setAdmin", $"{target.Name}");
        }
        public static void delPlayerAdminGroup(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "deladmin")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете забрать админ. права у себя", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете забрать права у этого администратора", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет админ. прав", 3000);
                return;
            }
            Main.Players[target].AdminLVL = 0;
            target.ResetSharedData("IS_ADMIN");
            target.SetSharedData("ALVL", 0);
            Fractions.GangsCapture.UnLoadBlips(target);

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы забрали права у администратора {target.Name}", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} забрал у Вас админ. права", 3000);
            GameLog.Admin($"{player.Name}", $"delAdmin", $"{target.Name}");
        }
        public static void setPlayerAdminRank(Player player, Player target, int rank)
        {
            if (!Group.CanUseCmd(player, "setadminrank")) return;
            if (player == target)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете установить себе ранг", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игрок не является администратором!", 3000);
                return;
            }
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете изменить уровень прав у этого администратора", 3000);
                return;
            }
            if (rank < 1 || rank >= Main.Players[player].AdminLVL)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно выдать такой ранг", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали игроку {target.Name} {rank} уровень админ. прав", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} выдал Вам {rank} уровень админ. прав", 3000);
            Main.Players[target].AdminLVL = rank;
            //Main.AdminSlots[target.GetData("RealSocialClub")].AdminLVL = rank;
            GameLog.Admin($"{player.Name}", $"setAdminRank({rank})", $"{target.Name}");
        }
        public static void setPlayerVipLvl(Player player, Player target, int rank)
        {
            if (!Group.CanUseCmd(player, "setviplvl")) return;
            if (rank > 5 || rank < 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно выдать такой уровень ВИП аккаунта", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали игроку {target.Name} {Group.GroupNames[rank]}", 3000);
            Main.Accounts[target].VipLvl = rank;
            Main.Accounts[target].VipDate = DateTime.Now.AddDays(30);
            InvInterface.sendStats(target);
            GameLog.Admin($"{player.Name}", $"setVipLvl({rank})", $"{target.Name}");
        }

        public static void setFracLeader(Player sender, Player target, int fracid)
        {
            if (!Group.CanUseCmd(sender, "setleader")) return;
            if (fracid != 0 && fracid <= 18)
            {
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                int new_fraclvl = Fractions.Configs.FractionRanks[fracid].Count;
                Main.Players[target].FractionLVL = new_fraclvl;
                Main.Players[target].FractionID = fracid;
                Main.Players[target].WorkID = 0;
                if (fracid == 15)
                {
                    Trigger.PlayerEvent(target, "enableadvert", true);
                    Fractions.LSNews.onLSNPlayerLoad(target);
                }
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы стали лидером фракции {Fractions.Manager.getName(fracid)}", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы поставили {target.Name} на лидерство {Fractions.Manager.getName(fracid)}", 3000);
                Fractions.Manager.Load(target, fracid, new_fraclvl);
                InvInterface.sendStats(target);
                GameLog.Admin($"{sender.Name}", $"setFracLeader({fracid})", $"{target.Name}");
                return;
            }
        }
        public static void delFracLeader(Player sender, Player target)
        {
            if (!Group.CanUseCmd(sender, "delleader")) return;
            if (Main.Players[target].FractionID != 0 && Main.Players[target].FractionID <= 18)
            {
                if (Main.Players[target].FractionLVL < Fractions.Configs.FractionRanks[Main.Players[target].FractionID].Count)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок не является лидером", 3000);
                    return;
                }
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                if (Main.Players[target].FractionID == 15) Trigger.PlayerEvent(target, "enableadvert", false);

                Main.Players[target].OnDuty = false;
                Main.Players[target].FractionID = 0;
                Main.Players[target].FractionLVL = 0;

                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{sender.Name.Replace('_', ' ')} снял Вас с поста лидера фракции", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы сняли {target.Name.Replace('_', ' ')} с поста лидера фракции", 3000);
                InvInterface.sendStats(target);

                Customization.ApplyCharacter(target);
                NAPI.Player.RemoveAllPlayerWeapons(target);
                GameLog.Admin($"{sender.Name}", $"delFracLeader", $"{target.Name}");
            }
            else Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет фракции", 3000);
        }
        public static void delJob(Player sender, Player target)
        {
            if (!Group.CanUseCmd(sender, "deljob")) return;
            if (Main.Players[target].WorkID != 0)
            {
                if (NAPI.Data.GetEntityData(target, "ON_WORK") == true)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок должен быть не в рабочей форме", 3000);
                    return;
                }
                Main.Players[target].WorkID = 0;
                InvInterface.sendStats(target);
                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{sender.Name.Replace('_', ' ')} снял трудоустройство с Вашего персонажа", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы сняли {target.Name.Replace('_', ' ')} с трудоустройства", 3000);
                InvInterface.sendStats(target);
                GameLog.Admin($"{sender.Name}", $"delJob", $"{target.Name}");
            }
            else Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет работы", 3000);
        }
        public static void delFrac(Player sender, Player target)
        {
            if (!Group.CanUseCmd(sender, "delfrac")) return;
            if (Main.Players[target].FractionID != 0 && Main.Players[target].FractionID <= 18)
            {
                if (Main.Players[target].FractionLVL >= Fractions.Configs.FractionRanks[Main.Players[target].FractionID].Count)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок - лидер фракции", 3000);
                    return;
                }
                Fractions.Manager.UNLoad(target);
                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
                if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

                if (Main.Players[target].FractionID == 15) Trigger.PlayerEvent(target, "enableadvert", false);

                Main.Players[target].OnDuty = false;
                Main.Players[target].FractionID = 0;
                Main.Players[target].FractionLVL = 0;

                Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"Администратор {sender.Name.Replace('_', ' ')} выгнал Вас из фракции", 3000);
                Notify.Send(sender, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выгнали {target.Name.Replace('_', ' ')} из фракции", 3000);

                Customization.ApplyCharacter(target);
                NAPI.Player.RemoveAllPlayerWeapons(target);
                GameLog.Admin($"{sender.Name}", $"delFrac", $"{target.Name}");
            }
            else Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока нет фракции", 3000);
        }

        public static void giveMoney(Player player, Player target, int amount)
        {
            if (!Group.CanUseCmd(player, "givemoney")) return;
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[target].UUID})", amount, "admin");
            MoneySystem.Wallet.Change(target, amount);
            GameLog.Admin($"{player.Name}", $"giveMoney({amount})", $"{target.Name}");
        }
        public static void OffMutePlayer(Player player, string target, int time, string reason)
        {
            try
            {
                if (!Group.CanUseCmd(player, "mute")) return;
                if (NAPI.Player.GetPlayerFromName(target) != null)
                {
                    mutePlayer(player, NAPI.Player.GetPlayerFromName(target), time, reason);
                    Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Игрок был онлайн, поэтому offmute заменён на mute", 3000);
                    return;
                }
                if (player.Name.Equals(target)) return;
                if (time > 480)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете дать мут больше, чем на 480 минут", 3000);
                    return;
                }
                var split = target.Split('_');
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = "UPDATE `characters` SET `unmute`=@unmute WHERE firstname=@firstname AND lastname=@lastname";
                cmd.Parameters.AddWithValue("@firstname", split[0]);
                cmd.Parameters.AddWithValue("@lastname", split[1]);
                MySQL.Query(cmd);
                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} выдал мут игроку {target} на {time}м ({reason})");
                GameLog.Admin($"{player.Name}", $"mutePlayer({time}, {reason})", $"{target}");
            }
            catch { }

        }
        public static void mutePlayer(Player player, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "mute")) return;
            if (player == target) return;
            if (time > 480)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете дать мут больше, чем на 480 минут", 3000);
                return;
            }
            Main.Players[target].Unmute = time * 60;
            Main.Players[target].VoiceMuted = true;
            if (target.HasData("MUTE_TIMER")) Timers.Stop(target.GetData<string>("MUTE_TIMER"));
            NAPI.Data.SetEntityData(target, "MUTE_TIMER", Timers.StartTask(1000, () => timer_mute(target)));
            target.SetSharedData("voice.muted", true);
            Trigger.PlayerEvent(target, "voice.mute");
            NAPI.Chat.SendChatMessageToAll($"~o~{player.Name} выдал мут игроку {target.Name} на {time}м ({reason})");
            GameLog.Admin($"{player.Name}", $"mutePlayer({time}, {reason})", $"{target.Name}");
        }
        public static void unmutePlayer(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "unmute")) return;

            Main.Players[target].Unmute = 2;
            Main.Players[target].VoiceMuted = false;
            target.SetSharedData("voice.muted", false);

            NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} снял мут с игрока {target.Name}");
            GameLog.Admin($"{player.Name}", $"unmutePlayer", $"{target.Name}");
        }
        public static void banPlayer(Player player, Player target, int time, string reason, bool isSilence)
        {
            string cmd = (isSilence) ? "sban" : "ban";
            if (!Group.CanUseCmd(player, cmd)) return;
            if (player == target) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.SendToAdmins(3, $"!{{#d35400}}[BAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            if (!isSilence)
                NAPI.Chat.SendChatMessageToAll($"!{{#f0ba12}}{player.Name} забанил игрока {target.Name} на {time}{banTimeMsg} ({reason})");

            Ban.Online(target, unbanTime, false, reason, player.Name);

            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Вы заблокированы до {unbanTime.ToString()}", 30000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Причина: {reason}", 30000);

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.Players[target].UUID;

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, false);

            target.Kick(reason);
        }
        public static void hardbanPlayer(Player player, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "ban")) return;
            if (player == target) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.SendToAdmins(3, $"!{{#d35400}}[HARDBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }
            NAPI.Chat.SendChatMessageToAll($"~o~{player.Name} ударил банхаммером игрока {target.Name} на {time}{banTimeMsg} ({reason})");
            if (reason.Contains("cheat"))
            {
                Trigger.PlayerEvent(player, "setDataBanned");
            }

            Ban.Online(target, unbanTime, true, reason, player.Name);

            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Ты словил банхаммер до {unbanTime.ToString()}", 30000);
            Notify.Send(target, NotifyType.Warning, NotifyPosition.Center, $"Причина: {reason}", 30000);

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.Players[target].UUID;

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, true);

            target.Kick(reason);
        }
        public static void offBanPlayer(Player player, string name, int time, string reason, bool isSilence)
        {
            if (!Group.CanUseCmd(player, "offban")) return;
            if (player.Name == name) return;
            string cmd = (isSilence) ? "sban" : "ban";
            Player target = NAPI.Player.GetPlayerFromName(name);
            if (target != null)
            {
                if (Main.Players.ContainsKey(target))
                {
                    if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
                    {
                        Commands.SendToAdmins(3, $"!{{#d35400}}[OFFBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                        return;
                    }
                    else
                    {
                        target.Kick();
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Center, "Игрок находился в Online, но был кикнут.", 3000);
                    }
                }
            }
            else
            {
                string[] split = name.Split('_');
                DataTable result = MySQL.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                DataRow row = result.Rows[0];
                int targetadminlvl = Convert.ToInt32(row[0]);
                if (targetadminlvl >= Main.Players[player].AdminLVL)
                {
                    Commands.SendToAdmins(3, $"!{{#d35400}}[OFFBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {name} (offline), который имеет выше уровень администратора.");
                    return;
                }
            }

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.PlayerUUIDs[name];

            Ban ban = Ban.Get2(TUUID);
            if (ban != null)
            {
                string hard = (ban.isHard) ? "хард " : "";
                Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Игрок уже в {hard}бане", 3000);
                return;
            }

            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м"; // Можно использовать char
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            Ban.Offline(name, unbanTime, false, reason, player.Name);

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, false);

            if (!isSilence)
                NAPI.Chat.SendChatMessageToAll($"~o~{player.Name} забанил игрока {name} на {time}{banTimeMsg} ({reason})");
        }
        public static void offHardBanPlayer(Player player, string name, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "offban")) return;
            if (player.Name.Equals(name)) return;
            Player target = NAPI.Player.GetPlayerFromName(name);
            if (target != null)
            {
                if (Main.Players.ContainsKey(target))
                {
                    if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
                    {
                        Commands.SendToAdmins(3, $"!{{#d35400}}[OFFHARDBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                        return;
                    }
                    else
                    {
                        target.Kick();
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Center, "Игрок находился в Online, но был кикнут.", 3000);
                    }
                }
            }
            else
            {
                string[] split = name.Split('_');
                DataTable result = MySQL.QueryRead($"SELECT adminlvl FROM characters WHERE firstname='{split[0]}' AND lastname='{split[1]}'");
                DataRow row = result.Rows[0];
                int targetadminlvl = Convert.ToInt32(row[0]);
                if (targetadminlvl >= Main.Players[player].AdminLVL)
                {
                    Commands.SendToAdmins(3, $"!{{#d35400}}[OFFHARDBAN-DENIED] {player.Name} ({player.Value}) попытался забанить {name} (offline), который имеет выше уровень администратора.");
                    return;
                }
            }

            int AUUID = Main.Players[player].UUID;
            int TUUID = Main.PlayerUUIDs[name];

            Ban ban = Ban.Get2(TUUID);
            if (ban != null)
            {
                string hard = (ban.isHard) ? "хард " : "";
                Notify.Send(player, NotifyType.Warning, NotifyPosition.Center, $"Игрок уже в {hard}бане", 3000);
                return;
            }

            DateTime unbanTime = DateTime.Now.AddMinutes(time);
            string banTimeMsg = "м";
            if (time > 60)
            {
                banTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    banTimeMsg = "д";
                    time /= 24;
                }
            }

            Ban.Offline(name, unbanTime, true, reason, player.Name);

            GameLog.Ban(AUUID, TUUID, unbanTime, reason, true);

            NAPI.Chat.SendChatMessageToAll($"~o~{player.Name} ударил банхаммером игрока {name} на {time}{banTimeMsg} ({reason})");
        }
        public static void unbanPlayer(Player player, string name)
        {
            if (!Main.PlayerNames.ContainsValue(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Такого имени нет!", 3000);
                return;
            }
            if (!Ban.Pardon(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{name} не находится в бане!", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Игрок разблокирован!", 3000);
            GameLog.Admin($"{player.Name}", $"unban", $"{name}");
        }
        public static void unhardbanPlayer(Player player, string name)
        {
            if (!Main.PlayerNames.ContainsValue(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Такого имени нет!", 3000);
                return;
            }
            if (!Ban.PardonHard(name))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"{name} не находится в бане!", 3000);
                return;
            }
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "С игрока снят хардбан!", 3000);
        }
        public static void kickPlayer(Player player, Player target, string reason, bool isSilence)
        {
            string cmd = (isSilence) ? "skick" : "kick";
            if (!Group.CanUseCmd(player, cmd)) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.SendToAdmins(3, $"!{{#d35400}}[KICK-DENIED] {player.Name} ({player.Value}) попытался кикнуть {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            if (!isSilence)
                NAPI.Chat.SendChatMessageToAll($"~r~{player.Name} кикнул игрока {target.Name} ({reason})");
            else
            {
                foreach (Player p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].AdminLVL >= 1)
                    {
                        p.SendChatMessage($"~o~{player.Name} тихо кикнул игрока {target.Name}");
                    }
                }
            }
            GameLog.Admin($"{player.Name}", $"kickPlayer({reason})", $"{target.Name}");
            NAPI.Player.KickPlayer(target, reason);
        }
        public static void warnPlayer(Player player, Player target, string reason)
        {
            if (!Group.CanUseCmd(player, "warn")) return;
            if (player == target) return;
            if (Main.Players[target].AdminLVL >= Main.Players[player].AdminLVL)
            {
                Commands.SendToAdmins(3, $"!{{#d35400}}[WARN-DENIED] {player.Name} ({player.Value}) попытался предупредить {target.Name} ({target.Value}), который имеет выше уровень администратора.");
                return;
            }
            Main.Players[target].Warns++;
            Main.Players[target].Unwarn = DateTime.Now.AddDays(14);

            int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == target.Name);
            if (index > -1) Fractions.Manager.AllMembers.RemoveAt(index);

            Main.Players[target].OnDuty = false;
            Main.Players[target].FractionID = 0;
            Main.Players[target].FractionLVL = 0;

            NAPI.Chat.SendChatMessageToAll($"!{{#e03d3d}}{player.Name} выдал предупреждение игроку {target.Name} ({reason}) [{Main.Players[target].Warns}/3]");

            if (Main.Players[target].Warns >= 3)
            {
                DateTime unbanTime = DateTime.Now.AddMinutes(43200);
                Main.Players[target].Warns = 0;
                Ban.Online(target, unbanTime, false, "Warns 3/3", "Server_Serverniy");
            }

            GameLog.Admin($"{player.Name}", $"warnPlayer({reason})", $"{target.Name}");
            target.Kick("Предупреждение");
        }
        public static void kickPlayerByName(Player player, string name)
        {
            if (!Group.CanUseCmd(player, "nkick")) return;
            Player target = NAPI.Player.GetPlayerFromName(name);
            if (target == null) return;
            NAPI.Player.KickPlayer(target);
            GameLog.Admin($"{player.Name}", $"kickPlayer", $"{name}");
        }

        public static void killTarget(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "kill")) return;
            NAPI.Player.SetPlayerHealth(target, 0);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы убили игрока {target.Name}", 3000);
            GameLog.Admin($"{player.Name}", $"killPlayer", $"{target.Name}");
        }
        public static void healTarget(Player player, Player target, int hp)
        {
            if (!Group.CanUseCmd(player, "hp")) return;
            NAPI.Player.SetPlayerHealth(target, hp);
            player.Health = hp;
            GameLog.Admin($"{player.Name}", $"healPlayer({hp})", $"{target.Name}");
        }
        public static void armorTarget(Player player, Player target, int ar)
        {
            if (!Group.CanUseCmd(player, "ar")) return;

            nItem aItem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
            if (aItem == null)
                nInventory.Add(player, new nItem(ItemType.BodyArmor, 1, ar.ToString()));
            GameLog.Admin($"{player.Name}", $"armorPlayer({ar})", $"{target.Name}");
        }
        public static void checkGamemode(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "gm")) return;
            int targetHealth = target.Health;
            int targetArmor = target.Armor;
            NAPI.Entity.SetEntityPosition(target, target.Position + new Vector3(0, 0, 10));
            NAPI.Task.Run(() => { try { Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"{target.Name} было {targetHealth} HP {targetArmor} Броня | Стало {target.Health} HP {target.Armor} Броня.", 3000); } catch { } }, 3000);
            GameLog.Admin($"{player.Name}", $"checkGm", $"{target.Name}");
        }
        public static void checkMoney(Player player, Player target)
        {
            try
            {
                if (!Group.CanUseCmd(player, "checkmoney")) return;
                MoneySystem.Bank.Data bankAcc = MoneySystem.Bank.Accounts.FirstOrDefault(a => a.Value.Holder == target.Name).Value;
                int bankMoney = 0;
                if (bankAcc != null) bankMoney = (int)bankAcc.Balance;
                Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"У {target.Name} {Main.Players[target].Money}$ | Bank: {bankMoney}", 3000);
                GameLog.Admin($"{player.Name}", $"checkMoney", $"{target.Name}");
            }
            catch (Exception e) { Log.Write("CheckMoney: " + e.Message, nLog.Type.Error); }
        }

        public static void teleportTargetToPlayer(Player player, Player target, bool withveh = false)
        {
            if (!Group.CanUseCmd(player, "metp")) return;
            if (!withveh)
            {
                GameLog.Admin($"{player.Name}", $"metp", $"{target.Name}");
                NAPI.Entity.SetEntityPosition(target, player.Position);
                NAPI.Entity.SetEntityDimension(target, player.Dimension);
            }
            else
            {
                if (!target.IsInVehicle) return;
                NAPI.Entity.SetEntityPosition(target.Vehicle, player.Position + new Vector3(2, 2, 2));
                NAPI.Entity.SetEntityDimension(target.Vehicle, player.Dimension);
                GameLog.Admin($"{player.Name}", $"gethere", $"{target.Name}");
            }
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы телепортировали {target.Name} к себе", 3000);
            Notify.Send(target, NotifyType.Info, NotifyPosition.BottomCenter, $"{player.Name} телепортировал Вас к себе", 3000);
        }

        public static void freezeTarget(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "fz")) return;
            Trigger.PlayerEvent(target, "freeze", true);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы заморозили игрока {target.Name}", 3000);
            GameLog.Admin($"{player.Name}", $"freeze", $"{target.Name}");
        }
        public static void unFreezeTarget(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "ufz")) return;
            Trigger.PlayerEvent(target, "freeze", false);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы разморозили игрока {target.Name}", 3000);
            GameLog.Admin($"{player.Name}", $"unfreeze", $"{target.Name}");
        }

        public static void giveTargetGun(Player player, Player target, string weapon, string serial)
        {
            if (!Group.CanUseCmd(player, "guns")) return;
            if (serial.Length != 9)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Серийный номер состоит из 9 символов", 3000);
                return;
            }
            ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), weapon);
            if (wType == ItemType.Mask || wType == ItemType.Gloves || wType == ItemType.Bag || wType == ItemType.Leg || wType == ItemType.Bag || wType == ItemType.Feet ||
                wType == ItemType.Jewelry || wType == ItemType.Undershit || wType == ItemType.BodyArmor || wType == ItemType.Unknown || wType == ItemType.Top ||
                wType == ItemType.Hat || wType == ItemType.Glasses || wType == ItemType.Accessories)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Предметы одежды выдавать запрещено", 3000);
                return;
            }
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У игрока недостаточно места в инвентаре", 3000);
                return;
            }
            Weapons.GiveWeapon(target, wType, serial);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали игроку {target.Name} оружие ({weapon.ToString()})", 3000);
            GameLog.Admin($"{player.Name}", $"giveGun({weapon},{serial})", $"{target.Name}");
        }
        public static void giveTargetSkin(Player player, Player target, string pedModel)
        {
            if (!Group.CanUseCmd(player, "setskin")) return;
            if (pedModel.Equals("-1"))
            {
                if (target.HasData("AdminSkin"))
                {
                    target.ResetData("AdminSkin");
                    target.SetSkin((Main.Players[target].Gender) ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);
                    Customization.ApplyCharacter(target);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Вы восстановили игроку внешность", 3000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Игроку не меняли внешность", 3000);
                    return;
                }
            }
            else
            {
                PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                if (pedHash != 0)
                {
                    target.SetData("AdminSkin", true);
                    target.SetSkin(pedHash);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы сменили игроку {target.Name} внешность на ({pedModel})", 3000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Внешности с таким названием не было найдено", 3000);
                    return;
                }
            }
        }
        public static void giveTargetClothes(Player player, Player target, string weapon, string serial)
        {
            if (!Group.CanUseCmd(player, "giveclothes")) return;
            if (serial.Length < 6 || serial.Length > 12)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Серийный номер состоит из 6-12 символов", 3000);
                return;
            }
            ItemType wType = (ItemType)Enum.Parse(typeof(ItemType), weapon);
            if (wType != ItemType.Mask && wType != ItemType.Gloves && wType != ItemType.Bag && wType != ItemType.Leg && wType != ItemType.Bag && wType != ItemType.Feet &&
                wType != ItemType.Jewelry && wType != ItemType.Undershit && wType != ItemType.BodyArmor && wType != ItemType.Unknown && wType != ItemType.Top &&
                wType != ItemType.Hat && wType != ItemType.Glasses && wType != ItemType.Accessories)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Этой командой можно выдавать только предметы одежды", 3000);
                return;
            }
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У игрока недостаточно места в инвентаре", 3000);
                return;
            }
            Weapons.GiveWeapon(target, wType, serial);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выдали игроку {target.Name} одежду ({weapon.ToString()})", 3000);
        }
        public static void takeTargetGun(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "oguns")) return;
            Weapons.RemoveAll(target, true);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы забрали у игрока {target.Name} всё оружие", 3000);
            GameLog.Admin($"{player.Name}", $"takeGuns", $"{target.Name}");
        }

        public static void adminSMS(Player player, Player target, string message)
        {
            if (!Group.CanUseCmd(player, "asms")) return;
            foreach (Player p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 1)
                {
                    p.SendChatMessage($"~g~Администратор ~w~{player.Name} для {target.Name}:~b~ {message}");
                }
            }
            target.SendChatMessage($"~g~Сообщение от администратора {player.Name}:~w~ {message}");
            //player.SendChatMessage($"~y~{player.Name} ({player.Value}): {message}");
        }
        public static void answerReport(Player player, Player target, string message)
        {
            if (!Group.CanUseCmd(player, "ans")) return;
            if (!target.HasData("IS_REPORT")) return;

            player.SendChatMessage($"~r~Вы ответили для {target.Name}: {message}");
            target.SendChatMessage($"~r~Ответ от администратора {player.Name}:~w~ {message}");
            target.ResetData("IS_REPORT");
            foreach (Player p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 1)
                {
                    p.SendChatMessage($"~g~Администратор ~w~{player.Name} ответил для {target.Name}:~b~ {message}");
                }
            }
            GameLog.Admin($"{player.Name}", $"answer({message})", $"{target.Name}");
        }
        public static string GetNameRank(int lvl)
        {
            if (!AdminRanks.ContainsKey(lvl))
            {
                return "";
            }
            else
            {
                return AdminRanks[lvl];
            }
        }
        public static void adminChat(Player player, string message)
        {
            if (!Group.CanUseCmd(player, "a")) return;
            int lvl;
            foreach (Player p in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].AdminLVL >= 1)
                {
                    lvl = Main.Players[player].AdminLVL;
                    p.SendChatMessage($"~r~[ADMIN]~w~{GetNameRank(lvl)} ~w~{player.Name.Replace("_"," ")}({player.Value}):~w~ {message}");
                }
            }
        }
        public static void adminGlobal(Player player, string message)
        {
            if (!Group.CanUseCmd(player, "global")) return;
            NAPI.Chat.SendChatMessageToAll("!{#DF5353}" + $"[GLOBAL] {player.Name.Replace('_', ' ')}: {message}");
            GameLog.Admin($"{player.Name}", $"global({message})", $"");
        }
        public static List<uint> DemorganPets = new List<uint>()
        {
            0xCE5FF074,
            0x573201B8,
            0x14EC17EA,
            0xFCFA9E1E,
            0x644AC75E,
            0xD86B5A95,
            0x6AF51FAF,
            0x4E8F95A2,
            0x1250D7BA,
            0xB11BAB56,
            0x431D501C,
            0x6D362854,
            0xDFB55C81,
            0xC3B52966,
            0x349F33E1,
            0x9563221D,
            0x431FC24C,
            0xAD7844BB,

        };
        public static void sendPlayerToDemorgan(Player admin, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(admin, "demorgan")) return;
            if (!Main.Players.ContainsKey(target)) return;
            if (admin == target) return;
            int firstTime = time * 60;
            string deTimeMsg = "м";
            if (time > 60)
            {
                deTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    deTimeMsg = "д";
                    time /= 24;
                }
            }
            int rand = new Random().Next(0, DemorganPets.Count);
            NAPI.Player.SetPlayerSkin(target, DemorganPets[rand]);
            NAPI.Chat.SendChatMessageToAll($"!{{#f0ba12}}{admin.Name} посадил игрока {target.Name} в спец. тюрьму на {time}{deTimeMsg} ({reason})");
            Main.Players[target].ArrestTime = 0;
            Main.Players[target].DemorganTime = firstTime;
            Fractions.FractionCommands.unCuffPlayer(target);

            NAPI.Entity.SetEntityPosition(target, DemorganPosition + new Vector3(0, 0, 1.5));
            //if (target.HasData("ARREST_TIMER")) Main.StopT(target.GetData("ARREST_TIMER"), "timer_34");
            if (target.HasData("ARREST_TIMER")) Timers.Stop(target.GetData<string>("ARREST_TIMER"));
            //NAPI.Data.SetEntityData(target, "ARREST_TIMER", Main.StartT(1000, 1000, (o) => timer_demorgan(target), "DEMORGAN_TIMER"));
            NAPI.Data.SetEntityData(target, "ARREST_TIMER", Timers.StartTask(1000, () => timer_demorgan(target)));
            Timers.StartTask(60000, () => timer_text(target));
            NAPI.Entity.SetEntityDimension(target, 1337);
            Weapons.RemoveAll(target, true);
            GameLog.Admin($"{admin.Name}", $"demorgan({time}{deTimeMsg},{reason})", $"{target.Name}");
        }
        public static void sendPlayerToDemorganBOT(string admin, Player target, int time, string reason)
        {
            if (!Main.Players.ContainsKey(target)) return;
            int firstTime = time * 60;
            string deTimeMsg = "м";
            if (time > 60)
            {
                deTimeMsg = "ч";
                time /= 60;
                if (time > 24)
                {
                    deTimeMsg = "д";
                    time /= 24;
                }
            }
            int rand = new Random().Next(0, DemorganPets.Count);
            NAPI.Player.SetPlayerSkin(target, DemorganPets[rand]);
            NAPI.Chat.SendChatMessageToAll($"!{{#f0ba12}}{admin} посадил игрока {target.Name} в спец. тюрьму на {time}{deTimeMsg} ({reason})");
            Main.Players[target].ArrestTime = 0;
            Main.Players[target].DemorganTime = firstTime;
            Fractions.FractionCommands.unCuffPlayer(target);

            NAPI.Entity.SetEntityPosition(target, DemorganPosition + new Vector3(0, 0, 1.5));
            //if (target.HasData("ARREST_TIMER")) Main.StopT(target.GetData("ARREST_TIMER"), "timer_34");
            if (target.HasData("ARREST_TIMER")) Timers.Stop(target.GetData<string>("ARREST_TIMER"));
            //NAPI.Data.SetEntityData(target, "ARREST_TIMER", Main.StartT(1000, 1000, (o) => timer_demorgan(target), "DEMORGAN_TIMER"));
            NAPI.Data.SetEntityData(target, "ARREST_TIMER", Timers.StartTask(1000, () => timer_demorgan(target)));
            Timers.StartTask(60000, () => timer_text(target));
            NAPI.Entity.SetEntityDimension(target, 1337);
            Weapons.RemoveAll(target, true);
            GameLog.Admin($"{admin}", $"demorgan({time}{deTimeMsg},{reason})", $"{target.Name}");
        }
        public static void releasePlayerFromDemorgan(Player admin, Player target)
        {
            if (!Group.CanUseCmd(admin, "udemorgan")) return;
            if (!Main.Players.ContainsKey(target)) return;

            Main.Players[target].DemorganTime = 0;
            Notify.Send(admin, NotifyType.Warning, NotifyPosition.BottomCenter, $"Вы освободили {target.Name} из админ. тюрьмы", 3000);
            GameLog.Admin($"{admin.Name}", $"undemorgan", $"{target.Name}");
        }

        #region Demorgan
        public static Vector3 DemorganPosition = new Vector3(5060.1157, -5341.618, 9.588966);
        public static void timer_demorgan(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].DemorganTime <= 0)
                {
                    Fractions.FractionCommands.freePlayer(player);
                    return;
                }
                Main.Players[player].DemorganTime--;
            }
            catch (Exception e)
            {
                Log.Write("DEMORGAN_TIMER: " + e.ToString(), nLog.Type.Error);
            }
        }
        public static void timer_text(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].DemorganTime <= 0)
                {
                    return;
                }
                var time = Convert.ToInt32(Main.Players[player].DemorganTime / 60.0);
                string deTimeMsg = "минут";
                if (time > 60)
                {
                    deTimeMsg = "часов";
                    time /= 60;
                    if (time > 24)
                    {
                        deTimeMsg = "дней";
                        time /= 24;
                    }
                }
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вам осталось сидеть {time} {deTimeMsg}", 3000);
            }
            catch (Exception e)
            {
                Log.Write("DEMORGAN_TEXT: " + e.ToString(), nLog.Type.Error);
            }
        }

        public static void timer_mute(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].Unmute <= 0 || Main.Players[player].VUnmute <= 0)
                {
                    if (player.HasData("MUTE_TIMER") && Main.Players[player].Unmute <= 0)
                    {
                        Timers.Stop(NAPI.Data.GetEntityData(player, "MUTE_TIMER"));
                        NAPI.Data.ResetEntityData(player, "MUTE_TIMER");
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Mute был снят, не нарушайте больше!", 3000);
                        return;
                    }
                    if (player.HasData("VMUTE_TIMER") && Main.Players[player].VUnmute <= 0)
                    {
                        if (player.HasData("MUTE_TIMER"))
                            Timers.Stop(NAPI.Data.GetEntityData(player, "MUTE_TIMER"));
                        if (player.HasData("VMUTE_TIMER"))
                            Timers.Stop(NAPI.Data.GetEntityData(player, "VMUTE_TIMER"));
                        NAPI.Data.ResetEntityData(player, "VMUTE_TIMER");
                        Main.Players[player].VoiceMuted = false;
                        player.SetSharedData("voice.muted", false);
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, "Mute был снят, не нарушайте больше!", 3000);
                        return;
                    }
                }
                if (Main.Players[player].Unmute > 0)
                {
                    Main.Players[player].Unmute--;
                }
                else if (Main.Players[player].VUnmute > 0)
                {
                    Main.Players[player].VUnmute--;
                }
            }
            catch (Exception e)
            {
                Log.Write("MUTE_TIMER: " + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        public static void muteVPlayer(Player player, Player target, int time, string reason)
        {
            if (!Group.CanUseCmd(player, "mute")) return;
            if (player == target) return;
            if (time > 480)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете дать войс мут больше, чем на 480 минут", 3000);
                return;
            }
            Main.Players[target].VUnmute = time * 60;
            Main.Players[target].VoiceMuted = true;
            if (target.HasData("VMUTE_TIMER")) Timers.Stop(target.GetData<string>("VMUTE_TIMER"));
            NAPI.Data.SetEntityData(target, "VMUTE_TIMER", Timers.StartTask(1000, () => timer_mute(target)));
            target.SetSharedData("voice.muted", true);
            Trigger.PlayerEvent(target, "voice.mute");
            NAPI.Chat.SendChatMessageToAll($"~o~{player.Name} выдал мут игроку {target.Name} на {time}м ({reason})");
            GameLog.Admin($"{player.Name}", $"vmutePlayer({time}, {reason})", $"{target.Name}");
        }

        public static void unmuteVPlayer(Player player, Player target)
        {
            if (!Group.CanUseCmd(player, "unmute")) return;

            Main.Players[target].VUnmute = 2;
            Main.Players[target].VoiceMuted = false;
            target.SetSharedData("voice.muted", false);

            NAPI.Chat.SendChatMessageToAll($"~o~{player.Name} снял мут с игрока {target.Name}");
            GameLog.Admin($"{player.Name}", $"unmutePlayer", $"{target.Name}");
        }

        // need refactor
        public static void respawnAllCars(Player player)
        {
            if (!Group.CanUseCmd(player, "allspawncar")) return;
            List<Vehicle> all_vehicles = NAPI.Pools.GetAllVehicles();

            foreach (Vehicle vehicle in all_vehicles)
            {
                List<Player> occupants = VehicleManager.GetVehicleOccupants(vehicle);
                if (occupants.Count > 0)
                {
                    List<Player> newOccupants = new List<Player>();
                    foreach (Player occupant in occupants)
                        if (Main.Players.ContainsKey(occupant)) newOccupants.Add(occupant);
                    vehicle.SetData("OCCUPANTS", newOccupants);
                }
            }

            foreach (Vehicle vehicle in all_vehicles)
            {
                if (VehicleManager.GetVehicleOccupants(vehicle).Count >= 1) continue;
                if (vehicle.GetData<string>("ACCESS") == "PERSONAL")
                {
                    Player owner = vehicle.GetData<Player>("OWNER");
                    NAPI.Entity.DeleteEntity(vehicle);
                }
                else if (vehicle.GetData<string>("ACCESS") == "WORK")
                    RespawnWorkCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "FRACTION")
                    RespawnFractionCar(vehicle);
                else if (vehicle.GetData<string>("ACCESS") == "GANGDELIVERY" || vehicle.GetData<string>("ACCESS") == "MAFIADELIVERY")
                    NAPI.Entity.DeleteEntity(vehicle);
            }
        }

        public static void RespawnWorkCar(Vehicle vehicle)
        {
            if (vehicle.GetData<bool>("ON_WORK") && Main.Players.ContainsKey(vehicle.GetData<Player>("DRIVER"))) return;
            var type = vehicle.GetData<string>("TYPE");

        }

        public static void RespawnFractionCar(Vehicle vehicle)
        {
            if (vehicle.HasData("loaderMats"))
            {
                Player loader = NAPI.Data.GetEntityData(vehicle, "loaderMats");
                Trigger.PlayerEvent(loader, "hideLoader");
                Notify.Send(loader, NotifyType.Warning, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
                if (loader.HasData("loadMatsTimer"))
                {
                    //Main.StopT(loader.GetData("loadMatsTimer"), "timer_35");
                    Timers.Stop(loader.GetData<string>("loadMatsTimer"));
                    loader.ResetData("loadMatsTime");
                }
                NAPI.Data.ResetEntityData(vehicle, "loaderMats");
            }
            Fractions.Configs.RespawnFractionCar(vehicle);
        }
    }

    public class Group
    {
        public static List<GroupCommand> GroupCommands = new List<GroupCommand>();
        public static void LoadCommandsConfigs()
        {
            DataTable result = MySQL.QueryRead($"SELECT * FROM adminaccess");
            if (result == null || result.Rows.Count == 0) return;
            List<GroupCommand> groupCmds = new List<GroupCommand>();
            foreach (DataRow Row in result.Rows)
            {
                string cmd = Convert.ToString(Row["command"]);
                bool isadmin = Convert.ToBoolean(Row["isadmin"]);
                int minrank = Convert.ToInt32(Row["minrank"]);

                groupCmds.Add(new GroupCommand(cmd, isadmin, minrank));
            }
            GroupCommands = groupCmds;
        }

        public static List<string> GroupNames = new List<string>()
        {
            "Нет",
            "Bronze",
            "Silver",
            "Gold",
            "Platinum",
            "Бизнесмен",
            "Big Daddy"
        };
        public static List<float> GroupPayAdd = new List<float>()
        {
            1.0f,
            1.15f,
            1.25f,
            1.35f,
            1.50f,
            1.0f,
            1.50f,
        };
        public static List<int> GroupAddPayment = new List<int>()
        {
            0,
            2000,
            4000,
            5500,
            10000,
            0,
            10000,
        };
        public static List<int> GroupMaxContacts = new List<int>()
        {
            50,
            60,
            70,
            80,
            100,
            50,
            100,
        };
        public static List<int> GroupMaxBusinesses = new List<int>()
        {
            1,
            1,
            1,
            1,
            1,
            2,
            1,
        };
        public static List<int> GroupEXP = new List<int>()
        {
            1,
            2,
            3,
            3,
            4,
            2,
            4,
        };

        public static bool CanUseCmd(Player player, string cmd, string args = "")
        {
            if (!Main.Players.ContainsKey(player)) return false;
            GroupCommand command = GroupCommands.FirstOrDefault(c => c.Command == cmd);
        check:
            if (command != null)
            {
                if (command.IsAdmin)
                {
                    if (Main.Players[player].AdminLVL >= command.MinLVL) return true;
                }
                else
                {
                    if (Main.Accounts[player].VipLvl >= command.MinLVL) return true;
                }
            }
            else
            {
                MySQL.Query($"INSERT INTO `adminaccess`(`command`, `isadmin`, `minrank`) VALUES ('{cmd}',1,7)");
                GroupCommand newGcmd = new GroupCommand(cmd, true, 7);
                command = newGcmd;
                GroupCommands.Add(newGcmd);
                goto check;
            }

            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно прав", 3000);
            return false;
        }

        public class GroupCommand
        {
            public GroupCommand(string command, bool isAdmin, int minlvl)
            {
                Command = command;
                IsAdmin = isAdmin;
                MinLVL = minlvl;
            }

            public string Command { get; }
            public bool IsAdmin { get; }
            public int MinLVL { get; }
        }
    }
}

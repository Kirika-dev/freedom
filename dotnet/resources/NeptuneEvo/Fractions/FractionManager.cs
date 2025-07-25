﻿using System.Collections.Generic;
using System.Data;
using System;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using NeptuneEVO.Core.Character;
using System.Linq;
using Newtonsoft.Json;
using MySqlConnector;

namespace NeptuneEVO.Fractions
{
    class Manager : Script
    { // Revision 3.0
        private static nLog Log = new nLog("Fractions");
        public class EMSExitCheckpoint
        {
            public static Vector3 pos { get; set; }
            public static float range { get; set; }

            public EMSExitCheckpoint(Vector3 pos, float range)
            {
                var shape = NAPI.ColShape.CreateCylinderColShape(pos, range, 3, 0);

                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        if (entity.HasData("HEAL_TIMER"))
                        {
                            Notify.Error(entity, "Вы должны закончить лечение чтоб покинуть больницу!");
                            NAPI.Entity.SetEntityPosition(entity, new Vector3(316.15222, -582.3583, 43.164104));
                        }
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
            }
        }
        public static void onResourceStart()
        {
            try
            {
                NAPI.Blip.CreateBlip(590, LSNews.LSNewsCoords[0], 0.7f, 45, Main.StringToU16("Новостное Агенство"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(489, new Vector3(299.77252, -584.85333, 0.16406), 1.1f, 1, Main.StringToU16("Emergency Medical Center"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(88, Fbi.EnterFBI, 1, 4, Main.StringToU16("Federal Investigation Buerau"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(60, new Vector3(435.1756, -981.82, 29.56958), 0.8f, 53, Main.StringToU16("Los Santos Police Department"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(85, new Vector3(-2449.677, 3287.597, 32), 0.7f, 28, Main.StringToU16("San Andreas National Guard"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(419, new Vector3(-554.94415, -186.95253, 37.1011), 1.2f, 4, Main.StringToU16("Government"), 255, 0, true, 0, 0);

                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[1], 0.7f, 11, Main.StringToU16("The Families"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[2], 0.7f, 50, Main.StringToU16("The Ballas Gang"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[3], 0.7f, 70, Main.StringToU16("Los Santos Vagos"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[4], 0.7f, 53, Main.StringToU16("Marabunta Grande"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(84, Manager.FractionSpawns[5], 0.7f, 59, Main.StringToU16("The Bloods Gang"), 255, 0, true, 0, 0);

                NAPI.Blip.CreateBlip(78, Manager.FractionSpawns[11], 1.1f, 4, Main.StringToU16("Русская Мафия"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(78, Manager.FractionSpawns[13], 1.1f, 5, Main.StringToU16("Итальянское Посольство"), 255, 0, true, 0, 0);

                NAPI.Blip.CreateBlip(621, Manager.FractionSpawns[67], 0.7f, 48, Main.StringToU16("Церковь"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(50, new Vector3(904.9575, -2926.302, 4.780589), 0.7f, 6, Main.StringToU16("Контейнеры"), 255, 0, true, 0, 0);

                new EMSExitCheckpoint(new Vector3(359.77145, -585.01654, 27.69834), 3f);
                new EMSExitCheckpoint(new Vector3(355.5426, -595.9737, 27.65402), 3f);
                new EMSExitCheckpoint(new Vector3(297.90366, -584.11224, 42.122063), 3f);
                new EMSExitCheckpoint(new Vector3(319.09128, -559.2485, 27.623772), 3f);
                new EMSExitCheckpoint(new Vector3(327.62537, -559.3047, 27.623772), 6f);
                new EMSExitCheckpoint(new Vector3(339.7418, -564.4038, 27.623802), 6f);

                NAPI.Blip.CreateBlip(567, new Vector3(-1518.477, 87.38084, 55.189007), 0.7f, 47, "ЧОО", 255, 0, true, 0, 0);

                var result = MySQL.QueryRead("SELECT `uuid`,`firstname`,`lastname`,`fraction`,`fractionlvl` FROM `characters`");
                if (result != null)
                {
                    foreach (DataRow Row in result.Rows)
                    {
                        var memberData = new MemberData();
                        memberData.Name = $"{Convert.ToString(Row["firstname"])}_{Convert.ToString(Row["lastname"])}";
                        memberData.FractionID = Convert.ToInt32(Row["fraction"]);
                        memberData.FractionLVL = Convert.ToInt32(Row["fractionlvl"]);
                        memberData.inFracName = getNickname(memberData.FractionID, memberData.FractionLVL);

                        if (memberData.FractionID != 0)
                            AllMembers.Add(memberData);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"FRACTIONS_MANAGER\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        public static List<List<object>> FracLogs = new List<List<object>> { new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { }, new List<object> { } };

        public static Dictionary<Weapons.Hash, int> matsForGun = new Dictionary<Weapons.Hash, int>()
        {
            { Weapons.Hash.Pistol, 50 },
            { Weapons.Hash.SNSPistol, 40 },
            { Weapons.Hash.DoubleBarrelShotgun, 80 },
            { Weapons.Hash.SawnOffShotgun, 100 },
            { Weapons.Hash.MachinePistol, 130 },
            { Weapons.Hash.Revolver, 290 },
            { Weapons.Hash.MiniSMG, 120 },
            { Weapons.Hash.Bat, 30 },
            { Weapons.Hash.Machete, 30 },
            { Weapons.Hash.Pistol50, 80 },
            { Weapons.Hash.CombatPistol, 80 },
            { Weapons.Hash.VintagePistol, 70 },
            { Weapons.Hash.PumpShotgun, 120 },
            { Weapons.Hash.BullpupShotgun, 140 },
            { Weapons.Hash.AssaultRifle, 200 },
            { Weapons.Hash.CompactRifle, 180 },
            { Weapons.Hash.Hatchet, 50 },
            { Weapons.Hash.GolfClub, 50 },
            { Weapons.Hash.SwitchBlade, 50 },
            { Weapons.Hash.Hammer, 50 },
            { Weapons.Hash.MicroSMG, 150 },
            { Weapons.Hash.Nightstick, 30 },
            { Weapons.Hash.SMG, 60 },
            { Weapons.Hash.CombatPDW, 60 },
            { Weapons.Hash.StunGun, 100 },
            { Weapons.Hash.CarbineRifle, 100 },
            { Weapons.Hash.SmokeGrenade, 5 },
            { Weapons.Hash.HeavyShotgun, 500 },
            { Weapons.Hash.Knife, 20 },
            { Weapons.Hash.SniperRifle, 150 },
            { Weapons.Hash.HeavySniper, 1200 },
            { Weapons.Hash.AssaultSMG, 50 },
            { Weapons.Hash.AdvancedRifle, 50 },
            { Weapons.Hash.Gusenberg, 500 },
            { Weapons.Hash.CombatMG, 500 },
        };
        public static int matsForArmor = 250;
        private static List<List<string>> gangGuns = new List<List<string>>
        {
            new List<string>
            {
                "Pistol",
                "SNSPistol",
                "Бронежилет",
                "Revolver"
            },
            new List<string>
            {
                "DoubleBarrelShotgun",
                "SawnOffShotgun",
            },
            new List<string>
            {
                "MicroSMG",
            },
            new List<string>(),
        };
        private static List<List<string>> mafiaGuns = new List<List<string>>
        {
            new List<string>
            {
                "Pistol",
                "Pistol50",
                "VintagePistol",
                "Бронежилет",
				"PumpShotgun",
				"MiniSMG",
				"AssaultRifle",
                "CompactRifle",
            },
            new List<string>
            {
                
            },
            new List<string>
            {
                
            },
            new List<string>
            {
                
            },
        };

        public static Dictionary<Player, MemberData> Members = new Dictionary<Player, MemberData>();
        public static SortedList<int, Vector3> FractionSpawns = new SortedList<int, Vector3>()
        {
            {1, new Vector3(-22.7138, -1406.353, 28.5081)},    // The Families
            {2, new Vector3(109.8277, -2014.295, 17.3004)},     // The Ballas Gang
            {3, new Vector3(823.0684, -2336.574, 29.4647)},     // Los Santos Vagos
            {4, new Vector3(1377.317, -2096.102, 51.6482)},     // Marabunta Grande
            {5, new Vector3(967.5901, -1831.698, 30.2642)},     // Blood Street
            {6, new Vector3(-553.74335, -188.93385, 38)},      // Cityhall
            {7, new Vector3(439.1714, -982.04346, 29.569563)},      // LSPD police
            {8, new Vector3(296.04504, -580.946, 43.16986)},      // Emergency care
            {9, new Vector3(149.4746, -756.9065, 243.0319)},     // FBI 
            {10, new Vector3(-1516.0873, 110.48351, 55.1)},     // La Cosa Nostra 
            {11, new Vector3(-108.7949, 987.5535, 234.7884)},    // Russian Mafia
            {12, new Vector3(-1546.365, 124.0002, 56.65994)},    // Мексиканская мафия
            {13, new Vector3(-1887.016, 2053.428, 140.0099)},    // Armenian Mafia 
            {14, new Vector3(-2357.629, 3253.3296, 32.80404)},   // Army
            {15, new Vector3(-602.7327, -929.8058, 23.74406)},    // LSNews
            {16, new Vector3(944.8635, 6.0115, 74.7413)},    // NamanaBet
            {17, new Vector3(4990.6187, -5717.935, 19.760202)},    // Меривейзер
			{18, new Vector3(-144.5746, -642.7132, 168.7005)}, // Gruppe 6
            {67, new Vector3(-777.6857, -708.8641, -40.93192)},    // Церковь

        };
        public static SortedList<int, int> FractionTypes = new SortedList<int, int>() // 0 - mafia, 1 gangs, 2 - gov, 
        {

            {0, -1},
            {1, 1}, // The Families
            {2, 1}, // The Ballas Gang
            {3, 1},  // Los Santos Vagos
            {4, 1}, // Marabunta Grande
            {5, 1}, // Blood Street
            {6, 2}, // Cityhall
            {7, 2}, // LSPD police
            {8, 2}, // Emergency care
            {9, 2}, // FBI 
            {10, 0}, // La Cosa Nostra 
            {11, 0}, // Russian Mafia
            {12, 0}, // Yakuza 
            {13, 0}, // Armenian Mafia 
            {14, 2}, // Army
            {15, 2}, // News
            {16, 2}, // NamanaBet
            {17, 2}, // Merryweather
			{18, 2}, // Gruppe 6
			
			
        };
        public static string GetFracType(int fracid)
        {
            var result = "";
            switch (fracid) 
            {
                case -1:
                    result = "Без фракции";
                    break;
                case 0:
                    result = "Мафия";
                    break;
                case 1:
                    result = "Банда";
                    break;
                case 2:
                    result = "Государственная";
                    break;
                default:
                    result = "Без фракции";
                    break;
            }
            return result;
        } 
        public static SortedList<int, string> FractionNames = new SortedList<int, string>()
        {
            {0, "-" },
            {1, "The Families" },
            {2, "The Ballas Gang" },
            {3, "Los Santos Vagos" },
            {4, "Marabunta Grande" },
            {5, "The Bloods Gang" },
            {6, "Правительство" },
            {7, "Los Santos Police Department" },
            {8, "Emergency Medical Center" },
            {9, "FBI" },
            {10, "ЧОО" },
            {11, "Русская Мафия" },
            {12, "Мексиканская Мафия" },
            {13, "Итальянское Посольство" },
            {14, "Армия" },
            {15, "Новостное Агенство" },
            {16, "NamanaBet" },
            {17, "Частная Военная Компания" },
            {18, "Gruppe 6" },
            {67, "Церковь" },
        };
        public static List<MemberData> AllMembers = new List<MemberData>();

        public static void fractionChat(Player sender, string message)
        {
            try
            {
                if (sender == null || !Main.Players.ContainsKey(sender)) return;
                if (Main.Players[sender].FractionID == 0) return;

                if (Main.Players[sender].Unmute > 0)
                {
                    Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[sender].Unmute / 60} минут", 3000);
                    return;
                }

                int Fraction = Main.Players[sender].FractionID;
                if (!Members.ContainsKey(sender)) return;
                string msgSender = $"~g~{Members[sender].inFracName} " + sender.Name.ToString().Replace('_', ' ') + " (" + sender.Value + "):~w~ " + message;
                var fracid = Main.Players[sender].FractionID;
                foreach (var p in NAPI.Pools.GetAllPlayers())
                {
                    if (p == null || !Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].FractionID == fracid)
                        NAPI.Chat.SendChatMessageToPlayer(p, msgSender);
                }
            }
            catch (Exception e) { Log.Write($"FractionChat:\n {e.ToString()}", nLog.Type.Error); }
        }

        public static Dictionary<int, int> GovIds = new Dictionary<int, int>
        {
            { 7, 14 },
            { 6, 8 },
            { 8, 11 },
            { 9, 14 },
            { 14, 15 },
            { 15, 16 },
            { 17, 15 },
            { 18, 15 }
        };
        public static Dictionary<int, string> GovTags = new Dictionary<int, string>
        {
            { 7, "Полиция ЛС" },
            { 6, "Правительство" },
            { 8, "Мед.Центр ЛС" },
            { 9, "FIB" },
            { 14, "Армия" },
            { 15, "Новости" },
            { 16, "NAMANABET" },
            { 17, "PMC" },
            { 18, "GRUPPE6" }
        };
        public static void govFractionChat(Player sender, string message)
        {
            if (!GovIds.ContainsKey(Main.Players[sender].FractionID)) return;
            if (!canUseCommand(sender, "dep")) return;
            if (Main.Players[sender].Unmute > 0)
            {
                Notify.Send(sender, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы замучены еще на {Main.Players[sender].Unmute / 60} минут", 3000);
                return;
            }
            int Fraction = Main.Players[sender].FractionID;

            var color = "!{#00DFFF}";
            string msgSender = $"{color}[{GovTags[Fraction]} | {Members[sender].inFracName}]" + sender.Name.ToString().Replace('_', ' ') + " (" + sender.Value + "):~w~" + message;
            var fracid = Main.Players[sender].FractionID;
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null) continue;
                if (!Main.Players.ContainsKey(p)) continue;
                if (GovIds.ContainsKey(Main.Players[p].FractionID))
                    NAPI.Chat.SendChatMessageToPlayer(p, msgSender);
            }
        }

        public static void Load(Player player, int fractionID, int fractionLVL)
        {
            if (Members.ContainsKey(player)) Members.Remove(player);
            MemberData data = new MemberData();
            data.FractionID = fractionID;
            data.FractionLVL = fractionLVL;
            data.inFracName = getNickname(fractionID, fractionLVL);
            data.Name = player.Name.ToString();
            Members.Add(player, data);

            if (fractionID == 14 && fractionLVL < 6)
                Main.Players[player].OnDuty = true;

            if (Main.Players[player].OnDuty)
            {
                setSkin(player, fractionID, fractionLVL);
                player.SetData("ON_DUTY", true);
            }

            var index = AllMembers.FindIndex(d => d.Name == player.Name);
            if (index == -1) AllMembers.Add(data);
            else
            {
                AllMembers[index].FractionID = data.FractionID;
                AllMembers[index].FractionLVL = data.FractionLVL;
                AllMembers[index].inFracName = data.inFracName;
            }
            Trigger.PlayerEvent(player, "fractionChange", fractionID);
            player.SetSharedData("fraction", fractionID);
            player.SetSharedData("fractionRankName", getNickname(fractionID, fractionLVL));
            Log.Write($"Member {player.Name.ToString()} loaded. ", nLog.Type.Success);
        } 
        public static void UNLoad(Player player)
        {
            try
            {
                if (!Members.ContainsKey(player)) return;
                Members.Remove(player);
                Trigger.PlayerEvent(player, "fractionChange", 0);
                player.SetSharedData("fraction", 0);
                Trigger.PlayerEvent(player, "closePc");
                player.SetData("ON_DUTY", false);
                MenuManager.Close(player);
                Trigger.PlayerEvent(player, "deleteFracBlips");
                GangsCapture.UnLoadBlips(player);

                if (Main.Players[player].FractionID == 9)
                {
                    var data = (Main.Players[player].Gender) ? "128_0_true" : "98_0_false";
                    var item = nInventory.Items[Main.Players[player].UUID].FirstOrDefault(i => i.Type == ItemType.Jewelry && i.Data == data);
                    if (item != null)
                    {
                        if (item.IsActive)
                        {
                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory = new ComponentItem(0, 0);
                            player.SetClothes(7, 0, 0);
                        }
                        nInventory.Remove(player, item);
                    }
                }
                Log.Write($"Member {player.Name.ToString()} unloaded.", nLog.Type.Success);
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        public static void Spawn(Player player)
        {
            if (!Members.ContainsKey(player)) return;
            Vector3 spawnPos = FractionSpawns[Members[player].FractionID];
            NAPI.Entity.SetEntityPosition(player, spawnPos);
        }
        public static int countOfFractionMembers(int fracID)
        {
            int count = 0;
            foreach (var p in Members.Values)
            {
                if (p.FractionID == fracID) count++;
            }
            return count;
        }
        public static bool isHaveFraction(Player player)
        {
            return Main.Players[player].FractionID != 0;
        }
        public static bool inFraction(Player player, int FracID)
        {
            return Main.Players[player].FractionID == FracID;
        }
        public static bool isLeader(Player player, int FracID)
        {
            return Main.Players[player].FractionID == FracID && Main.Players[player].FractionLVL == Configs.FractionRanks[FracID].Count;
        }
        public static string getName(int FractionID)
        {
            if (!FractionNames.ContainsKey(FractionID))
                return null;
            return FractionNames[FractionID];
        }

        public static void sendFractionMessage(int fracid, string message, bool inChat = false)
        {
            var all_players = NAPI.Pools.GetAllPlayers();
            if (inChat)
            {
                foreach (var p in all_players)
                {
                    if (p == null) continue;
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].FractionID == fracid)
                        p.SendChatMessage(message);
                }
            }
            else
            {
                foreach (var p in all_players)
                {
                    if (p == null) continue;
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].FractionID == fracid)
                        Notify.Send(p, NotifyType.Warning, NotifyPosition.BottomCenter, message, 3000);
                }
            }
        }

        public static void sendFractionPictureNotification(int fracid, string sender, string submessage, string message, string pic)
        {
            var all_players = NAPI.Pools.GetAllPlayers();
            foreach (var p in all_players)
            {
                if (p == null) continue;
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].FractionID == fracid)
                    NAPI.Notification.SendPictureNotificationToPlayer(p, message, pic, 0, 0, sender, submessage);
            }
        }
        public static void sendWaypoint(int fracid, Vector3 pos)
        {
            var all_players = NAPI.Pools.GetAllPlayers();

            foreach (var p in all_players)
            {
                if (p == null) continue;
                if (!Main.Players.ContainsKey(p)) continue;
                if (Main.Players[p].FractionID == fracid)
                    Trigger.PlayerEvent(p, "createWaypoint", pos.X, pos.Y);
            }
        }
        public static bool canUseCommand(Player player, string command, bool notify = true)
        {
            if (Main.Players[player].AdminLVL > 0) return true;
            if (player == null || !NAPI.Entity.DoesEntityExist(player)) return false;
            // Get player FractionID
            int fracid = Main.Players[player].FractionID;
            int fraclvl = Main.Players[player].FractionLVL;
            int minrank = 100;
            // Fractions available commands //

            if (Configs.FractionCommands.ContainsKey(fracid) && Configs.FractionCommands[fracid].ContainsKey(command))
                minrank = Configs.FractionCommands[fracid][command];

            #region Logic
            if (fraclvl < minrank)
            {
                if (notify && minrank < 100)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нет доступа", 3000);
                return false;
            }
            else return true;
            #endregion Logic
        }
        public static bool canGetWeapon(Player player, string weapon, bool notify = true)
        {
            // Get player FractionID
            int fracid = Main.Players[player].FractionID;
            int fraclvl = Main.Players[player].FractionLVL;
            int minrank = 100;
            // Fractions available commands //

            if (Configs.FractionWeapons.ContainsKey(fracid) && Configs.FractionWeapons[fracid].ContainsKey(weapon))
                minrank = Configs.FractionWeapons[fracid][weapon];

            #region Logic
            if (fraclvl < minrank)
            {
                if (notify)
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нет доступа", 3000);
                return false;
            }
            else return true;
            #endregion Logic
        }
        public static void setSkin(Player player, int fracID, int fracLvl)
        { // Only magic 
            bool gender = Main.Players[player].Gender;

            var clothes = (gender) ? Configs.FractionRanks[fracID][fracLvl].Item2 : Configs.FractionRanks[fracID][fracLvl].Item3;
            if (clothes == "null") return;

            Customization.ApplyCharacter(player);
            Customization.ClearClothes(player, gender);

            if (gender)
            {
                switch (clothes)
                {
                    case "city_1":
                        player.SetAccessories(1, 1, 1);
                        player.SetClothes(11, 242, 2);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(8, 129, 0);
                        player.SetClothes(6, 54, 0);
                        player.SetClothes(3, 0, 0);
                        break;
                    case "city_2":
                        player.SetClothes(8, 7, 0);
                        player.SetClothes(11, 120, 11);
                        player.SetClothes(4, 25, 2);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(3, 11, 0);
                        break;
                    case "city_3":
                        player.SetClothes(8, 71, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(4, 25, 2);
                        player.SetClothes(11, 28, 2);
                        player.SetClothes(3, 1, 0);
                        break;
                    case "city_4":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(8, 58, 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 54, 0);
                        player.SetClothes(7, 10, 2);
                        player.SetAccessories(1, 1, 1);
                        player.SetClothes(3, Customization.CorrectTorso[true][13], 0);
                        break;
                    case "city_5":
                        player.SetClothes(11, 60, 0);
                        player.SetClothes(8, 13, 0);
                        player.SetClothes(4, 10, 0);
                        player.SetClothes(6, 21, 0);
                        player.SetClothes(7, 10, 2);
                        player.SetAccessories(1, 1, 1);
                        player.SetClothes(3, 8, 0);
                        break;
                    case "city_6":
                        player.SetClothes(11, 60, 0);
                        player.SetClothes(8, 13, 0);
                        player.SetClothes(4, 10, 0);
                        player.SetClothes(6, 21, 0);
                        player.SetClothes(7, 10, 2);
                        player.SetAccessories(1, 1, 1);
                        player.SetClothes(3, 8, 0);
                        break;
                    case "city_7":
                        player.SetClothes(8, 31, 4);
                        player.SetClothes(7, 28, 4);
                        player.SetClothes(11, 32, 0);
                        player.SetClothes(6, 21, 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(3, 12, 0);
                        break;
                    case "city_8":
                        player.SetClothes(8, 31, 0);
                        player.SetClothes(7, 28, 12);
                        player.SetClothes(11, 32, 1);
                        player.SetClothes(6, 21, 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(3, 12, 0);
                        break;
                    case "city_9":
                        player.SetClothes(8, 31, 0);
                        player.SetClothes(7, 28, 15);
                        player.SetClothes(11, 32, 2);
                        player.SetClothes(6, 21, 0);
                        player.SetClothes(4, 25, 2);
                        player.SetClothes(3, 12, 0);
                        break;
                    case "police_1":
                        player.SetClothes(11, 26, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][26], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(10, 72, 0);
                        break;
                    case "police_2":
                        player.SetClothes(11, 55, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][55], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(8, 58, 0);
                        player.SetClothes(10, 72, 0);
                        break;
                    case "police_3":
                        player.SetClothes(11, 55, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][55], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(8, 58, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(10, 72, 0);
                        break;
                    case "police_4":
                        player.SetClothes(11, 53, 1);
                        player.SetClothes(3, 4, 0);
                        player.SetClothes(4, 33, 0);
                        player.SetClothes(6, 25, 0);
                        player.SetClothes(8, 122, 0);
                        player.SetClothes(7, 125, 0);
                        Customization.SetHat(player, 58, 2);
                        player.SetClothes(10, 72, 0);
                        break;
                    case "police_5":
                        player.SetClothes(11, 319, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 35, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        player.SetClothes(10, 72, 0);
                        Customization.SetHat(player, 46, 0);
                        break;
                    case "police_6":
                        player.SetClothes(11, 13, 2);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        player.SetClothes(10, 72, 0);
                        break;
                    case "police_7":
                        player.SetClothes(11, 13, 5);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(8, 130, 0);
                        player.SetClothes(10, 72, 0);
                        break;
                    case "police_8":
                        player.SetClothes(11, 72, 2);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][13], 0);
                        player.SetClothes(4, 25, 0);
                        player.SetClothes(8, 34, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(7, 125, 0);
                        player.SetClothes(10, 72, 0);
                        Customization.SetHat(player, 26, 0);
                        break;
                    case "ems_1":
                        player.SetClothes(11, 250, 1);
                        player.SetClothes(3, 85, 0);
                        player.SetClothes(4, 304, 0);
                        player.SetClothes(6, 8, 0);
                        player.SetClothes(7, 127, 0);
                        player.SetClothes(10, 58, 0);
                        break;
                    case "ems_2":
                        player.SetClothes(11, 250, 0);
                        player.SetClothes(3, 85, 0);
                        player.SetClothes(4, 304, 0);
                        player.SetClothes(6, 8, 0);
                        player.SetClothes(7, 127, 0);
                        player.SetClothes(10, 58, 0);
                        break;
                    case "ems_3":
                        player.SetClothes(11, 249, 1);
                        player.SetClothes(3, 86, 1);
                        player.SetClothes(4, 96, 1);
                        player.SetClothes(6, 8, 0);
                        player.SetClothes(7, 126, 0);
                        player.SetClothes(10, 57, 0);
                        break;
                    case "ems_4":
                        player.SetClothes(11, 249, 0);
                        player.SetClothes(3, 86, 1);
                        player.SetClothes(4, 96, 0);
                        player.SetClothes(6, 8, 0);
                        player.SetClothes(7, 126, 0);
                        player.SetClothes(10, 57, 0);
                        break;
                    case "army_1":
                        Customization.SetHat(player, 114, 12);
                        player.SetClothes(11, 208, 8);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][208], 0);
                        player.SetClothes(4, 88, 8);
                        player.SetClothes(6, 62, 7);
                        break;
                    case "army_2":
                        Customization.SetHat(player, 63, 0);
                        player.SetClothes(11, 220, 8);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][220], 0);
                        player.SetClothes(4, 86, 8);
                        player.SetClothes(6, 62, 7);
                        break;
                    case "army_3":
                        Customization.SetHat(player, 60, 0);
                        player.SetClothes(11, 220, 8);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][220], 0);
                        player.SetClothes(4, 87, 8);
                        player.SetClothes(6, 62, 7);
                        Customization.SetHat(player, 114, 19);
                        break;
                    case "army_4":
                        Customization.SetHat(player, 59, 0);
                        player.SetClothes(11, 222, 8);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][222], 0);
                        player.SetClothes(4, 86, 8);
                        player.SetClothes(6, 63, 7);
                        break;
                    case "army_5":
                        Customization.SetHat(player, 84, 4);
                        player.SetClothes(11, 222, 8);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][222], 0);
                        player.SetClothes(4, 87, 8);
                        player.SetClothes(6, 62, 7);
                        Customization.SetHat(player, 59, 0);
                        break;
                    case "army_6":
                        Customization.SetHat(player, 89, 4);
                        player.SetClothes(11, 221, 3);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][221], 0);
                        player.SetClothes(4, 86, 8);
                        player.SetClothes(6, 63, 7);
                        break;
                    case "army_7":
                        Customization.SetHat(player, 107, 6);
                        player.SetClothes(11, 221, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][221], 0);
                        player.SetClothes(4, 87, 6);
                        player.SetClothes(6, 62, 7);
                        break;
                    case "army_8":
                        Customization.SetHat(player, 112, 3);
                        player.SetClothes(11, 219, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][219], 0);
                        player.SetClothes(4, 87, 6);
                        player.SetClothes(6, 62, 7);
                        break;
                    case "army_9":
                        Customization.SetHat(player, 106, 6);
                        player.SetClothes(11, 222, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][222], 0);
                        player.SetClothes(4, 86, 6);
                        player.SetClothes(6, 63, 7);
                        break;
                    case "army_10":
                        Customization.SetHat(player, 104, 6);
                        player.SetClothes(11, 232, 9);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        player.SetClothes(8, 121, 6);
                        player.SetClothes(4, 87, 6);
                        player.SetClothes(6, 62, 7);
                        player.SetClothes(10, 78, 0);
                        break;
                    case "army_11":
                        Customization.SetHat(player, 106, 6);
                        player.SetClothes(11, 228, 17);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][228], 0);
                        player.SetClothes(4, 87, 6);
                        player.SetClothes(6, 62, 7);
                        player.SetClothes(10, 78, 0);
                        break;
                    case "army_12":
                        Customization.SetHat(player, 108, 6);
                        player.SetClothes(11, 222, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][222], 0);
                        player.SetClothes(4, 86, 6);
                        player.SetClothes(6, 63, 7);
                        player.SetClothes(10, 78, 0);
                        break;
                    case "army_13":
                        Customization.SetHat(player, 114, 12);
                        player.SetClothes(11, 221, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][221], 0);
                        player.SetClothes(4, 87, 6);
                        player.SetClothes(6, 62, 7);
                        player.SetClothes(10, 78, 0);
                        break;
                    case "army_14":
                        Customization.SetHat(player, 114, 19);
                        player.SetClothes(11, 220, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][220], 0);
                        player.SetClothes(4, 87, 6);
                        player.SetClothes(6, 62, 7);
                        player.SetClothes(10, 78, 0);
                        break;
                    case "army_15":
                        Customization.SetHat(player, 113, 8);
                        player.SetClothes(6, 21, 4);
                        player.SetClothes(4, 25, 3);
                        player.SetClothes(8, 34, 3);
                        player.SetClothes(11, 77, 3);
                        player.SetClothes(10, 78, 0);
                        break;
                    case "mco_17":
                        player.SetClothes(11, 220, 10);
						player.SetClothes(3, 139, 10);
						player.SetClothes(4, 87, 10);
						player.SetClothes(6, 25, 0);
                        break;
                    case "news_1":
                        player.SetClothes(11, 82, 9);
                        player.SetClothes(4, 24, 5);
                        player.SetClothes(6, 77, 2);
                        break;
                    case "news_2":
                        player.SetClothes(6, 77, 0);
                        player.SetClothes(11, 358, 1);
                        player.SetClothes(4, 24, 5);
                        break;
                    case "news_3":
                        player.SetClothes(4, 28, 0);
                        player.SetClothes(6, 77, 3);
                        player.SetClothes(11, 193, 2);
                        break;
                    case "nac_1":
                        player.SetClothes(11, 60, 0);
                        player.SetClothes(3, 8, 0);
                        player.SetClothes(4, 49, 0);
                        player.SetClothes(6, 21, 0);
                        player.SetClothes(7, 128, 0);
                        player.SetClothes(8, 11, 0);
                        break;
                    case "fib_1":
                        player.SetClothes(10, 76, 0);
                        player.SetClothes(9, 53, 0);
                        player.SetClothes(9, 4, 1);
                        player.SetClothes(4, 7, 0);
                        player.SetClothes(11, 13, 2);
                        player.SetClothes(6, 69, 0);
                        player.SetClothes(0, 142, 0);
                        break;
                    case "fib_2":
                        player.SetClothes(9, 53, 0); 
                        player.SetClothes(10, 76, 0);
                        player.SetClothes(4, 73, 0);
                        player.SetClothes(6, 25, 0);
                        player.SetClothes(11, 50, 0);
                        player.SetClothes(0, 142, 0);
                        break;
                    case "fib_3":
                        player.SetClothes(0, 126, 0);
                        player.SetClothes(6, 50, 0);
                        player.SetClothes(9, 53, 0);
                        player.SetClothes(4, 34, 0);
                        player.SetClothes(11, 53, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][53], 0);
                        player.SetClothes(9, 16, 0);
                        player.SetClothes(7, 147, 0);
                        player.SetClothes(2, 1, 0);
                        player.SetClothes(10, 76, 0);
                        break;
                    case "fib_4":
                        player.SetClothes(0, 12, 0);
                        player.SetClothes(6, 97, 0);
                        player.SetClothes(4, 24, 0);
                        player.SetClothes(8, 28, 0);
                        player.SetClothes(11, 76, 1);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][76], 0);
                        player.SetClothes(10, 76, 0);
                        break;
                    case "fib_5":
                        player.SetClothes(9, 53, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(11, 58, 0);
                        player.SetClothes(4, 24, 0);
                        player.SetClothes(8, 26, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][58], 0);
                        player.SetClothes(10, 76, 0);
                        break;
                }
            }
            else
            {
                switch (clothes)
                {
                    case "city_1":
                        player.SetClothes(11, 250, 2);
                        player.SetAccessories(1, 0, 1);
                        player.SetClothes(8, 159, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(4, 64, 1);
                        player.SetClothes(3, 14, 0);
                        break;
                    case "city_2":
                        player.SetClothes(8, 24, 0);
                        player.SetClothes(11, 28, 10);
                        player.SetClothes(4, 36, 2);
                        player.SetClothes(6, 6, 0);
                        player.SetClothes(3, 0, 0);
                        break;
                    case "city_3":
                        player.SetClothes(11, 25, 7);
                        player.SetClothes(8, 67, 3);
                        player.SetClothes(4, 47, 0);
                        player.SetClothes(6, 6, 0);
                        player.SetClothes(3, 3, 0);
                        break;
                    case "city_4":
                        player.SetClothes(8, 35, 0);
                        player.SetClothes(11, 27, 0);
                        player.SetClothes(4, 64, 1);
                        player.SetClothes(6, 29, 0);
                        player.SetAccessories(1, 0, 1);
                        player.SetClothes(3, Customization.CorrectTorso[false][27], 0);
                        break;
                    case "city_5":
                        player.SetAccessories(1, 0, 1);
                        player.SetClothes(11, 7, 0);
                        player.SetClothes(8, 38, 0);
                        player.SetClothes(7, 21, 2);
                        player.SetClothes(4, 2, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(3, 3, 0);
                        break;
                    case "city_6":
                        player.SetAccessories(1, 0, 1);
                        player.SetClothes(11, 139, 0);
                        player.SetClothes(8, 38, 0);
                        player.SetClothes(7, 21, 2);
                        player.SetClothes(4, 2, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(3, Customization.CorrectTorso[false][139], 0);
                        break;
                    case "city_7":
                        player.SetClothes(11, 6, 0);
                        player.SetClothes(4, 6, 0);
                        player.SetClothes(6, 42, 0);
                        player.SetClothes(8, 20, 1);
                        player.SetClothes(7, 12, 0);
                        player.SetClothes(3, Customization.CorrectTorso[false][6], 0);
                        break;
                    case "city_8":
                        player.SetClothes(8, 41, 2);
                        player.SetClothes(11, 6, 2);
                        player.SetClothes(4, 6, 2);
                        player.SetClothes(6, 42, 2);
                        player.SetClothes(3, Customization.CorrectTorso[false][6], 0);
                        break;
                    case "city_9":
                        player.SetClothes(4, 50, 0);
                        player.SetClothes(11, 7, 1);
                        player.SetClothes(6, 0, 0);
                        player.SetClothes(8, 38, 0);
                        player.SetClothes(7, 22, 0);
                        player.SetClothes(3, Customization.CorrectTorso[false][6], 0);
                        break;
                    case "police_1":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(8, 35, 0);
                        break;
                    case "police_2":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][48], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(8, 35, 0);
                        player.SetClothes(10, 7, 1);
                        break;
                    case "police_3":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][48], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(8, 35, 0);
                        player.SetClothes(10, 7, 2);
                        break;
                    case "police_4":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][46], 0);
                        player.SetClothes(4, 32, 0);
                        player.SetClothes(6, 25, 0);
                        player.SetClothes(8, 152, 0);
                        player.SetClothes(7, 95, 0);
                        Customization.SetHat(player, 58, 2);
                        break;
                    case "police_5":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        Customization.SetHat(player, 45, 0);
                        break;
                    case "police_6":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        break;
                    case "police_7":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        break;
                    case "police_8":
                        player.SetClothes(11, 13, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][27], 0);
                        player.SetClothes(4, 37, 0);
                        player.SetClothes(6, 29, 0);
                        player.SetClothes(7, 95, 0);
                        player.SetClothes(8, 152, 0);
                        break;
                    case "ems_1":
                        player.SetClothes(11, 73, 0);
                        player.SetClothes(4, 23, 3);
                        player.SetClothes(6, 1, 3);
                        player.SetClothes(3, 109, 0);
                        player.SetClothes(7, 97, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][73], 0);
                        break;
                    case "ems_2":
                        player.SetClothes(11, 249, 1);
                        player.SetClothes(4, 23, 3);
                        player.SetClothes(6, 1, 3);
                        player.SetClothes(3, 109, 1);
                        player.SetClothes(7, 96, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][249], 0);
                        break;
                    case "ems_3":
                        player.SetClothes(11, 249, 2);
                        player.SetClothes(4, 23, 0);
                        player.SetClothes(6, 1, 3);
                        player.SetClothes(3, 109, 0);
                        player.SetClothes(7, 96, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][249], 0);
                        break;
                    case "ems_4":
                        player.SetClothes(11, 244, 4);
                        player.SetClothes(4, 23, 3);
                        player.SetClothes(6, 1, 3);
                        player.SetClothes(3, 93, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][244], 0);
                        break;
                    case "army_1":
                        player.SetClothes(11, 212, 3);
                        player.SetClothes(4, 91, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][212], 0);
                        break;
                    case "army_2":
                        player.SetClothes(11, 230, 3);
                        player.SetClothes(4, 89, 3);
                        player.SetClothes(6, 66, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][230], 0);
                        break;
                    case "army_3":
                        Customization.SetHat(player, 102, 3);
                        player.SetClothes(11, 230, 3);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][230], 0);
                        break;
                    case "army_4":
                        Customization.SetHat(player, 102, 3);
                        player.SetClothes(11, 232, 3);
                        player.SetClothes(4, 89, 3);
                        player.SetClothes(6, 66, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
                    case "army_5":
                        Customization.SetHat(player, 106, 3);
                        player.SetClothes(11, 232, 3);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
                    case "army_6":
                        Customization.SetHat(player, 104, 3);
                        player.SetClothes(11, 231, 3);
                        player.SetClothes(4, 89, 3);
                        player.SetClothes(6, 66, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][231], 0);
                        break;
                    case "army_7":
                        Customization.SetHat(player, 106, 3);
                        player.SetClothes(11, 231, 0);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][231], 0);
                        break;
                    case "army_8":
                        Customization.SetHat(player, 111, 11);
                        player.SetClothes(11, 226, 3);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, 4, 0);
                        break;
                    case "army_9":
                        Customization.SetHat(player, 105, 3);
                        player.SetClothes(11, 232, 3);
                        player.SetClothes(4, 89, 3);
                        player.SetClothes(6, 66, 6);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
                    case "army_10":
                        Customization.SetHat(player, 105, 14);
                        player.SetClothes(11, 243, 7);
                        player.SetClothes(8, 141, 14);
                        player.SetClothes(4, 90, 14);
                        player.SetClothes(6, 65, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][243], 0);
                        break;
                    case "army_11":
                        Customization.SetHat(player, 105, 9);
                        player.SetClothes(11, 238, 15);
                        player.SetClothes(4, 90, 9);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][238], 0);
                        break;
                    case "army_12":
                        Customization.SetHat(player, 107, 14);
                        player.SetClothes(11, 232, 14);
                        player.SetClothes(4, 89, 14);
                        player.SetClothes(6, 66, 0);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
                    case "army_13":
                        Customization.SetHat(player, 113, 15);
                        player.SetClothes(11, 231, 5);
                        player.SetClothes(4, 90, 5);
                        player.SetClothes(6, 65, 0);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][231], 0);
                        break;
                    case "army_14":
                        Customization.SetHat(player, 113, 15);
                        player.SetClothes(11, 230, 4);
                        player.SetClothes(4, 90, 4);
                        player.SetClothes(6, 65, 0);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][230], 0);
                        break;
                    case "army_15":
                        Customization.SetHat(player, 112, 5);
                        player.SetClothes(11, 232, 3);
                        player.SetClothes(4, 90, 3);
                        player.SetClothes(6, 65, 6);
                        player.SetClothes(8, 6, 0);
                        player.SetClothes(3, Customization.CorrectTorso[Main.Players[player].Gender][232], 0);
                        break;
					case "mco_17":
                        player.SetClothes(11, 230, 10);
						player.SetClothes(3, 190, 10);
						player.SetClothes(4, 90, 10);
						player.SetClothes(6, 25, 0);
                        break;
					case "nac_1":
                        player.SetClothes(11, 60, 0);
                        player.SetClothes(3, 8, 0);
                        player.SetClothes(4, 49, 0);
                        player.SetClothes(6, 21, 0);
                        player.SetClothes(7, 128, 0);
                        player.SetClothes(8, 11, 0);
                        break;
                    case "fib_1":
                        player.SetClothes(10, 76, 0);
                        player.SetClothes(9, 53, 0);
                        player.SetClothes(9, 4, 1);
                        player.SetClothes(4, 7, 0);
                        player.SetClothes(11, 13, 2);
                        player.SetClothes(6, 69, 0);
                        player.SetClothes(0, 142, 0);
                        break;
                    case "fib_2":
                        player.SetClothes(9, 53, 0);
                        player.SetClothes(10, 76, 0);
                        player.SetClothes(4, 73, 0);
                        player.SetClothes(6, 25, 0);
                        player.SetClothes(11, 50, 0);
                        player.SetClothes(0, 142, 0);
                        break;
                    case "fib_3":
                        player.SetClothes(0, 126, 0);
                        player.SetClothes(6, 50, 0);
                        player.SetClothes(9, 53, 0);
                        player.SetClothes(4, 34, 0);
                        player.SetClothes(11, 53, 0);
                        player.SetClothes(9, 16, 0);
                        player.SetClothes(7, 147, 0);
                        player.SetClothes(2, 1, 0);
                        player.SetClothes(10, 76, 0);
                        break;
                    case "fib_4":
                        player.SetClothes(0, 12, 0);
                        player.SetClothes(6, 97, 0);
                        player.SetClothes(4, 24, 0);
                        player.SetClothes(8, 28, 0);
                        player.SetClothes(11, 76, 1);
                        player.SetClothes(10, 76, 0);
                        break;
                    case "fib_5":
                        player.SetClothes(9, 53, 0);
                        player.SetClothes(6, 10, 0);
                        player.SetClothes(11, 58, 0);
                        player.SetClothes(4, 24, 0);
                        player.SetClothes(8, 26, 0);
                        player.SetClothes(10, 76, 0);
                        break;
                }
            }

            if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
            else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
        }
        public static string getNickname(int fracID, int fracLvl)
        { // Only magic
            if (Configs.FractionRanks.ContainsKey(fracID) && Configs.FractionRanks[fracID].ContainsKey(fracLvl))
                return Configs.FractionRanks[fracID][fracLvl].Item1;
            return null;
        }

        public static Dictionary<Weapons.Hash, int> WeaponsMaxAmmo = new Dictionary<Weapons.Hash, int>()
        {
            { Weapons.Hash.Nightstick, 1 },
            { Weapons.Hash.Pistol, 12 },
            { Weapons.Hash.SMG, 30 },
            { Weapons.Hash.PumpShotgun, 8 },
            { Weapons.Hash.StunGun, 100 },
            { Weapons.Hash.Pistol50, 9 },
            { Weapons.Hash.CarbineRifle, 30 },
            { Weapons.Hash.SmokeGrenade, 1 },
            { Weapons.Hash.HeavyShotgun, 6 },
            { Weapons.Hash.Knife, 1 },
            { Weapons.Hash.SniperRifle, 10 },
            { Weapons.Hash.AssaultSMG, 30 },
            { Weapons.Hash.Gusenberg, 50 },
            { Weapons.Hash.CombatPistol, 10 },
            { Weapons.Hash.Revolver, 6 },
            { Weapons.Hash.HeavyPistol, 10 },
            { Weapons.Hash.SawnOffShotgun, 6 },
            { Weapons.Hash.BullpupShotgun, 10 },
            { Weapons.Hash.DoubleBarrelShotgun, 2 },
            { Weapons.Hash.MicroSMG, 15 },
            { Weapons.Hash.MachinePistol, 13 },
            { Weapons.Hash.CombatPDW, 30 },
            { Weapons.Hash.MiniSMG, 13 },
            { Weapons.Hash.SpecialCarbine, 30 },
            { Weapons.Hash.AssaultRifle, 30 },
            { Weapons.Hash.BullpupRifle, 30 },
            { Weapons.Hash.AdvancedRifle, 30 },
            { Weapons.Hash.CompactRifle, 30 },
            { Weapons.Hash.CombatMG, 100 },
        };

        public static void giveGun(Player player, Weapons.Hash gun, string weaponstr)
        {
            if (!Main.Players.ContainsKey(player) || !Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (player.HasData($"GET_{gun.ToString()}") && DateTime.Now < player.GetData<DateTime>($"GET_{gun.ToString()}"))
            {
                DateTime date = player.GetData<DateTime>($"GET_{gun.ToString()}");
                DateTime g = new DateTime((date - DateTime.Now).Ticks);
                var min = g.Minute;
                var sec = g.Second;
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы сможете взять {gun.ToString()} через {min}:{sec}", 3000);
                return;
            }

            var frac = Main.Players[player].FractionID;
            if (Configs.FractionWeapons[frac].ContainsKey(weaponstr) && Main.Players[player].FractionLVL < Configs.FractionWeapons[frac][weaponstr])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа к данному виду оружия", 3000);
                return;
            }
            if (Stocks.fracStocks[Main.Players[player].FractionID].Materials < matsForGun[gun])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На складе недостаточно материала", 3000);
                return;
            }

            var wType = (ItemType)Enum.Parse(typeof(ItemType), gun.ToString());
            if (nInventory.TryAdd(player, new nItem(wType)) == -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                return;
            }

            var serial = Weapons.GetSerial(true, Main.Players[player].FractionID);
            Weapons.GiveWeapon(player, wType, serial);

            Stocks.fracStocks[Main.Players[player].FractionID].Materials -= matsForGun[gun];
            Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();

            var minutes = 0;
            if (Main.Players[player].FractionID == 7) minutes = 0;
            player.SetData($"GET_{gun.ToString()}", DateTime.Now.AddMinutes(minutes));

            GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, $"{gun.ToString()}({serial})", 1, false);
            Manager.FracLogs[Main.Players[player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "" )}{DateTime.Now.Minute}", player.Name, $"{gun.ToString()}({serial})", 1, "скрафтил" });
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили {wType.ToString()}", 3000);
            return;
        }

        public static void giveAmmo(Player player, ItemType ammoType, int ammo)
        {
            if (!Main.Players.ContainsKey(player) || !Stocks.fracStocks.ContainsKey(Main.Players[player].FractionID)) return;

            if (Stocks.fracStocks[Main.Players[player].FractionID].Materials < MatsForAmmoType[ammoType] * ammo)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На складе недостаточно материала", 3000);
                return;
            }

            var tryAdd = nInventory.TryAdd(player, new nItem(ammoType, ammo));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                return;
            }

            Stocks.fracStocks[Main.Players[player].FractionID].Materials -= MatsForAmmoType[ammoType] * ammo;
            Stocks.fracStocks[Main.Players[player].FractionID].UpdateLabel();

            nInventory.Add(player, new nItem(ammoType, ammo));
            GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, ammoType.ToString(), 1, false);
            Manager.FracLogs[Main.Players[player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "" )}{DateTime.Now.Minute}", player.Name, ammoType.ToString(), ammo, "скрафтил" });
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы получили {nInventory.InventoryItems.Find(x => x.ID == (int)ammoType).Name} x{ammo}", 3000);
            return;
        }

        public static void enterInterier(Player player, int fractionId)
        {
            var position = (fractionId <= 5) ? Gangs.ExitPoints[fractionId] : Mafia.ExitPoints[fractionId];
            player.Position = position + new Vector3(0, 0, 1.12);
            Main.PlayerEnterInterior(player, position + new Vector3(0, 0, 1.12));

            Trigger.PlayerEvent(player, "stopTime");
        }

        public static void exitInterier(Player player, int fractionId)
        {
            var position = (fractionId <= 5) ? Gangs.EnterPoints[fractionId] : Mafia.EnterPoints[fractionId];
            player.Position = position + new Vector3(0, 0, 1.12);
            Main.PlayerEnterInterior(player, position + new Vector3(0, 0, 1.12));

            Trigger.PlayerEvent(player, "resumeTime");
        }

        public class MemberData
        {
            public string Name { get; set; }
            public int FractionID { get; set; }
            public int FractionLVL { get; set; }
            public string inFracName { get; set; }
        }

        #region CraftMenu
        public static void OpenGunCraftMenu(Player player)
        {
            int fracid = Main.Players[player].FractionID;
            List<List<string>> list = null;

            if (FractionTypes[fracid] == -1 || FractionTypes[fracid] == 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не умеете крафтить оружие", 3000);
                return;
            }
            else if (FractionTypes[fracid] == 1) list = gangGuns;
            else if (FractionTypes[fracid] == 0) list = mafiaGuns;

            List<List<int>> prices = new List<List<int>>();
            for (int i = 0; i < list.Count; i++)
            {
                List<int> p = new List<int>();
                foreach (string g in list[i])
                {
                    if (g == "Бронежилет")
                    {
                        p.Add(100);
                        continue;
                    }    
                    p.Add(matsForGun[Weapons.GetHash(g)]);
                }
                prices.Add(p);
            }
            string json = JsonConvert.SerializeObject(prices);
            Log.Debug(json);
            Trigger.PlayerEvent(player, "openWCraft", fracid, json);
        }
        [RemoteEvent("wcraft")]
        public static void Event_WCraft(Player Player, int frac, int cat, int index)
        {
            int where = -1;
            try
            {
                Log.Debug($"{frac.ToString()}:{cat.ToString()}:{index.ToString()}");
                List<List<string>> list = null;
                if (FractionTypes[frac] == 1) list = gangGuns;
                else if (FractionTypes[frac] == 0) list = mafiaGuns;
                if (list.Count < 1 || list.Count < cat + 1 || list[cat].Count < index + 1) return;

                if (list == gangGuns && cat == 0 && index == 2)
                {
                    if (FractionTypes[frac] == -1 || FractionTypes[frac] == 2)
                    {
                        Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не умеете крафтить бронежилет", 3000);
                        return;
                    }
                    var mItem2 = nInventory.Find(Main.Players[Player].UUID, ItemType.Material);
                    var count2 = (mItem2 == null) ? 0 : mItem2.Count;
                    if (count2 < 100)
                    {
                        Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно материала", 3000);
                        return;
                    }
                    var tryAdd2 = nInventory.TryAdd(Player, new nItem(ItemType.BodyArmor, 1));
                    if (tryAdd2 == -1 || tryAdd2 > 0)
                    {
                        Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                        return;
                    }
                    nInventory.Remove(Player, ItemType.Material, 100);
                    nInventory.Add(Player, new nItem(ItemType.BodyArmor, 1, "75"));
                    Notify.Send(Player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы скрафтили бронежилет за 100 матов", 3000);
                    return;
                }
				if (list == mafiaGuns && cat == 0 && index == 3)
				{
					if (FractionTypes[frac] == -1 || FractionTypes[frac] == 2)
                    {
                        Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не умеете крафтить бронежилет", 3000);
                        return;
                    }
                    var mItem2 = nInventory.Find(Main.Players[Player].UUID, ItemType.Material);
                    var count2 = (mItem2 == null) ? 0 : mItem2.Count;
                    if (count2 < 100)
                    {
                        Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно материала", 3000);
                        return;
                    }
                    var tryAdd2 = nInventory.TryAdd(Player, new nItem(ItemType.BodyArmor, 1));
                    if (tryAdd2 == -1 || tryAdd2 > 0)
                    {
                        Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                        return;
                    }
                    nInventory.Remove(Player, ItemType.Material, 100);
                    nInventory.Add(Player, new nItem(ItemType.BodyArmor, 1, "75"));
                    Notify.Send(Player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы скрафтили бронежилет за 100 матов", 3000);
                    return;
				}
					

                string selected = list[cat][index];
                if (FractionTypes[frac] == -1 || FractionTypes[frac] == 2)
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не умеете крафтить оружие", 3000);
                    return;
                }
                var mItem = nInventory.Find(Main.Players[Player].UUID, ItemType.Material);
                var count = (mItem == null) ? 0 : mItem.Count;
                if (count < matsForGun[Weapons.GetHash(selected)])
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно материала", 3000);
                    return;
                }

                var wType = (ItemType)Enum.Parse(typeof(ItemType), selected);
                var tryAdd = nInventory.TryAdd(Player, new nItem(wType, 1));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }

                nInventory.Remove(Player, ItemType.Material, matsForGun[Weapons.GetHash(selected)]);
                Weapons.GiveWeapon(Player, wType, Weapons.GetSerial(true, frac));
                Notify.Send(Player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы скрафтили {selected} за {matsForGun[Weapons.GetHash(selected)]} матов", 3000);
            }
            catch (Exception e)
            {
                Log.Write($"Event_WCraft/{where}/{frac}/{cat}/{index}/: \n{e.ToString()}", nLog.Type.Error);
            }
        }
        [RemoteEvent("wcraftammo")]
        public static void Event_WCraftAmmo(Player player, int frac, string text1, string text2)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                if (FractionTypes[frac] != 0 && FractionTypes[frac] != 1) return;

                var category = Convert.ToInt32(text1.Replace("wcraftslider", null));
                var mats = Convert.ToInt32(text2.Trim('M'));
                var ammo = mats / MatsForAmmo[category];

                if (ammo == 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не указали количество патрон", 3000);
                    return;
                }

                var matsItem = nInventory.Find(Main.Players[player].UUID, ItemType.Material);
                var matsCount = (matsItem == null) ? 0 : matsItem.Count;
                if (matsCount < MatsForAmmo[category] * ammo)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно материалов", 3000);
                    return;
                }

                var tryAdd = nInventory.TryAdd(player, new nItem(AmmoTypes[category], ammo));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                    return;
                }

                nInventory.Remove(player, ItemType.Material, MatsForAmmo[category] * ammo);
                nInventory.Add(player, new nItem(AmmoTypes[category], ammo));
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно скрафтили {nInventory.InventoryItems.Find(x => x.ID == (int)AmmoTypes[category]).Name} x{ammo}", 3000);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }
        private static Dictionary<ItemType, int> MatsForAmmoType = new Dictionary<ItemType, int>()
        {
            { ItemType.PistolAmmo, 1 },
            { ItemType.ShotgunsAmmo, 2 },
            { ItemType.SMGAmmo, 1 },
            { ItemType.RiflesAmmo, 2 },
            { ItemType.SniperAmmo, 5 },
        };
        private static List<int> MatsForAmmo = new List<int>()
        {
            1, // pistol
            2, // shotgun
            1, // smg
            2, // rifles
        };
        private static List<ItemType> AmmoTypes = new List<ItemType>()
        {
            ItemType.PistolAmmo,
            ItemType.ShotgunsAmmo,
            ItemType.SMGAmmo,
            ItemType.RiflesAmmo,
        };
        #endregion

        [RemoteEvent("setmembers")]
        public static void SetMembersToMenu(Player player)
        {
            try
            {
                Character acc = Main.Players[player];
                if (acc.FractionID == 0) return;
                List<List<object>> people = new List<List<object>>();

                var count = 0;
                var on = 0;
                var off = 0;

                for (int i = 0; i < AllMembers.Count; i++)
                {
                    if (i >= AllMembers.Count) break;
                    var member = AllMembers[i];
                    if (member.FractionID != acc.FractionID) continue;
                    count++;
                    bool online = false;
                    bool work = false;
                    string id = "-";
                    if (Members.Values.FirstOrDefault(m => m.Name == member.Name) != null)
                    {
                        id = NAPI.Player.GetPlayerFromName(member.Name).Value.ToString();
                        Player ply = NAPI.Player.GetPlayerFromName(member.Name);
                        online = true;
                        if (ply.HasData("ON_DUTY"))
                            work = player.GetData<bool>("ON_DUTY");
                        on++;
                    }
                    else
                        off++;
                    string ws = "Не работает";
                    if (work)
                        ws = "Работает";
                    string name = getNickname(member.FractionID, member.FractionLVL);
                    List<object> data = new List<object>
                    {
                        online,
                        id,
                        member.Name, 
                        ws,
                        "-",
                        {member.FractionLVL},
                        name
                    };
                    people.Add(data);
                }
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(people);
                Trigger.PlayerEvent(player, "setmem", json, count, on, off);
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"SETMEMBERS\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [RemoteEvent("openfmenu")]
        public static void OpenFractionMenu(Player player)
        {
            try
            {
                SetMembersToMenu(player);

                List<string> Names = new List<string>();

                if (isLeader(player, Main.Players[player].FractionID))
                    foreach (var rank in Configs.FractionRanks[Main.Players[player].FractionID])
                        Names.Add(rank.Value.Item1);

                Trigger.PlayerEvent(player, "openfm", FractionNames[Main.Players[player].FractionID], JsonConvert.SerializeObject(FracLogs[Main.Players[player].FractionID]), JsonConvert.SerializeObject(Names));
            }
            catch (Exception e) { Log.Write("EXCEPTION AT \"OPENFMENU\":\n" + e.ToString(), nLog.Type.Error); }
        }
        [RemoteEvent("fmenu")]
        public static void callback_FracMenu(Player player, params object[] args)
        {
            try
            {
                int act = Convert.ToInt32(args[0]);
                string data1 = Convert.ToString(args[1]);
                string data2 = Convert.ToString(args[2]);
                int rank;
                int id;
                switch (act)
                {
                    case 2: //invite
                        if (Int32.TryParse(data1, out id))
                        {
                            Player target = Main.GetPlayerByID(id);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                                return;
                            }
                            FractionCommands.InviteToFraction(player, target);
                        }
                        else
                        {
                            Player target = NAPI.Player.GetPlayerFromName(data1);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким именем не найден", 3000);
                                return;
                            }
                            FractionCommands.InviteToFraction(player, target);
                        }
                        break;
                    case 3: //kick
                        if (Int32.TryParse(data1, out id))
                        {
                            Player target = Main.GetPlayerByID(id);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                                return;
                            }
                            FractionCommands.UnInviteFromFraction(player, target);
                        }
                        else
                        {
                            if (!Main.PlayerNames.ContainsValue(data1))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким именем не найден", 3000);
                                return;
                            }
                            Player target = NAPI.Player.GetPlayerFromName(data1);
                            if (target == null)
                            {
                                if (!Manager.canUseCommand(player, "uninvite")) return;

                                int targetLvl = 0;
                                int targetFrac = 0;

                                var split = data1.Split('_');
                                MySqlCommand cmd = new MySqlCommand();
                                cmd.CommandText = $"SELECT fraction,fractionlvl FROM characters WHERE firstname=@firstname AND lastname=@lastname";
                                cmd.Parameters.AddWithValue("@firstname", split[0]);
                                cmd.Parameters.AddWithValue("@lastname", split[1]);
                                DataTable result = MySQL.QueryRead(cmd);
                                if (result == null || result.Rows.Count == 0) return;
                                foreach (DataRow Row in result.Rows)
                                {
                                    targetFrac = Convert.ToInt32(Row["fraction"].ToString());
                                    targetLvl = Convert.ToInt32(Row["fractionlvl"].ToString());
                                }

                                if (targetFrac != Main.Players[player].FractionID)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не состоит в Вашей фракции", 3000);
                                    return;
                                }
                                if (targetLvl >= Main.Players[player].FractionLVL)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете уволить этого гражданина", 3000);
                                    return;
                                }
                                cmd = new MySqlCommand();
                                cmd.CommandText = $"UPDATE `characters` SET fraction=0,fractionlvl=0 WHERE firstname=@firstname AND lastname=@lastname";
                                cmd.Parameters.AddWithValue("@firstname", split[0]);
                                cmd.Parameters.AddWithValue("@lastname", split[1]);
                                MySQL.Query(cmd);
                                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы уволили гражданина {data1} из Вашей фракции", 3000);

                                int index = Fractions.Manager.AllMembers.FindIndex(m => m.Name == data1);
                                if (index > -1) Manager.AllMembers.RemoveAt(index);
                                return;
                            }
                            else
                                FractionCommands.UnInviteFromFraction(player, target);
                        }
                        break;
                    case 4: //change
                        if (!Int32.TryParse(data2, out rank))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                            return;
                        }
                        if (Int32.TryParse(data1, out id))
                        {
                            Player target = Main.GetPlayerByID(id);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким ID не найден", 3000);
                                return;
                            }
                            FractionCommands.SetFracRank(player, target, rank);
                        }
                        else
                        {
                            Player target = NAPI.Player.GetPlayerFromName(data1);
                            if (target == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин с таким именем не найден", 3000);
                                return;
                            }
                            FractionCommands.SetFracRank(player, target, rank);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"FRACMENU\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        [RemoteEvent("SaveRanks")]
        public static void SaveRanks(Player player, string data)
        {
            try
            {
                if (!isLeader(player, Main.Players[player].FractionID))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Для изменения ранга вы должны быть лидером", 3000);
                    return;
                }

                List<string> Names = JsonConvert.DeserializeObject<List<string>>(data);

                Dictionary<int, Tuple<string, string, string, int>> OLDRanks = Configs.FractionRanks[Main.Players[player].FractionID];

                Dictionary<int, Tuple<string, string, string, int>> NEWRanks = new Dictionary<int, Tuple<string, string, string, int>>();

                foreach (var ranki in OLDRanks)
                {
                    if (Names[ranki.Key - 1] == null) continue;
                    Log.Write($"NEW RANKS: {ranki.Key - 1} | {Names[ranki.Key - 1]}");
                    Tuple<string, string, string, int> old = ranki.Value;
                    Tuple<string, string, string, int> newR = new Tuple<string, string, string, int>(Names[ranki.Key - 1], old.Item2, old.Item3, old.Item4);
                    NEWRanks.Add(ranki.Key - 1, newR);
                    MySQL.Query($"UPDATE `fractionranks` SET `name`='{Names[ranki.Key - 1]}' WHERE `fraction`='{Main.Players[player].FractionID}' AND `rank`='{ranki.Key}'");
                }
                var result = MySQL.QueryRead($"SELECT * FROM `fractionranks` WHERE `fraction`='{Main.Players[player].FractionID}'");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow Row in result.Rows)
                {
                    var fraction = Convert.ToInt32(Row["fraction"]);
                    var rank = Convert.ToInt32(Row["rank"]);
                    var payday = Convert.ToInt32(Row["payday"]);
                    var name = Row["name"].ToString();
                    var clothesm = Row["clothesm"].ToString();
                    var clothesf = Row["clothesf"].ToString();

                    Configs.FractionRanks[fraction].Remove(rank);
                    Configs.FractionRanks[fraction].Add(rank, new Tuple<string, string, string, int>(name, clothesm, clothesf, payday));
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно сохранили ранги", 3000);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"SaveRanks\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
    }

    class Stocks : Script
    {
        private static nLog Log = new nLog("Stocks");

        public static Dictionary<int, FractionStock> fracStocks = new Dictionary<int, FractionStock>();
        private static Dictionary<int, Vector3> stockCoords = new Dictionary<int, Vector3>()
        {
            { 1, new Vector3(-26.2269, -1412.821, 28.5051)},
            { 2, new Vector3(114.1609, -1993.219, 17.3004)},
            { 3, new Vector3(809.462, -2310.786, 29.4647)},
            { 4, new Vector3(1373.534, -2095.765, 47.6042)},
            { 5, new Vector3(973.2294, -1844.04, 25.4073)},
            { 6, new Vector3()},
            { 7, new Vector3(472.28717, -1006.0978, 29.58738)},
            { 8, new Vector3()},
            { 9, new Vector3()},
            { 10, new Vector3(-1518.0305, 112.15948, 49.3)},
            { 11, new Vector3(-84.87458, 997.0488, 233.63676)}, // 234.63676
            { 12, new Vector3(-1520.195, 122.9179, -48.93244)},//48.93244
            { 13, new Vector3(-1869.83, 2059.325, 134.4344)},
            { 14, new Vector3()},
            { 15, new Vector3(-583.8796, -934.4156, 27.03708)},
            { 16, new Vector3(944.8635, 6.0115, 74.7413)},
            { 17, new Vector3(4998.112, -5745.9897, -13.720463)},  //поставить -62.9019
            { 18, new Vector3(-133.5834, -643.4975, -167.7005)},
			{ 19, new Vector3()},
        };
        private static Dictionary<int, Vector3> garageCoords = new Dictionary<int, Vector3>()
        {
            { 1, new Vector3(-32.7405, -1418.181, 28.3183)},
            { 2, new Vector3(117.9307, -1987.882, 17.3041)},
            { 3, new Vector3(845.1365, -2362.966, 29.3461)},
            { 4, new Vector3(1357.964, -2095.357, 50.9985)},
            { 5, new Vector3(984.9295, -1844.417, 30.0735)},
            { 6, new Vector3(-580.291, -130.4515, 34.00952)},
            { 7, new Vector3(479.2215, -1024.153, 26.61038)},
            { 8, new Vector3(324.46582, -545.86896, 27.624048)},
            { 9, new Vector3(179.1792, -695.0984, 31.1256)},
            { 10, new Vector3(-1524.504, 82.08303, 54.462765)},
            { 11, new Vector3(-130.57677, 1005.58484, 233.6121)}, //235.6121
            { 12, new Vector3(-1523.346, 97.01775, -55.54241)}, //55.54241
            { 13, new Vector3(-1923.32, 2035.93, 139.73)},
            { 14, new Vector3(-2455.718, 2984.414, 31.81033)},
            { 15, new Vector3()},
            { 16, new Vector3(977.316, -133.0648, 72.66093)},
            { 17, new Vector3(175.78754, -3325.2566, -4.6541348)}, //убрать минус
            { 18, new Vector3(-114.8835, -648.5388, -34.93016)},			// TODO:
			{ 19, new Vector3(1906.2898, 4926.8765, -47.793625)}, // Farm
        };
        public static Dictionary<string, int> maxMats = new Dictionary<string, int>()
        {
            { "", 300 },
            { "BARRACKS", 5000 },
            { "BURRITO", 1500 },
            { "YOUGA", 1500 },
            { "YOUGA2", 1500 },
            { "RUMPO3", 1500 },
        };

        [ServerEvent(Event.ResourceStart)]
        public void fillStocks()
        {
            try
            {
                var result = MySQL.QueryRead("SELECT * FROM fractions");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("Table 'fractions' returns null result", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    var data = new FractionStock();
                    data.Drugs = Convert.ToInt32(Row["drugs"]);
                    data.Money = Convert.ToInt32(Row["money"]);
                    data.Materials = Convert.ToInt32(Row["mats"]);
                    data.Medkits = Convert.ToInt32(Row["medkits"]);
                    data.Armor = Convert.ToInt32(Row["armor"]);
                    data.Koko = Convert.ToInt32(Row["koko"]);
					data.Product = Convert.ToInt32(Row["product"]);
                    data.Weapons = JsonConvert.DeserializeObject<List<nItem>>(Row["weapons"].ToString());
                    data.IsOpen = Convert.ToBoolean(Row["isopen"]);
                    data.FuelLimit = Convert.ToInt32(Row["fuellimit"]);
                    data.FuelLeft = Convert.ToInt32(Row["fuelleft"]);
                    var id = Convert.ToInt32(Row["id"]);
                    Weapons.FractionsLastSerial[id] = Convert.ToInt32(Row["lastserial"]);

                    #region label Creating
                    if (garageCoords.ContainsKey(id))
                    {
                        data.label = NAPI.TextLabel.CreateTextLabel("~b~", garageCoords[id] + new Vector3(0, 0, 1.5), 10f, 0.4f, 0, new Color(255, 255, 255), true);
                        if (id == 14) data.maxMats = 250000;
                        else data.maxMats = 100000;
                        data.UpdateLabel();
                    }

                    #endregion
                    fracStocks.Add(id, data);

                    var colshape = NAPI.ColShape.CreateCylinderColShape(stockCoords[id], 1, 2, 0); // stock colshape
                    colshape.SetData("FRACID", id);
                    colshape.OnEntityEnterColShape += enterStockShape;
                    colshape.OnEntityExitColShape += exitStockShape;
                    NAPI.Marker.CreateMarker(1, stockCoords[id] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1f, new Color(255, 255, 255, 220));
                    NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"~b~Склад {Manager.getName(id)}"), new Vector3(stockCoords[id].X, stockCoords[id].Y, stockCoords[id].Z + 0.6), 5F, 0.5F, 0, new Color(255, 255, 255));

                    colshape = NAPI.ColShape.CreateCylinderColShape(garageCoords[id], 5, 8, 0); // garage colshape
                    colshape.SetData("FRACID", id);
                    colshape.OnEntityEnterColShape += enterGarageShape;
                    colshape.OnEntityExitColShape += exitGarageShape;
                    NAPI.Marker.CreateMarker(1, garageCoords[id], new Vector3(), new Vector3(), 3f, new Color(155, 0, 0));
                }
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        #region stocks colshape
        private static void enterGarageShape(ColShape shape, Player entity)
        {
            try
            {
                entity.SetData("INTERACTIONCHECK", 32);
                entity.SetData("ONFRACSTOCK", shape.GetData<int>("FRACID"));
                entity.TriggerEvent("interactHint", true);
            }
            catch (Exception ex) { Log.Write("enterGarageShape: " + ex.Message, nLog.Type.Error); }
        }

        private static void exitGarageShape(ColShape shape, Player entity)
        {
            try
            {
                entity.SetData("INTERACTIONCHECK", 0);
                entity.SetData("ONFRACSTOCK", 0);
                entity.TriggerEvent("interactHint", false);
            }
            catch (Exception ex) { Log.Write("exitGarageShape: " + ex.Message, nLog.Type.Error); }
        }

        private static void enterStockShape(ColShape shape, Player entity)
        {
            try
            {
                entity.SetData("INTERACTIONCHECK", 33);
                entity.SetData("ONFRACSTOCK", shape.GetData<int>("FRACID"));
                entity.TriggerEvent("interactHint", true);
            }
            catch (Exception ex) { Log.Write("enterStockShape: " + ex.Message, nLog.Type.Error); }
        }

        private static void exitStockShape(ColShape shape, Player entity)
        {
            try
            {
                entity.SetData("INTERACTIONCHECK", 0);
                entity.SetData("ONFRACSTOCK", 0);
                entity.TriggerEvent("interactHint", false);
            }
            catch (Exception ex) { Log.Write("exitStockShape: " + ex.Message, nLog.Type.Error); }
        }
        #endregion

        public static int TryAdd(int fraction, nItem item)
        {
            List<nItem> items = fracStocks[fraction].Weapons;

            var tail = 0;
            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type))
            {
                if (items.Count >= 2000) return -1;
            }
            else
            {
                var count = 0;
                foreach (var i in items)
                    if (i.Type == item.Type) count += nInventory.InventoryItems.Find(x => x.ItemType == i.Type).Stacks - i.Count;

                var slots = 2000;
                var maxCapacity = (slots - items.Count) * nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Stacks + count;
                if (item.Count > maxCapacity) tail = item.Count - maxCapacity;
            }
            return tail;
        }
        public static void Add(int fraction, nItem item)
        {
            List<nItem> items = fracStocks[fraction].Weapons;

            if (nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type))
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

            fracStocks[fraction].Weapons = items;
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 6 && p.HasData("ONFRACSTOCK") && p.GetData<int>("ONFRACSTOCK") == fraction) InvInterface.OpenOut(p, fracStocks[fraction].Weapons, "Склад оружия", 6);
            }
        }
        public static void Remove(int fraction, nItem item)
        {
            List<nItem> items = fracStocks[fraction].Weapons;

            if (nInventory.ClothesItems.Contains(item.Type) || nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type)
                || item.Type == ItemType.BagWithDrill || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || nInventory.IgnoreItems.Contains(item.Type))
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

            fracStocks[fraction].Weapons = items;
            foreach (var p in NAPI.Pools.GetAllPlayers())
            {
                if (p == null || !Main.Players.ContainsKey(p)) continue;
                if (p.HasData("OPENOUT_TYPE") && p.GetData<int>("OPENOUT_TYPE") == 6 && p.HasData("ONFRACSTOCK") && p.GetData<int>("ONFRACSTOCK") == fraction) InvInterface.OpenOut(p, fracStocks[fraction].Weapons, "Склад оружия", 6);
            }
        }
        public static int GetCountOfType(int fraction, ItemType type)
        {
            List<nItem> items = fracStocks[fraction].Weapons;
            var count = 0;

            for (int i = 0; i < items.Count; i++)
            {
                if (i >= items.Count) break;
                if (items[i].Type == type) count += items[i].Count;
            }

            return count;
        }
        public static void inputStocks(Player player, int where, string action, int amount)
        {
            // where (0 - stock, 1 - garage)
            if (where == 0)
            {
                switch (action)
                {
                    case "put_stock":
                        var item = player.GetData<string>("selectedStock");
                        var data = fracStocks[Main.Players[player].FractionID];
                        int stockContains = 0;
                        int playerHave = 0;

                        if (item == "mats")
                        {
                            stockContains = data.Materials;
                            var maxstock = 100000;
                            if (Main.Players[player].FractionID == 14) maxstock = 250000;
                            if (stockContains + amount > maxstock)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад не может вместить такое кол-во материала", 3000);
                                return;
                            }
                            playerHave = (nInventory.Find(Main.Players[player].UUID, ItemType.Material) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Material).Count;
                        }
                        else if (item == "drugs")
                        {
                            stockContains = data.Drugs;
                            if (stockContains + amount > 10000)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад не может вместить такое кол-во наркотиков", 3000);
                                return;
                            }
                            playerHave = (nInventory.Find(Main.Players[player].UUID, ItemType.Drugs) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Drugs).Count;
                        }
                        else if (item == "money")
                        {
                            stockContains = data.Money;
                            playerHave = (int)Main.Players[player].Money;
                        }
                        else if (item == "medkits")
                        {
                            stockContains = data.Medkits;
                            var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                            if (invitem == null) playerHave = 0;
                            else playerHave += invitem.Count;
                        }
                        else if (item == "armor")
                        {
                            stockContains = data.Armor;
                            var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
                            if (invitem == null) playerHave = 0;
                            else playerHave += invitem.Count;
                        }
                        else if (item == "koko")
                        {
                            int fracid = Main.Players[player].FractionID;
                            if (Fractions.Manager.FractionTypes[fracid] != 1 || fracid != 12)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вам нельзя хранить листья на складе", 3000);
                                return;
                            }
                            stockContains = data.Koko;
                            var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.Kokos);
                            if (invitem == null) playerHave = 0;
                            else playerHave += invitem.Count;
                        }

                        if (playerHave < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет столько", 3000);
                            return;
                        }

                        if (item == "mats")
                        {
                            data.Materials += amount;
                            nInventory.Remove(player, ItemType.Material, amount);
                        }
                        else if (item == "drugs")
                        {
                            data.Drugs += amount;
                            nInventory.Remove(player, ItemType.Drugs, amount);
                        }
                        else if (item == "money")
                        {
                            data.Money += amount;
                            MoneySystem.Wallet.Change(player, -amount);
                            GameLog.Money($"player({Main.Players[player].UUID})", $"frac({Main.Players[player].FractionID})", amount, $"putStock");
                        }
                        else if (item == "medkits")
                        {
                            data.Medkits += amount;
                            nInventory.Remove(player, ItemType.HealthKit, amount);
                        }
                        else if (item == "armor")
                        {
                            data.Armor += amount;
                            nInventory.Remove(player, ItemType.BodyArmor, amount);
                        }
                        else if (item == "koko")
                        {
                            data.Koko += amount;
                            nInventory.Remove(player, ItemType.Kokos, amount);
                        }

                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, item, amount, true);
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"На складе осталось {stockContains + amount}, у Вас {playerHave - amount}", 3000);
                        data.UpdateLabel();
                        break;
                    case "take_stock":
                        item = player.GetData<string>("selectedStock");
                        if (!Manager.canUseCommand(player, $"take{item}")) return;

                        data = fracStocks[Main.Players[player].FractionID];
                        stockContains = 0;
                        playerHave = 0;
                        if (item == "mats")
                        {
                            stockContains = data.Materials;
                            playerHave = (nInventory.Find(Main.Players[player].UUID, ItemType.Material) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Material).Count;
                            if (playerHave + amount > 500)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете вместить столько в свой инвентарь", 3000);
                                return;
                            }
                        }
                        else if (item == "drugs")
                        {
                            stockContains = data.Drugs;
                            playerHave = (nInventory.Find(Main.Players[player].UUID, ItemType.Drugs) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Drugs).Count;
                            if (playerHave + amount > 50)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете вместить столько в свой инвентарь", 3000);
                                return;
                            }
                        }
                        else if (item == "money")
                        {
                            stockContains = data.Money;
                            playerHave = (int)Main.Players[player].Money;
                        }
                        else if (item == "medkits")
                        {
                            stockContains = data.Medkits;
                            var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                            if (invitem == null) playerHave = 0;
                            else playerHave += invitem.Count;
                        }
                        else if (item == "armor")
                        {
                            stockContains = data.Armor;
                            var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
                            if (invitem == null) playerHave = 0;
                            else playerHave += invitem.Count;
                        }
                        else if (item == "koko")
                        {
                            int fracid = Main.Players[player].FractionID;
                            if (Fractions.Manager.FractionTypes[fracid] != 1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вам нельзя хранить листья на складе", 3000);
                                return;
                            }
                            stockContains = data.Koko;
                            var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.Kokos);
                            if (invitem == null) playerHave = 0;
                            else playerHave += invitem.Count;
                        }

                        if (stockContains < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На складе столько нет", 3000);
                            return;
                        }

                        if (item == "mats")
                        {
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Material, amount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            data.Materials -= amount;
                            nInventory.Add(player, new nItem(ItemType.Material, amount));
                        }
                        else if (item == "drugs")
                        {
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Drugs, amount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            data.Drugs -= amount;
                            nInventory.Add(player, new nItem(ItemType.Drugs, amount));
                        }
                        else if (item == "money")
                        {
                            data.Money -= amount;
                            MoneySystem.Wallet.Change(player, amount);
                            GameLog.Money($"frac({Main.Players[player].FractionID})", $"player({Main.Players[player].UUID})", amount, $"takeStock");
                        }
                        else if (item == "medkits")
                        {
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.HealthKit, amount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            data.Medkits -= amount;
                            nInventory.Add(player, new nItem(ItemType.HealthKit, amount));
                        }
                        else if (item == "armor")
                        {
                            var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
                            if (invitem != null && invitem.Count >= 1)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.BodyArmor, amount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            data.Armor -= amount;
                            nInventory.Add(player, new nItem(ItemType.BodyArmor, amount, "100"));
                        }
                        else if (item == "koko")
                        {
                            int fracid = Main.Players[player].FractionID;
                            if (Fractions.Manager.FractionTypes[fracid] != 1  || fracid != 12)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вам нельзя хранить листья на складе", 3000);
                                return;
                            }
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Kokos, amount));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            data.Koko -= amount;
                            nInventory.Add(player, new nItem(ItemType.Kokos, amount));
                        }

                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, item, amount, false);
                        Manager.FracLogs[Main.Players[player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "" )}{DateTime.Now.Minute}", player.Name, item, amount });

                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"На складе осталось {stockContains - amount}, у Вас {playerHave + amount}", 3000);
                        data.UpdateLabel();
                        break;
                }
            }
            else
            {
                if (!player.IsInVehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                    return;
                }
                var vehicle = player.Vehicle;
                if (!vehicle.HasData("CANMATS") && !vehicle.HasData("CANDRUGS") && !vehicle.HasData("CANMEDKITS"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Машина не может ничего перевозить", 3000);
                    return;
                }
                int onfrac = player.GetData<int>("ONFRACSTOCK");
                switch (action)
                {
                    case "load_mats":
                        if (onfrac != 14 && Main.Players[player].FractionID != onfrac)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в {Fractions.Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                            return;
                        }
                        if (onfrac != 14 && !Manager.canUseCommand(player, "takestock")) return;
                        if (fracStocks[onfrac].Materials < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На складе нет такого кол-ва матов", 3000);
                            return;
                        }
                        var maxMats = (Fractions.Stocks.maxMats.ContainsKey(vehicle.DisplayName)) ? Stocks.maxMats[vehicle.DisplayName] : 600;
                        if (VehicleInventory.GetCountOfType(vehicle, ItemType.Material) + amount > maxMats)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно загрузить такое кол-во матов", 3000);
                            return;
                        }
                        var tryAdd = VehicleInventory.TryAdd(vehicle, new nItem(ItemType.Material, amount));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно загрузить такое кол-во матов", 3000);
                            return;
                        }
                        var data = new nItem(ItemType.Material);
                        data.Count = amount;
                        VehicleInventory.Add(vehicle, data);
                        fracStocks[onfrac].Materials -= amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы загрузили материалы в машину", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "mats", amount, false);
                        return;
                    case "unload_mats":
                        var count = VehicleInventory.GetCountOfType(vehicle, ItemType.Material);
                        if (count < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине нет такого кол-ва матов", 3000);
                            return;
                        }
                        var maxstock = 100000;
                        if (onfrac == 14) maxstock = 250000;
                        if (fracStocks[onfrac].Materials + amount > maxstock)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад не может вместить такое кол-во материала", 3000);
                            return;
                        }
                        VehicleInventory.Remove(vehicle, ItemType.Material, amount);
                        fracStocks[onfrac].Materials += amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выгрузили материалы из машины", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "mats", amount, true);
                        return;
                    case "load_drugs":
                        if (Main.Players[player].FractionID != onfrac)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в ~y~{Fractions.Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                            return;
                        }
                        if (!Manager.canUseCommand(player, "takestock")) return;
                        if (fracStocks[onfrac].Drugs < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На складе нет такого кол-ва наркотиков", 3000);
                            return;
                        }
                        tryAdd = VehicleInventory.TryAdd(vehicle, new nItem(ItemType.Drugs, amount));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно загрузить такое кол-во наркотиков", 3000);
                            return;
                        }
                        data = new nItem(ItemType.Drugs);
                        data.Count = amount;
                        VehicleInventory.Add(vehicle, data);
                        fracStocks[onfrac].Drugs -= amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы загрузили наркотики в машину", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "drugs", amount, false);
                        return;
                    case "unload_drugs":
                        count = VehicleInventory.GetCountOfType(vehicle, ItemType.Drugs);
                        if (count < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине нет такого кол-ва наркотиков", 3000);
                            return;
                        }
                        if (fracStocks[onfrac].Drugs + amount > 10000)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад не может вместить такое кол-во наркотиков", 3000);
                            return;
                        }
                        VehicleInventory.Remove(vehicle, ItemType.Drugs, amount);
                        fracStocks[onfrac].Drugs += amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выгрузили наркотики из машины", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "drugs", amount, true);
                        return;
                    case "load_medkits":
                        if (Main.Players[player].FractionID != onfrac)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в {Fractions.Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                            return;
                        }
                        if (!player.GetData<bool>("ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                            return;
                        }
                        if (fracStocks[onfrac].Medkits < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На складе нет такого кол-ва аптечек", 3000);
                            return;
                        }
                        var maxMedkits = 100;
                        if (VehicleInventory.GetCountOfType(vehicle, ItemType.HealthKit) + amount > maxMedkits)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно загрузить такое кол-во аптечек", 3000);
                            return;
                        }
                        tryAdd = VehicleInventory.TryAdd(vehicle, new nItem(ItemType.HealthKit, amount));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно загрузить такое кол-во аптечек", 3000);
                            return;
                        }
                        VehicleInventory.Add(vehicle, new nItem(ItemType.HealthKit, amount));
                        fracStocks[onfrac].Medkits -= amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы загрузили аптечки в машину", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "medkits", amount, false);
                        return;
                    case "unload_medkits":
                        if (!player.GetData<bool>("ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                            return;
                        }
                        count = VehicleInventory.GetCountOfType(vehicle, ItemType.HealthKit);
                        if (count < amount)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине нет такого кол-ва аптечек", 3000);
                            return;
                        }
                        if (fracStocks[onfrac].Medkits + amount > 1000)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад не может вместить такое кол-во аптечек", 3000);
                            return;
                        }
                        VehicleInventory.Remove(vehicle, ItemType.HealthKit, amount);
                        fracStocks[onfrac].Medkits += amount;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выгрузили аптечки из машины", 3000);
                        fracStocks[onfrac].UpdateLabel();
                        GameLog.Stock(Main.Players[player].FractionID, Main.Players[player].UUID, "medkits", amount, true);
                        return;
                }
            }
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {
                case 32:
                    if (!player.IsInVehicle)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                        return;
                    }
                    var vehicle = player.Vehicle;
                    if (!vehicle.HasData("CANMATS") && !vehicle.HasData("CANDRUGS") && !vehicle.HasData("CANMEDKITS"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Машина не может ничего перевозить", 3000);
                        return;
                    }

                    if (player.GetData<int>("ONFRACSTOCK") == 14 && (DateTime.Now.Hour < 11 && DateTime.Now.Hour > 23))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад намертво закрыт", 3000);
                        return;
                    }
                    else if (!fracStocks[(int)player.GetData<int>("ONFRACSTOCK")].IsOpen && player.GetData<int>("ONFRACSTOCK") != 14)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }

                    OpenFracGarageMenu(player);
                    return;
                case 33:
                    if (Main.Players[player].FractionID != player.GetData<int>("ONFRACSTOCK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в {Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                        return;
                    }
                    if (!fracStocks[Main.Players[player].FractionID].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    OpenFracStockMenu(player);
                    return;
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public static void onPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            try
            {
                /*var menu = MenuController.MenuManager.getMenu(player);
                if (menu != null && menu.Id == "fracGarage") MenuConstructor.CloseAll(player);*/
            }
            catch (Exception e) { Log.Write("PlayerExit: " + e.Message, nLog.Type.Error); }
        }

        public static void saveStocksDic()
        {
            foreach (var key in fracStocks.Keys)
            {
                var data = fracStocks[key];
                MySQL.Query($"UPDATE fractions SET drugs={data.Drugs},money={data.Money},mats={data.Materials},armor={data.Armor},koko={data.Koko},medkits={data.Medkits},lastserial={Weapons.FractionsLastSerial[key]}," +
                    $"weapons='{JsonConvert.SerializeObject(data.Weapons)}',isopen={data.IsOpen},fuellimit={data.FuelLimit},fuelleft={data.FuelLeft} WHERE id={key}");
            }
        }

        [RemoteEvent("openWeaponStock")]
        public static void Event_openWeaponsStock(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.HasData("ONFRACSTOCK") || player.GetData<int>("ONFRACSTOCK") == 0) return;
                if (Main.Players[player].FractionID != player.GetData<int>("ONFRACSTOCK")) return;

                if (!Manager.canUseCommand(player, "openweaponstock")) return;

                InvInterface.OpenOut(player, fracStocks[(int)player.GetData<int>("ONFRACSTOCK")].Weapons, "Склад оружия", 6);
            }
            catch (Exception e) { Log.Write("Openweaponstock: " + e.Message, nLog.Type.Error); }
        }

        #region menus
        public static void OpenFracGarageMenu(Player player)
        {
            bool isArmy = !player.Vehicle.HasData("CANDRUGS");
            bool isMed = player.Vehicle.HasData("CANMEDKITS");
            Trigger.PlayerEvent(player, "matsOpen", isArmy, isMed);
        }
        public static void fracgarage(Player player, string eventName, string data)
        {
            int amount = 0;
            if (!Int32.TryParse(data, out amount))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                return;
            }
            if (amount < 1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Введите корректные данные", 3000);
                return;
            }
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                return;
            }
            var vehicle = player.Vehicle;
            if (!vehicle.HasData("CANMATS") && !vehicle.HasData("CANDRUGS") && !vehicle.HasData("CANMEDKITS"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Машина не может ничего перевозить", 3000);
                return;
            }
            switch (eventName)
            {
                case "loadmats":
                    if (player.GetData<int>("ONFRACSTOCK") != 14 && Main.Players[player].FractionID != player.GetData<int>("ONFRACSTOCK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в {Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                        return;
                    }
                    //Main.OpenInputMenu(player, "Введите кол-во материалов", "load_mats");
                    Fractions.Stocks.inputStocks(player, 1, "load_mats", amount);
                    return;
                case "unloadmats":
                    //Main.OpenInputMenu(player, "Введите кол-во материалов", "unload_mats");
                    Fractions.Stocks.inputStocks(player, 1, "unload_mats", amount);
                    return;
                case "loaddrugs":
                    if (!vehicle.HasData("CANDRUGS"))
                    {
                        MenuManager.Close(player);
                        return;
                    }
                    if (Main.Players[player].FractionID != player.GetData<int>("ONFRACSTOCK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в {Manager.getName(player.GetData<int>("ONFRACSTOCK"))}", 3000);
                        return;
                    }
                    //Main.OpenInputMenu(player, "Введите кол-во наркотиков", "load_drugs");
                    Fractions.Stocks.inputStocks(player, 1, "load_drugs", amount);
                    return;
                case "unloaddrugs":
                    //Main.OpenInputMenu(player, "Введите кол-во наркотиков", "unload_drugs");
                    Fractions.Stocks.inputStocks(player, 1, "unload_drugs", amount);
                    return;
                case "loadmedkits":
                    Fractions.Stocks.inputStocks(player, 1, "load_medkits", amount);
                    return;
                case "unloadmedkits":
                    Fractions.Stocks.inputStocks(player, 1, "unload_medkits", amount);
                    return;
            }
        }

        public static void OpenFracStockMenu(Player player)
        {
            List<int> counter = new List<int>
            {
                fracStocks[Main.Players[player].FractionID].Money,
                fracStocks[Main.Players[player].FractionID].Medkits,
                fracStocks[Main.Players[player].FractionID].Drugs,
                fracStocks[Main.Players[player].FractionID].Materials,
                fracStocks[Main.Players[player].FractionID].Armor,
                fracStocks[Main.Players[player].FractionID].Koko
            };
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(counter);
            Log.Debug(json);
            Trigger.PlayerEvent(player, "openStock", json);
        }
        private static void callback_fracstock(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "mats":
                case "drugs":
                case "money":
                case "armor":
                case "koko":
                case "medkits":
                    MenuManager.Close(player);
                    OpenStockSelectMenu(player, item.ID);
                    return;
                case "close":
                    MenuManager.Close(player);
                    return;
            }
        }

        public static void OpenStockSelectMenu(Player player, string item)
        {
            player.SetData("selectedStock", item);
            string itemcount = "";
            string menuname = "";
            if (item == "mats")
            {
                var count = (nInventory.Find(Main.Players[player].UUID, ItemType.Material) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Material).Count;
                itemcount += count + " матов";
                menuname = "Материалы";
            }
            else if (item == "drugs")
            {
                var count = (nInventory.Find(Main.Players[player].UUID, ItemType.Drugs) == null) ? 0 : nInventory.Find(Main.Players[player].UUID, ItemType.Drugs).Count;
                itemcount += count + "г";
                menuname = "Наркотики";
            }
            else if (item == "money")
            {
                itemcount += Main.Players[player].Money + "$";
                menuname = "Деньги";
            }
            else if (item == "medkits")
            {
                var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.HealthKit);
                if (invitem == null) itemcount += "0 шт";
                else itemcount += invitem.Count + " шт";
                menuname = "Аптечки";
            }
            else if (item == "armor")
            {
                var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.BodyArmor);
                if (invitem == null) itemcount += "0 шт";
                else itemcount += invitem.Count + " шт";
                menuname = "Бронежилеты";
            }
            else if (item == "koko")
            {
                var invitem = nInventory.Find(Main.Players[player].UUID, ItemType.Kokos);
                if (invitem == null) itemcount += "0 шт";
                else itemcount += invitem.Count + " шт";
                menuname = "Бронежилеты";
            }
            Menu menu = new Menu("stockselect", false, false);
            menu.Callback = callback_stockselect;

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = menuname;
            menu.Add(menuItem);

            menuItem = new Menu.Item("uhave", Menu.MenuItem.Card);
            menuItem.Text = $"У Вас есть {itemcount}";
            menu.Add(menuItem);

            menuItem = new Menu.Item("put", Menu.MenuItem.Button);
            menuItem.Text = "Положить";
            menu.Add(menuItem);

            menuItem = new Menu.Item("take", Menu.MenuItem.Button);
            menuItem.Text = "Взять";
            menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.Button);
            menuItem.Text = "Назад";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_stockselect(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            switch (item.ID)
            {
                case "put":
                    MenuManager.Close(player);
                    Main.OpenInputMenu(player, "Введите кол-во", "put_stock");
                    return;
                case "take":
                    MenuManager.Close(player);
                    Main.OpenInputMenu(player, "Введите кол-во", "take_stock");
                    return;
                case "back":
                    MenuManager.Close(player);
                    OpenFracStockMenu(player);
                    return;
            }
        }
        #endregion

        public class FractionStock
        {
            public int Drugs { get; set; }
            public int Money { get; set; }
            public int Materials { get; set; }
            public int Medkits { get; set; }
            public int Armor { get; set; }
            public int Koko { get; set; }
			public int Product { get; set; }
            public List<nItem> Weapons { get; set; }
            public bool IsOpen { get; set; }
            [JsonIgnore]
            public int maxMats { get; set; }
            [JsonIgnore]
            public TextLabel label { get; set; }
            public int FuelLimit { get; set; }
            public int FuelLeft { get; set; }

            public void UpdateLabel()
            {
                if (label == null) return;
                var text = $"~b~";
                if (Materials > 0) text += $"Материалы: ~w~{Materials}/{maxMats}\n";
                label.Text = text;
            }
        }
    }

    class MatsWar : Script
    {
        private static API api = new API();
        public static bool isWar = false;
        public static int matsLeft = 15000;
        private static Marker warMarker = null;
        private static Vector3 warPosition = new Vector3(33.33279, -2669.874, 5.008363);
        private static Blip warblip;
        private static string startWarTimer = null;

        private static nLog Log = new nLog("MatsWar");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var col = NAPI.ColShape.CreateCylinderColShape(warPosition, 6, 2, 0);
                col.OnEntityEnterColShape += onEntityEnterColShape;
                col.OnEntityExitColShape += onEntityExitColShape;

                warblip = NAPI.Blip.CreateBlip(478, warPosition, 1, 40, Main.StringToU16("Война за маты"), 255, 0, true, 0, 0);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void startMatWarTimer()
        {
            Manager.sendFractionMessage(1, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            Manager.sendFractionMessage(2, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            Manager.sendFractionMessage(3, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            Manager.sendFractionMessage(4, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            Manager.sendFractionMessage(5, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            Manager.sendFractionMessage(10, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            Manager.sendFractionMessage(11, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            Manager.sendFractionMessage(12, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            Manager.sendFractionMessage(13, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            Manager.sendFractionMessage(17, "Через 10 минут в порт Los-Santos'а прибудет корабль с материалами.", true);
            //startWarTimer = Main.StartT(600000, 99999999, (o) => startWar(), "STARTMATWAR_TIMER");
            startWarTimer = Timers.StartOnce(5000, () => startWar());
        }

        public static void startWar()
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (isWar) return;
                    matsLeft = 15000;
                    warMarker = NAPI.Marker.CreateMarker(1, warPosition - new Vector3(0, 0, 5), new Vector3(), new Vector3(), 6f, new Color(155, 0, 0, 255));
                    isWar = true;
                    Manager.sendFractionMessage(1, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    Manager.sendFractionMessage(2, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    Manager.sendFractionMessage(3, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    Manager.sendFractionMessage(4, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    Manager.sendFractionMessage(5, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    Manager.sendFractionMessage(10, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    Manager.sendFractionMessage(11, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    Manager.sendFractionMessage(12, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    Manager.sendFractionMessage(13, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    Manager.sendFractionMessage(17, "Корабль с материалами прибыл в порт Los-Santos'a.", true);
                    warblip.Color = 49;
                    //Main.StopT(startWarTimer, "timer_11");
                }
                catch { }
            });
        }

        public static void endWar()
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    NAPI.Entity.DeleteEntity(warMarker);
                    isWar = false;
                    Manager.sendFractionMessage(1, "Корабль ушел из порта Los-Santos'a.", true);
                    Manager.sendFractionMessage(2, "Корабль ушел из порта Los-Santos'a.", true);
                    Manager.sendFractionMessage(3, "Корабль ушел из порта Los-Santos'a.", true);
                    Manager.sendFractionMessage(4, "Корабль ушел из порта Los-Santos'a.", true);
                    Manager.sendFractionMessage(5, "Корабль ушел из порта Los-Santos'a.", true);
                    Manager.sendFractionMessage(10, "Корабль ушел из порта Los-Santos'a.", true);
                    Manager.sendFractionMessage(11, "Корабль ушел из порта Los-Santos'a.", true);
                    Manager.sendFractionMessage(12, "Корабль ушел из порта Los-Santos'a.", true);
                    Manager.sendFractionMessage(13, "Корабль ушел из порта Los-Santos'a.", true);
                    Manager.sendFractionMessage(17, "Корабль ушел из порта Los-Santos'a.", true);
                    warblip.Color = 49;
                });
            }
            catch (Exception e) { Log.Write($"EndMatsWar: " + e.Message, nLog.Type.Error); }
        }

        public static void interact(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            var fracid = Main.Players[player].FractionID;
            if (!((fracid >= 1 && fracid <= 5) || (fracid >= 10 && fracid <= 13) || (fracid == 17)))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете сделать это", 3000);
                return;
            }
            if (!player.IsInVehicle)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                return;
            }
            if (!player.Vehicle.HasData("CANMATS"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На этой машине нельзя перевозить маты", 3000);
                return;
            }
            if (player.HasData("loadMatsTimer"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже загружаете материалы в машину", 3000);
                return;
            }
            var count = VehicleInventory.GetCountOfType(player.Vehicle, ItemType.Material);
            if (count >= Fractions.Stocks.maxMats[player.Vehicle.DisplayName])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В машине максимальное кол-во материала", 3000);
                return;
            }
            //player.SetData("loadMatsTimer", Main.StartT(20000, 99999999, (o) => Fractions.Army.loadMaterialsTimer(player), "GMLOADMATS_TIMER"));
            player.SetData("loadMatsTimer", Timers.StartOnce(20000, () => Fractions.Army.loadMaterialsTimer(player)));
            player.Vehicle.SetData("loaderMats", player);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Загрузка материалов началась (20 секунд)", 3000);
            Trigger.PlayerEvent(player, "showLoader", "Загрузка материалов", 1);
            player.SetData("vehicleMats", player.Vehicle);
            player.SetData("whereLoad", "WAR");
            return;
        }

        private void onEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                if (!isWar) return;
                if (NAPI.Entity.GetEntityType(entity) != EntityType.Player) return;
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 37);
            }
            catch (Exception ex) { Log.Write("onEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
        }

        private void onEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) == EntityType.Player)
                {
                    NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
                    if (entity.IsInVehicle && NAPI.Data.HasEntityData(entity.Vehicle, "loaderMats"))
                    {
                        Player player = NAPI.Data.GetEntityData(entity.Vehicle, "loaderMats");
                        //Main.StopT(player.GetData("loadMatsTimer"), "timer_12");
                        Timers.Stop(player.GetData<string>("loadMatsTimer"));
                        NAPI.Data.ResetEntityData(entity.Vehicle, "loaderMats");
                        player.ResetData("loadMatsTimer");
                        Trigger.PlayerEvent(player, "hideLoader");
                        Notify.Send(player, NotifyType.Warning, NotifyPosition.BottomCenter, $"Загрузка материалов отменена, так как машина покинула чекпоинт", 3000);
                    }
                }
            }
            catch (Exception ex) { Log.Write("onEntityExitColShape: " + ex.Message, nLog.Type.Error); }
        }
    }
}

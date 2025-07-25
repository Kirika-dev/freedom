﻿using System;
using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using System.Data;
using System.Linq;
using MySqlConnector;

namespace NeptuneEVO.Fractions
{
    class LSNews : Script
    {
        private static nLog Log = new nLog("News");

        private class Advert
        {
            public int ID { get; set; }
            public string Author { get; set; }
            public int AuthorSIM { get; set; }
            public string AD { get; set; }
            public string EditedAD { get; set; }
            public string BlockedBy { get; set; }

            public DateTime OpenedDate { get; set; }
            public DateTime ClosedDate { get; set; }

            public bool Status { get; set; }

            public void Send(Player someone = null)
            {
                if (someone == null)
                {
                    foreach (Player target in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(target)) continue;
                        if (Main.Players[target].FractionID != 15) continue;

                        Trigger.PlayerEvent(target, "addadvert", ID, Author, AD);
                    }
                }
                else
                {
                    if (!Main.Players.ContainsKey(someone)) return;
                    if (Main.Players[someone].FractionID != 15) return;

                    Trigger.PlayerEvent(someone, "addadvert", ID, Author, AD);
                }
            }
        }
        private static Dictionary<int, Advert> Adverts;

        public static List<string> AdvertNames;

        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> LSNewsCoords = new List<Vector3>
        {
            new Vector3(-598.9359, -930.0089, -220.74352), // Метка над новостями - блип
            new Vector3(-584.452, -938.0066, 22.74986), // Колшэйп изнутри интерьера для телепорта наверх
        };


        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStartHandler()
        {
            try
            {

                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(LSNewsCoords[0], 1f, 2, 0));
                Cols[0].OnEntityEnterColShape += lsnews_OnEntityEnterColShape;
                Cols[0].OnEntityExitColShape += lsnews_OnEntityExitColShape;
                Cols[0].SetData("INTERACT", 80);

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(LSNewsCoords[1], 1f, 2, 0));
                Cols[1].SetData("INTERACT", 81);
                Cols[1].OnEntityEnterColShape += lsnews_OnEntityEnterColShape;
                Cols[1].OnEntityExitColShape += lsnews_OnEntityExitColShape;
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~b~Переодеться"), LSNewsCoords[1] + new Vector3(0, 0, 0.7), 5F, 0.3F, 0, new Color(255, 255, 255));

                NAPI.Marker.CreateMarker(1, LSNewsCoords[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, LSNewsCoords[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT\"FRACTIONS_LSNEWS\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private void lsnews_OnEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
            }
            catch (Exception e) { Log.Write("lsnews_OnEntityEnterColShape: " + e.Message, nLog.Type.Error); }
        }

        private void lsnews_OnEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("lsnews_OnEntityExitColShape: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {
                case 80:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    if (Main.Players[player].FractionID != 15)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не состоите в News", 3000);
                        return;
                    }
                    NAPI.Entity.SetEntityPosition(player, LSNewsCoords[1] + new Vector3(0, 0, 1.12));
                    return;
                case 81:
                    beginWorkDay(player);
                    break;

            }
        }
        //=======
        public static void Init()
        {
            try
            {
                AdvertNames = new List<string>();
                Adverts = new Dictionary<int, Advert>();

                string cmd = @"SELECT * FROM advertised;";

                DataTable result = MySQL.QueryRead(cmd);
                if (result is null) return;
                foreach (DataRow row in result.Rows)
                {
                    if (Convert.ToBoolean((sbyte)row[8]) != false) continue;

                    Advert ad = new Advert
                    {
                        ID = (int)row[0],
                        Author = row[1].ToString(),
                        AuthorSIM = (int)row[2],
                        AD = Main.BlockSymbols(row[3].ToString()),
                        BlockedBy = row[4].ToString(),
                        EditedAD = Main.BlockSymbols(row[5].ToString()),
                        OpenedDate = (DateTime)row[6],
                        ClosedDate = (DateTime)row[7],
                        Status = Convert.ToBoolean((sbyte)row[8])
                    };
                    AdvertNames.Add(ad.Author);
                    Adverts.Add((int)row[0], ad);
                }

            }
            catch (Exception e)
            {
                Log.Write("Init: " + e.ToString(), nLog.Type.Error);
            }
        }

        public static void beginWorkDay(Player player)
        {
            if (Main.Players[player].FractionID == 15)
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы переоделись в форму Новостного Агенства", 3000);
                    Manager.setSkin(player, 15, Main.Players[player].FractionLVL);
                    NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                    return;
                }
                else
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сняли форму Новостного Агенства", 3000);
                    Customization.ApplyCharacter(player);
                    if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                    else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                    NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                    return;
                }
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не состоите в Новостном Агенстве", 3000);
        }

        public static void onLSNPlayerLoad(Player Player)
        {
            try
            {
                foreach (Advert ad in Adverts.Values)
                {
                    ad.Send(Player);
                }

            }
            catch (Exception e)
            {
                Log.Write("onLSNPlayerLoad: " + e.ToString(), nLog.Type.Error);
            }
        }

        #region Remote Events
        //Админ взял репорт на себя
        [RemoteEvent("takeadvert")]
        public void AdvertTake(Player Player, int id, bool retrn = false)
        {
            try
            {
                if (Main.Players[Player].FractionID != 15) return;
                if (!Adverts.ContainsKey(id))
                {
                    Remove(id, Player);
                    return;
                }

                if (Adverts[id].Status)
                {
                    Remove(id, Player);
                    return;
                }

                foreach (Player target in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(target)) continue;
                    if (Main.Players[target].FractionID != 15) continue;

                    if (retrn) Trigger.PlayerEvent(target, "setadvert", id, "");
                    else Trigger.PlayerEvent(target, "setadvert", id, Player.Name);
                }
            }
            catch
            {
            }
        }
        [RemoteEvent("sendadvert")]
        public void AdvertSend(Player player, int ID, string answer)
        {
            try
            {
                if (Main.Players[player].FractionID != 15) return;
                if (!Adverts.ContainsKey(ID)) return;
                if (!Adverts[ID].Status)
                {
                    AddAnswer(player, ID, answer);
                }
                else
                {
                    player.SendChatMessage("Это объявление более недоступно для изменения.");
                    Remove(ID, player);
                }
            }
            catch
            {
            }
        }
        #endregion

        public static void AddAdvert(Player player, string question, int price)
        {
            try
            {
                question = Main.BlockSymbols(question);

                GameLog.Money($"bank({Main.Players[player].Bank})", $"server", price, "ad");
                player.SetData("NEXT_AD", DateTime.Now.AddMinutes(45));
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы подали объявление. Ожидайте публикации", 3000);

                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = "INSERT INTO `advertised` (`Author`,`AuthorSIM`,`AD`,`Opened`,`Closed`) VALUES (@pn,@sim,@ques,@time,@ntime); SELECT LAST_INSERT_ID();";
                cmd.Parameters.AddWithValue("@pn", player.Name);
                cmd.Parameters.AddWithValue("@sim", Main.Players[player].Sim);
                cmd.Parameters.AddWithValue("@ques", question);
                cmd.Parameters.AddWithValue("@time", MySQL.ConvertTime(DateTime.Now));
                cmd.Parameters.AddWithValue("@ntime", MySQL.ConvertTime(DateTime.MinValue));

                DataTable dt = MySQL.QueryRead(cmd);

                int id = Convert.ToInt32(dt.Rows[0][0]);
                Advert advert = new Advert
                {
                    ID = id,
                    Author = player.Name,
                    AuthorSIM = Main.Players[player].Sim,
                    AD = question,
                    BlockedBy = "",
                    EditedAD = "",
                    Status = false,
                    OpenedDate = DateTime.Now,
                    ClosedDate = DateTime.MinValue
                };
                AdvertNames.Add(advert.Author);
                advert.Send();
                Adverts.Add(id, advert);
                foreach (Player p in NAPI.Pools.GetAllPlayers())
                {
                    if (!Main.Players.ContainsKey(p)) continue;
                    if (Main.Players[p].AdminLVL >= 1) p.SendChatMessage($"~r~Объявление от {player.Name.Replace('_', ' ')} (ID {player.Value} | Объявление №{id}): {question}");
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        public static void RecordOnRealTime(Player player, string text)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (player.Position.DistanceTo2D(Manager.FractionSpawns[15]) > 50)
            {
                player.SendChatMessage("Вы слишком далеко от офиса");
                return;
            }
            if (Main.Players[player].AdminLVL == 0)
            {
                if (Main.Players[player].FractionID != 15) return;
            }
            if (string.IsNullOrEmpty(text)) return;
            NAPI.Chat.SendChatMessageToAll("!{#993600}" + $"[ПРЯМОЙ ЭФИР]" + "!{#FFFF}" + $" Ведущий <{player.Name}> {text}");
        }

        public static void AddAnswer(Player player, int repID, string response, bool deleted = false)
        {
            try
            {
                response = Main.BlockSymbols(response);

                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL == 0)
                {
                    if (Main.Players[player].FractionID != 15) return;
                }
                if (player.Position.DistanceTo2D(Manager.FractionSpawns[15]) > 50)
                {
                    player.SendChatMessage("Вы слишком далеко от офиса");
                    return;
                }
               
                if (!Adverts.ContainsKey(repID))
                {
                    if (deleted) player.SendChatMessage("Объявления с подобным номером не было найдено.");
                    return;
                }
                DateTime now = DateTime.Now;

                if (!deleted)
                {
                    try
                    {
                        int moneyad = Adverts[repID].AD.Length / 15 * 6;
                        MoneySystem.Bank.Change(Main.Players[player].Bank, (moneyad * 90 / 100), false);
                        Stocks.fracStocks[6].Money += moneyad * 10 / 100;
                        if (Adverts[repID].AuthorSIM >= 1) NAPI.Chat.SendChatMessageToAll("!{#993600}" + $"Объявление от {Adverts[repID].Author.Replace('_', ' ')}: {response} | Тел: {Adverts[repID].AuthorSIM}");
                        else NAPI.Chat.SendChatMessageToAll("!{#993600}" + $"Объявление от {Adverts[repID].Author.Replace('_', ' ')}: {response}");
                        NAPI.Chat.SendChatMessageToAll("!{#0E029C}" + $"Редактор: {player.Name.Replace('_', ' ')}.");
                    }
                    catch
                    {
                    }
                }
                else
                {
                    if (Main.Players[player].AdminLVL != 0) GameLog.Admin($"{player.Name}", $"delAd", $"{Adverts[repID].Author}");
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы удалили объявление гражданина {Adverts[repID].Author}", 3000);
                    Player target = NAPI.Player.GetPlayerFromName(Adverts[repID].Author);
                    if (target != null) Notify.Send(target, NotifyType.Error, NotifyPosition.BottomCenter, $"{player.Name} удалил Ваше объявление по причине: {response}.", 3000);
                    response += " | Удалено";
                }

                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = "UPDATE advertised SET Editor=@resp,EditedAD=@res,Status=@st,Closed=@time WHERE ID=@repid LIMIT 1";
                cmd.Parameters.AddWithValue("@resp", player.Name);
                cmd.Parameters.AddWithValue("@res", response);
                cmd.Parameters.AddWithValue("@st", true);
                cmd.Parameters.AddWithValue("@time", MySQL.ConvertTime(now));
                cmd.Parameters.AddWithValue("@repid", repID);
                MySQL.Query(cmd);

                AdvertNames.Remove(Adverts[repID].Author);

                Adverts[repID].Author = player.Name;
                Adverts[repID].EditedAD = response;
                Adverts[repID].ClosedDate = now;
                Adverts[repID].Status = true;
                Remove(repID);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }

        private static void Remove(int ID_, Player someone = null)
        {
            try
            {
                Log.Debug($"Remove {ID_}");
                if (someone == null)
                {
                    foreach (Player target in NAPI.Pools.GetAllPlayers())
                    {
                        if (!Main.Players.ContainsKey(target)) continue;
                        if (Main.Players[target].FractionID != 15) continue;

                        Trigger.PlayerEvent(target, "deladvert", ID_);
                    }
                }
                else
                {
                    if (!Main.Players.ContainsKey(someone)) return;
                    if (Main.Players[someone].FractionID != 15) return;

                    Trigger.PlayerEvent(someone, "deladvert", ID_);
                }
                Adverts.Remove(ID_);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
    }
}

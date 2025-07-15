using System;
using NeptuneEVO;
using NeptuneEVO.SDK;
using GTANetworkAPI;

namespace NeptuneEVO.Core
{
    public class Quests : Script
    {
        private static nLog Log = new nLog("Quests");
        public static Vector3 positionFirst = new Vector3(-1031.3076, -2736.0532, 19.09441);
        public static Vector3 positionSecond = new Vector3(-1061.9181, -1712.5243, 3.44516);
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                NAPI.Blip.CreateBlip(280, positionFirst, 0.7f, 5, Main.StringToU16("NPC: Бурно Коллинз"), 255, 0, true, 0, 0);
                NAPI.Blip.CreateBlip(280, positionFirst, 0.7f, 2, Main.StringToU16("NPC: Глория Коллинз"), 255, 0, true, 0, 0);

                new ColshapeQuest(new Vector3(-1037.4255, -1734.919, -8.464532), 1030, 1, 1, "Очки глории");
                new ColshapeQuest(new Vector3(-1027.2988, -1711.0444, -8.700983), 1030, 2, 1, "Очки глории");
                new ColshapeQuest(new Vector3(-1006.991, -1701.5065, -9.522463), 1030, 3, 1, "Очки глории");
                var shape = NAPI.ColShape.CreateCylinderColShape(positionFirst, 1.2f, 2, 0); shape.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 1000); Trigger.PlayerEvent(player, "client::showhintHUD", true, "Бурно Коллинз"); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } }; shape.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); Trigger.PlayerEvent(player, "client::showhintHUD", false, null); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } };
                var shape2 = NAPI.ColShape.CreateCylinderColShape(positionSecond, 1.2f, 2, 0); shape2.OnEntityEnterColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 1001); Trigger.PlayerEvent(player, "client::showhintHUD", true, "Глория Коллинз"); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } }; shape2.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); Trigger.PlayerEvent(player, "client::showhintHUD", false, null);  } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } };
            }
            catch { }
        }

        public static void Interactions(Player player, int id)
        {
            switch (id)
            {
                case 1000:
                    if (Main.Players[player].Achievements[0] == false && Main.Players[player].Achievements[1] == false)
                    {
                        Trigger.PlayerEvent(player, "NPC.cameraOn", "Bruno", 500);
                        Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Бруно Коллинз", "Проводник", "Привет, только приехал? Может помощь нужна?", (new QuestAnswer("Не помешало бы...", 0), 0));
                    }
                    else
                    {
                        Trigger.PlayerEvent(player, "NPC.cameraOn", "Bruno", 500);
                        Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Бруно Коллинз", "Проводник", "Привет я Бруно!", (new QuestAnswer("Ясно", 2), 0));
                    }
                    return;
                case 1001:
                    if (Main.Players[player].Achievements[0] == true && Main.Players[player].Achievements[1] == false)
                    {
                        Trigger.PlayerEvent(player, "NPC.cameraOn", "Gloria", 500);
                        Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Глория Коллинз", "Местная", "Привет! Неужели мой сын позвал тебя помочь мне, вот он негодяй! Помоги мне достать мои очки с моря, я их случайно туда уронила.", (new QuestAnswer("Хорошо", 3), new QuestAnswer("Потом...", 2)));
                    }
                    if (Main.Players[player].Achievements[1] == true && Main.Players[player].Achievements[2] == false)
                    {
                        Trigger.PlayerEvent(player, "NPC.cameraOn", "Gloria", 500);
                        Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Глория Коллинз", "Местная", "Нашел очки?", (new QuestAnswer("Пока нет...", 2), 0));
                    }
                    if (Main.Players[player].Achievements[2] == true && Main.Players[player].Achievements[3] == false)
                    {
                        Trigger.PlayerEvent(player, "NPC.cameraOn", "Gloria", 500);
                        Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Глория Коллинз", "Местная", "О, мои очки. Спасибо! ", (new QuestAnswer("Пока нет...", 2), 0));
                    }
                    return;
            }
        }
        [RemoteEvent("server::takequestcheckmenu")]
        public static void SERVER_TAKEQUESTCHECKMENU(Player player, int id)
        {
            /*switch (id)
            {
                  case 0:
                      Main.Players[player].Achievements[4] = true;
                      Trigger.ClientEvent(player, "NPC.cameraOn", "EmmaYoung", 500);
                      Trigger.ClientEvent(player, "client::addToMissionsOnHud", true, "Первая зелень", "Посетите Эмму Йонг и узнайте о задании");
                      Trigger.ClientEvent(player, "client::opendialogmenu", true, "Эмма Йонг", "Аренда скутеров", "Ладно... Раз уж ты у него на побегушках, то должен справиться с простецким дельцем! Я обещала своей сестре помочь на ферме, но у меня и так дел полно, хватай мопед и гони туда", (new QuestAnswer("Продолжить", 8), 0));
                      return;
               }
                
                
            }  */
        }
        public static void QUEST_TAKEGLASSES(Player player)
        {
            Notify.Succ(player, "Вы нашли очки Глории, отнесите их ей");
            Main.Players[player].Achievements[2] = true;
            Character.Character.CheckAchievements(player);
            player.SetData("QUESTARGS", 0);
            Trigger.PlayerEvent(player, "client::createradiusblip", false, new Vector3(-1000.3474, -1699.3494, -12.455874), 11, 53);
        }
        public class ColshapeQuest
        {
            public int Interaction { get; set; }
            public int args { get; set; }
            public Vector3 Pos { get; set; }
            public float radius { get; set; } 
            public string text { get; set; } 

            public ColshapeQuest(Vector3 pos, int inter, int arg, float rad, string txt)
            {
                Pos = pos;
                Interaction = inter;
                args = arg;
                radius = rad;
                text = txt;

                GTAElements();
            }

            public void GTAElements()
            {
                var shape = NAPI.ColShape.CreateCylinderColShape(Pos, radius, 2, 0); shape.OnEntityEnterColShape += (shape, player) => { try { if (player.HasData("QUESTARGS") && player.GetData<int>("QUESTARGS") == args) { player.SetData("INTERACTIONCHECK", Interaction); Trigger.PlayerEvent(player, "client::showhintHUD", true, text); } } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } }; shape.OnEntityExitColShape += (shape, player) => { try { player.SetData("INTERACTIONCHECK", 0); Trigger.PlayerEvent(player, "client::showhintHUD", false, null); } catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); } };
            }
        }
        public class QuestAnswer
        {
            public string Text { get; set; }
            public int Event { get; set; }
            public QuestAnswer(string text, int eventn)
            {
                Text = text;
                Event = eventn;
            }
        }
    }
}
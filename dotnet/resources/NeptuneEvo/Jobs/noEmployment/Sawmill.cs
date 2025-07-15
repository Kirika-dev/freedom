using GTANetworkAPI;
using System.Collections.Generic;
using System;
using NeptuneEVO.GUI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using Newtonsoft.Json;
using System.Data;

namespace NeptuneEVO.Jobs
{
    class Sawmill : Script
    {
        private static nLog Log = new nLog("Sawmill");
        public static Vector3 Position = new Vector3(-549.5424, 5431.908, 61.297855);
        public static Vector3 PositionMarket = new Vector3(-687.3896, 5487.0215, 46.193115);
        private static string Model = "prop_tree_log_01";
        private static int LastID = 0;
        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM sawmilljob");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB SMJ return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    int id = Convert.ToInt32(Row["id"].ToString());
                    Vector3 pos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());
                    new Checkpoint(id, pos, false, 0);
                    LastID = id;
                }
                SafeZones.CreateSafeZone(Position - new Vector3(0,0,30), 200, 100, 0, name: "Sawmill");

                new MarketNPC(1, "MNPC_Sawmill", "Карл Магнум", "Покупка инструмента", PositionMarket);
                NAPI.Blip.CreateBlip(468, PositionMarket, 0.8f, 52, Main.StringToU16("Лес"), 255, 0, true, 0, 0);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }

        }

        [Command("createsamwillpoint")]
        public static void CMD_CreateSamwillPoint(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (Main.Players[player].AdminLVL < 10) return;
            LastID++;
            Vector3 pos = player.Position - new Vector3(0,0,1.25);
            new Checkpoint(LastID, pos, false, 0);
            MySQL.Query($"INSERT INTO `sawmilljob` (`id`, `pos`) VALUES({LastID},'{JsonConvert.SerializeObject(pos)}')");
        }

        [RemoteEvent("server::sawmill:click")]
        public static void Sawmill_Click(Player player)
        {
            try
            {
                if (player != null && Main.Players.ContainsKey(player))
                {
                    Checkpoint tree = player.GetData<Checkpoint>("Sawmill");
                    if (tree.PlayerTasking == false && tree.Destroy == false && player.HasSharedData("Axe.InHands") && player.GetSharedData<bool>("Axe.InHands") == true)
                    {
                        player.PlayAnimation("melee@large_wpn@streamed_core", "car_side_attack_a", 47);
                        //Trigger.PlayerEvent(player, "client::soundplay", "./sounds/pickaxe.ogg", 0.5);   //Это звук я просто не нашел для дровосека
                        player.SetSharedData("SAWMILL_PLAYANIM", true);
                        tree.Health -= 25;
                        tree.PlayerTasking = true;
                        if (tree.Health <= 0)
                        {
                            var countitem = new Random().Next(1, 3);   //1 to 2
                            int tryAdd = Core.nInventory.TryAdd(player, new nItem(ItemType.WoodPile, countitem));
                            if (tryAdd == -1 || tryAdd > 0)
                                Notify.Alert(player, $"Недостаточно места");
                            else
                                nInventory.Add(player, new nItem(ItemType.WoodPile, countitem));
                            tree.Destroying();
                            //Trigger.PlayerEvent(player, "client::soundplay", "./sounds/breakrock.ogg", 0.5);  //Это звук я просто не нашел для дровосека
                            player.SetSharedData("SAWMILL_ON_TREE", false);
                        }
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && Main.Players.ContainsKey(player))
                                {
                                    player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                    player.SetSharedData("SAWMILL_PLAYANIM", false);
                                    tree.PlayerTasking = false;
                                }
                            }
                            catch { }
                        }, 1900);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine("SAWMILL.error: " + e.Message); }
        }
        internal class Checkpoint
        {
            public static Dictionary<int, Checkpoint> List = new Dictionary<int, Checkpoint>();
            public int ID { get; set; }
            public Vector3 Position { get; }
            public bool Destroy { get; set; }
            public int Time { get; set; }
            public bool PlayerTasking { get; set; }
            public int Health { get; set; }
            [JsonIgnore]
            public GTANetworkAPI.ColShape Shape { get; set; }
            [JsonIgnore]
            public GTANetworkAPI.Object Handle { get; set; }

            public Checkpoint(int id, Vector3 pos, bool destroy, int time, bool pld = false, int hp = 100)
            {
                ID = id; Position = pos; Destroy = destroy; Time = time; PlayerTasking = pld; Health = hp;
                Shape = NAPI.ColShape.CreateCylinderColShape(Position, 3, 5, 0);
                Handle = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(Model), Position, new Vector3(0,0, Main.rnd.Next(0, 180)), 255);
                Handle.SetSharedData("SAWMILL_OBJECT", true);
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetSharedData("SAWMILL_ON_TREE", true);
                        entity.SetData("Sawmill", this);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetSharedData("SAWMILL_ON_TREE", false);
                        entity.ResetData("Sawmill");
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                List.Add(ID, this);
            }
            public void Destroying()
            {
                try
                {
                    Checkpoint parent = List[ID];
                    Handle.Delete();
                    Destroy = true;
                    Time = 2;
                    PlayerTasking = false;
                    Health = 100;
                }
                catch { }
            }
            public void Reload()
            {
                try
                {
                    Checkpoint parent = List[ID];
                    if (Handle == null || Destroy == true)
                    {
                        if (Time == 0)
                        {
                            NAPI.Task.Run(() => { Handle = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(Model), Position, new Vector3()); Handle.SetSharedData("SAWMILL_OBJECT", true); });
                            Destroy = false;
                        }
                        else
                        {
                            Time--;
                        }
                    }
                }
                catch { }
            }
        }
    }
}

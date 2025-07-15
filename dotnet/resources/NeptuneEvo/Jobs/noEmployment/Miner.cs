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
    class Miner : Script
    {
        private static nLog Log = new nLog("Carrier");
        public static Vector3 Position = new Vector3(2832.54, 2798.8704, 56.332718);
        private static string Model = "prop_coral_stone_03";
        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM minerjob");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB MJ return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    int id = Convert.ToInt32(Row["id"].ToString());
                    Vector3 pos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());
                    new Checkpoint(id, pos, false, 0);
                }
                SafeZones.CreateSafeZone(Position - new Vector3(0,0,30), 250, 100, 0, name: "Carrier");

                new MarketNPC(0, "MNPC_Miner", "Роберт Маккензи", "Покупка инструмента", Position);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }

        }
        private static int[] MinerChance = { 5, 15, 30, 50 };
        private static ItemType[] OreName = { ItemType.GoldOre, ItemType.SilverOre, ItemType.CuprumOre, ItemType.IronOre };
        private static ItemType Random_Ore()
        { 
            Random rnd = new Random();
            double rand = rnd.NextDouble() * 100;
            if (rand > 100 - MinerChance[0])
            {
                return OreName[0];
            }
            else if(rand > 100 - MinerChance[0] - MinerChance[1])
            {
                return OreName[1];
            }
            else if (rand > 100 - MinerChance[0] - MinerChance[1] - MinerChance[2])
            {
                return OreName[2];
            }
            else
            {
                return OreName[3];
            }
        }

        [RemoteEvent("server::miner:click")]
        public static void Miner_Click(Player player)
        {
            try
            {
                if (player != null && Main.Players.ContainsKey(player))
                {
                    Checkpoint stone = player.GetData<Checkpoint>("Miner");
                    if (stone.PlayerTasking == false && stone.Destroy == false && player.HasSharedData("PickAxe.InHands") && player.GetSharedData<bool>("PickAxe.InHands") == true)
                    {
                        player.PlayAnimation("melee@large_wpn@streamed_core", "ground_attack_on_spot", 47);
                        Trigger.PlayerEvent(player, "client::soundplay", "./sounds/pickaxe.ogg", 0.5);
                        player.SetSharedData("MINER_PLAYANIM", true);
                        stone.Health -= 10;
                        stone.PlayerTasking = true;
                        if (stone.Health == 50)
                        {
                            var countstone = new Random().Next(1, 4);
                            int tryAdd = Core.nInventory.TryAdd(player, new nItem(ItemType.Stone, countstone));
                            if (tryAdd == -1 || tryAdd > 0)
                                Notify.Alert(player, $"Недостаточно места");    
                            else 
                                nInventory.Add(player, new nItem(ItemType.Stone, countstone));
                        }
                        if (stone.Health <= 0)
                        {
                            stone.Destroying();
                            Trigger.PlayerEvent(player, "client::soundplay", "./sounds/breakrock.ogg", 0.5);
                            player.SetSharedData("MINER_ON_ORE", false);
                            ItemType item = Random_Ore();
                            int tryAdd = Core.nInventory.TryAdd(player, new nItem(item));
                            if (tryAdd == -1 || tryAdd > 0)
                                Notify.Alert(player, $"Недостаточно места");
                            else
                            {
                                nInventory.Add(player, new nItem(item, 1));
                                BattlePass.AddProgressToQuest(player, 2, 1);
                            }
                        }
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null && Main.Players.ContainsKey(player))
                                {
                                    player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                                    player.SetSharedData("MINER_PLAYANIM", false);
                                    stone.PlayerTasking = false;
                                }
                            }
                            catch { }
                        }, 1900);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine("Miner.error: " + e.Message); }
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
                Handle = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(Model), Position, new Vector3(), 255);
                Handle.SetSharedData("MINER_OBJECT", true);
                Shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetSharedData("MINER_ON_ORE", true);
                        entity.SetData("Miner", this);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                Shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetSharedData("MINER_ON_ORE", false);
                        entity.ResetData("Miner");
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
                    Time = 10;
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
                            NAPI.Task.Run(() => { Handle = NAPI.Object.CreateObject(NAPI.Util.GetHashKey(Model), Position, new Vector3()); Handle.SetSharedData("MINER_OBJECT", true); });
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

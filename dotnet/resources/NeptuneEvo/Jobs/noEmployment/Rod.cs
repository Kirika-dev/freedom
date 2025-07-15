using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NeptuneEVO.GUI;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.SDK;
using System.Threading;

namespace NeptuneEVO.Core
{
    class RodManager : Script
    {
        class Fish
        {
            public int Chance { get; set; }
            public ItemType Type { get; set; }
            public Fish(int chance, ItemType type)
            {
                Chance = chance; Type = type;
            }
        }
        [ServerEvent(Event.PlayerDeath)]
        public void OnPlayerDeath(Player player, Player killer, uint reason)
        {
            BasicSync.DetachObject(player);
        }
        private static List<Fish> Ocean_Fish = new List<Fish>()
        {
            new Fish(10, ItemType.Amyr),
            new Fish(15, ItemType.Skat),
            new Fish(20, ItemType.Tunec),
            new Fish(15, ItemType.Kyndja),
            new Fish(20, ItemType.Okyn),
            new Fish(20, ItemType.Ocetr),
        };
        private static List<Fish> River_Fish = new List<Fish>()
        {
           new Fish(5, ItemType.Ygol),
           new Fish(10, ItemType.Chyka),
           new Fish(10, ItemType.Tunec),
           new Fish(15, ItemType.Lococ),
           new Fish(10, ItemType.Koroska),
           new Fish(30, ItemType.Okyn),
           new Fish(20, ItemType.Ocetr),
        };
        private static List<Fish> Pier_Fish = new List<Fish>()
        {
            new Fish(25, ItemType.Okyn),
            new Fish(25, ItemType.Ocetr),
            new Fish(10, ItemType.Skat),
            new Fish(20, ItemType.Tunec),
            new Fish(10, ItemType.Lococ),
            new Fish(10, ItemType.Koroska),
        };
        private static ItemType RandomRiverFish()
        {
            Random rnd = new Random();
            double rand = rnd.NextDouble() * 100;
            if (rand > 100 - River_Fish[0].Chance)
                return River_Fish[0].Type;
            else if (rand > 100 - River_Fish[0].Chance - River_Fish[1].Chance)
                return River_Fish[1].Type;
            else if (rand > 100 - River_Fish[0].Chance - River_Fish[1].Chance - River_Fish[2].Chance)
                return River_Fish[2].Type;        
            else if (rand > 100 - River_Fish[0].Chance - River_Fish[1].Chance - River_Fish[2].Chance - River_Fish[3].Chance)
                return River_Fish[3].Type;
            else if (rand > 100 - River_Fish[0].Chance - River_Fish[1].Chance - River_Fish[2].Chance - River_Fish[3].Chance - River_Fish[4].Chance)
                return River_Fish[4].Type;
            else if (rand > 100 - River_Fish[0].Chance - River_Fish[1].Chance - River_Fish[2].Chance - River_Fish[3].Chance - River_Fish[4].Chance - River_Fish[5].Chance)
                return River_Fish[5].Type;
            else 
                return River_Fish[6].Type;
        }
        private static ItemType RandomPierFish()
        {
            Random rnd = new Random();
            double rand = rnd.NextDouble() * 100;
            if (rand > 100 - Pier_Fish[0].Chance)
                return Pier_Fish[0].Type;
            else if (rand > 100 - Pier_Fish[0].Chance - Pier_Fish[1].Chance)
                return Pier_Fish[1].Type;
            else if (rand > 100 - Pier_Fish[0].Chance - Pier_Fish[1].Chance - Pier_Fish[2].Chance)
                return Pier_Fish[2].Type;
            else if (rand > 100 - Pier_Fish[0].Chance - Pier_Fish[1].Chance - Pier_Fish[2].Chance - Pier_Fish[3].Chance)
                return Pier_Fish[3].Type;
            else if (rand > 100 - Pier_Fish[0].Chance - Pier_Fish[1].Chance - Pier_Fish[2].Chance - Pier_Fish[3].Chance - Pier_Fish[4].Chance)
                return Pier_Fish[4].Type;
            else
                return Pier_Fish[5].Type;
        }
        private static ItemType RandomOceanFish()
        {
            Random rnd = new Random();
            double rand = rnd.NextDouble() * 100;
            if(rand > 100 - Ocean_Fish[0].Chance)
                return Ocean_Fish[0].Type;
            else if (rand > 100 - Ocean_Fish[0].Chance - Ocean_Fish[1].Chance)
                return Ocean_Fish[1].Type;
            else if (rand > 100 - Ocean_Fish[0].Chance - Ocean_Fish[1].Chance - Ocean_Fish[2].Chance)
                return Ocean_Fish[2].Type;
            else if (rand > 100 - Ocean_Fish[0].Chance - Ocean_Fish[1].Chance - Ocean_Fish[2].Chance - Ocean_Fish[3].Chance)
                return Ocean_Fish[3].Type;
            else if (rand > 100 - Ocean_Fish[0].Chance - Ocean_Fish[1].Chance - Ocean_Fish[2].Chance - Ocean_Fish[3].Chance - Ocean_Fish[4].Chance)
                return Ocean_Fish[4].Type;
            else
                return Ocean_Fish[5].Type;
        }
        [RemoteEvent("server::fish::game:finish")]
        public static void FinishGame(Player player, bool state, int depth)
        {
            if (state)
            {
                ItemType fishtype;
                switch (depth)
                {
                    case 0:
                        fishtype = RandomRiverFish();
                        break;
                    case 1:
                        fishtype = RandomPierFish();
                        break;
                    case 2:
                        fishtype = RandomOceanFish();
                        break;
                    default:
                        fishtype = RandomRiverFish();
                        break;
                }
                nInventory.Add(player, new nItem(fishtype, 1));
                Notify.Succ(player, $"Вы поймали {nInventory.InventoryItems.Find(x => x.ItemType == fishtype).Name}");
                if (fishtype == ItemType.Okyn)
                {
                    BattlePass.AddProgressToQuest(player, 1, 1);
                }
            }
            else
            {
                Notify.Error(player, "Вы упустили рыбу");
            }
            Main.StopSyncAnimation(player);
            foreach (nItem item in nInventory.Items[Main.Players[player].UUID])
            {
                if (item.Type == ItemType.Rod && item.IsActive)
                {
                    if (item.Wear <= 0)
                    {
                        Notify.Error(player, "Вы сломали удочку");
                        nInventory.Remove(player, item);
                        return;
                    }
                    item.Wear -= 1f;
                }
                if (item.Type == ItemType.RodUpgrade && item.IsActive)
                {
                    if (item.Wear <= 0)
                    {
                        Notify.Error(player, "Вы сломали удочку");
                        nInventory.Remove(player, item);
                        return;
                    }
                    item.Wear -= 0.05f;
                }
            }
        }

        [RemoteEvent("server::fish:stop")]
        public static void StopFishing(Player player)
        {
            Main.StopSyncAnimation(player);
            Timers.Stop(player.GetData<string>("ROD_TIMER"));
            player.ResetData("ROD_TIMER");
        }

        [RemoteEvent("server::fish:start")]
        public static void StartRodingGame(Player player)
        {
            int lvlRod = player.GetSharedData<int>("ROD_LVL");
            int time = 0;
            switch (lvlRod)
            {
                case 1:
                    time = 90;
                    break;
                case 2:
                    time = 50;
                    break;
                case 3:
                    time = 15;
                    break;
            }
            player.SetData("ROD_TIMER", Timers.StartOnceTask(time * 1000, () =>
            {
                try
                {
                    if (player != null && Main.Players.ContainsKey(player) && player.HasData("ROD_TIMER"))
                    {
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Что-то клюнуло...", 3000);
                        Trigger.PlayerEvent(player, "client::fish::game:start");
                        Timers.Stop(player.GetData<string>("ROD_TIMER"));
                        player.ResetData("ROD_TIMER");
                    }
                }
                catch { }
            }));
            Main.PlaySyncAnimation(player, "amb@world_human_stand_fishing@idle_a", "idle_c", 1);
        }

        public static void useInventory(Player player, nItem item, int level)
        {
            if (player.IsInVehicle) return;
            if (player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") == item.Type)
            {
                BasicSync.DetachObject(player);
                player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                player.SetSharedData("ROD_IN_HAND", false);
                player.SetSharedData("ROD_LVL", -1);
                item.IsActive = false;
                player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
            }
            else
            {
                BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_fishing_rod_02"), 18905, new Vector3(0.17, 0.137, 0.013), new Vector3(75, 268, 160));
                player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                player.SetSharedData("ROD_IN_HAND", true);
                player.SetSharedData("ROD_LVL", level);
                item.IsActive = true;
                player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
            }
            return;
        }
    }
}

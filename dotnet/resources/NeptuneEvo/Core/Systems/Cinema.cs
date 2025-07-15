using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEVO.Utils
{
    class Cinema : Script
    {
        public static List<CinemaHall> List = new List<CinemaHall>();
        public static int Price = 1000;
        private static nLog Log = new nLog("Cinema");

        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        {
            try
            {
                new CinemaHall(0, new Vector3(-1423.40611, -215.2932, 46.50), 521);
                new CinemaHall(1, new Vector3(394.16666, -711.7956, 29.28), 522);
                new CinemaHall(2, new Vector3(337.04654, 177.19456, 103.15), 523);
                if (List.Count != 0)
                    Log.Write($"Loaded {List.Count} Cinemas", nLog.Type.Success);
                else
                    Log.Write($"not found cinemas", nLog.Type.Success);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("server::cinema:seturl")]
        public static void ClientEvent_SetURL(Player player, string url)
        {
            if (!player.HasData("CINEMA")) return;
            CinemaHall cinema = player.GetData<CinemaHall>("CINEMA");
            cinema.SetVideo(player, url);
        }
        public static void Enter(Player player)
        {
            if (!player.HasData("CINEMA")) return;
            CinemaHall cinema = player.GetData<CinemaHall>("CINEMA");
            cinema.Enter(player);
        }
        [RemoteEvent("server::cinema:close")]
        public static void ClientEvent_Exit(Player player)
        {
            if (!player.HasData("CINEMA")) return;
            CinemaHall cinema = player.GetData<CinemaHall>("CINEMA");
            cinema.Exit(player);
        }
        [RemoteEvent("server::cinema:skip")]
        public static void ClientEvent_Skip(Player player)
        {
            if (!player.HasData("CINEMA")) return;
            CinemaHall cinema = player.GetData<CinemaHall>("CINEMA");
            cinema.SkipVideo(player);
        }
        internal class CinemaHall
        {
            public int ID { get; set; }
            public int Votes { get; set; }
            public uint Dimension { get; set; }
            public Vector3 EnterPosition { get; set; }
            public List<bool> SeatsState { get; set; } = new List<bool>();
            public List<CinemaURL> URLs { get; set; } = new List<CinemaURL>();
            public List<Vector3> seats { get; set; } = new List<Vector3>();
            public CinemaURL URL { get; set; } = null;
            public static int Time { get; set; } = 0;
            public static bool Playing { get; set; } = false;
            [JsonIgnore]
            public ColShape Shape { get; set; }
            [JsonIgnore]
            public Marker Marker { get; set; }
            [JsonIgnore]
            public Blip Blip { get; set; }
            public CinemaHall(int id, Vector3 pos, uint dim)
            {
                ID = id; EnterPosition = pos; Dimension = dim;
                for (int i = 0; i < 20; i++)
                {
                    SeatsState.Add(false);
                }
                seats.Add(new Vector3(-1418.052, -247.6875, 16.79762));
                seats.Add(new Vector3(-1419.253, -247.6236, 16.79766));
                seats.Add(new Vector3(-1421.659, -247.3815, 16.79851));
                seats.Add(new Vector3(-1423.86, -247.2797, 16.79926));
                seats.Add(new Vector3(-1427.273, -247.3671, 16.79961));
                seats.Add(new Vector3(-1430.556, -247.2636, 16.79898));
                seats.Add(new Vector3(-1432.485, -247.544, 16.79794));
                seats.Add(new Vector3(-1432.636, -249.7136, 16.7938));
                seats.Add(new Vector3(-1429.784, -249.8186, 16.79434));
                seats.Add(new Vector3(-1427.626, -249.6948, 16.7951));
                seats.Add(new Vector3(-1424.524, -249.5078, 16.79521));
                seats.Add(new Vector3(-1422.475, -249.7936, 16.79416));
                seats.Add(new Vector3(-1419.898, -249.7854, 16.79353));
                seats.Add(new Vector3(-1419.298, -251.0738, 16.79095));
                seats.Add(new Vector3(-1421.584, -250.7939, 16.79206));
                seats.Add(new Vector3(-1423.801, -250.8243, 16.79255));
                seats.Add(new Vector3(-1426.224, -250.7932, 16.7932));
                seats.Add(new Vector3(-1428.242, -250.6987, 16.79305));
                seats.Add(new Vector3(-1430.339, -250.802, 16.79233));
                seats.Add(new Vector3(-1432.28, -251.1464, 16.79117));

                Blip = NAPI.Blip.CreateBlip(135, pos, 0.9f, 4, "Кинотеатр", dimension: 0, shortRange: true);
                Marker = NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), 0.7f, new Color(67, 140, 239, 200), false, 0);
                Shape = NAPI.ColShape.CreateCylinderColShape(pos, 2, 2, 0);
                Shape.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (e.IsInVehicle) return;
                        if (e.HasData("CINEMA"))
                        {
                            e.ResetData("CINEMA");
                        }
                        e.SetData("CINEMA", this);
                        e.SetData("INTERACTIONCHECK", 818);
                        Trigger.PlayerEvent(e, "client::showhintHUD", true, "Войти в кинотеатр");
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColshape: " + ex.Message); }
                };
                Shape.OnEntityExitColShape += (s, e) =>
                {
                    try
                    {
                        e.SetData("INTERACTIONCHECK", -1);
                        Trigger.PlayerEvent(e, "client::showhintHUD", false, "");
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColshape: " + ex.Message); }
                };
                List.Add(this);
                CinemaTime();
            }
            public int GetFreeSeat()
            {
                for (int i = 0; i < 20; i++)
                {
                    if (SeatsState[i] == false)
                        return i;
                }
                return -1;
            }
            public static void CinemaTime()
            {
                TimerEx.SetTimer(() =>
                {
                    if (Playing == true)
                    {
                        Time++;
                    }

                }, 5000, 0);
            }
            public bool GetAdmin(Player player)
            {
                return Main.Players[player].AdminLVL > 0 ? true : false;
            }
            public void Enter(Player player)                 
            {
                if (player.IsInVehicle) return;
                player.SetData<CinemaHall>("CINEMA", this);
                int FreePlace = GetFreeSeat();
                if (FreePlace == -1)
                {
                    Notify.Error(player, "В кинотеатре нет мест");
                    return;
                }
                player.SetData<int>("CINEMA_SEAT", FreePlace);
                player.Dimension = Dimension;
                Main.Players[player].ExteriorPos = player.Position;
                Trigger.PlayerEvent(player, "client::fadescreen", 800);
                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        Trigger.PlayerEvent(player, "client::cinema:open", JsonConvert.SerializeObject(URL), Time, JsonConvert.SerializeObject(URLs), Playing, GetAdmin(player), Votes, GetMaxVotes());
                        NAPI.Entity.SetEntityPosition(player, seats[FreePlace]);
                        NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, 180));
                        player.SetSharedData("PLAYER_IN_CINEMA", true);
                    }
                }, 1000);
            } 
            public void SetVideo(Player player, string url)
            {
                if (Main.Players[player].Money < Price)
                {
                    Notify.Error(player, "Недостаточно средств");
                    return;
                }
                string urlnew = url.Replace("https://www.youtube.com/watch?v=", "");
                if (URLs.Count == 0 && URL == null)
                {
                    URL = new CinemaURL(player.Name.Replace("_"," "), urlnew);
                    Time = 0;
                    Playing = true;
                }
                else
                {
                    URLs.Add(new CinemaURL(player.Name.Replace("_", " "), urlnew));
                }
                foreach (var target in API.Shared.GetAllPlayers())
                {
                    if (target.HasSharedData("PLAYER_IN_CINEMA") && target.GetSharedData<bool>("PLAYER_IN_CINEMA") == true && target.HasData("CINEMA") && target.GetData<CinemaHall>("CINEMA").ID == ID)
                    {
                        target.TriggerEvent("client::cinema:skipvideo", Time, JsonConvert.SerializeObject(URL), JsonConvert.SerializeObject(URLs), Votes, GetMaxVotes());
                    }
                }
                MoneySystem.Wallet.Change(player, -1000);
                BattlePass.AddProgressToQuest(player, 16, 1);
                Notify.Succ(player, "Ваше видео добавлено в очередь");
            }
            public int GetPlayersInCinema()
            {
                int all = 0;
                for (int i = 0; i < 20; i++)
                {
                    if (SeatsState[i] == true)
                        all += 1;
                }
                return all;
            }
            public int GetMaxVotes()
            {
                int allP = GetPlayersInCinema();
                if (allP <= 2 && allP >= 4)
                    return allP / 2;
                else
                    return 1;
            }
            public void SkipVideo(Player player)
            {
                if (URLs.Count == 0) return;
                Votes+=1;
                if (Votes >= GetMaxVotes())
                {
                    URL = URLs[0];
                    Time = 0;
                    Votes = 0;
                    URLs.Remove(URLs[0]);
                    foreach (var target in API.Shared.GetAllPlayers())
                    {
                        if (target.HasSharedData("PLAYER_IN_CINEMA") && target.GetSharedData<bool>("PLAYER_IN_CINEMA") == true && target.HasData("CINEMA") && target.GetData<CinemaHall>("CINEMA").ID == ID)
                        {
                            target.TriggerEvent("client::cinema:skipvideo", Time, JsonConvert.SerializeObject(URL), JsonConvert.SerializeObject(URLs), Votes, GetMaxVotes());
                            Notify.Succ(target, "Видео было пропущено");
                        }
                    }
                }
                foreach (var target in API.Shared.GetAllPlayers())
                {
                    if (target.HasSharedData("PLAYER_IN_CINEMA") && target.GetSharedData<bool>("PLAYER_IN_CINEMA") == true && target.HasData("CINEMA") && target.GetData<CinemaHall>("CINEMA").ID == ID)
                    {
                        Trigger.PlayerEvent(target, "client::cinema:sendvote", Votes, GetMaxVotes());
                    }
                }
            }
            public void Exit(Player player)
            {
                if (!player.HasData("CINEMA_SEAT")) return;
                Main.Players[player].ExteriorPos = new Vector3();
                Trigger.PlayerEvent(player, "client::fadescreen", 800);
                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        SeatsState[player.GetData<int>("CINEMA_SEAT")] = false;
                        NAPI.Entity.SetEntityPosition(player, EnterPosition);
                        NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, 0));
                        NAPI.Entity.SetEntityDimension(player, 0);
                        player.SetSharedData("PLAYER_IN_CINEMA", false);
                        player.ResetData("CINEMA_SEAT");
                        player.ResetData("CINEMA");
                    }
                }, 1000);
            }
        }
        internal class CinemaURL
        {
            public string Name { get; set; }
            public string URL { get; set; }
            public string NameVideo { get; set; }
            
            public CinemaURL(string name, string url)
            {
                Name = name;
                URL = url;
                NameVideo = "";
            }
        }
    }
}
//Автор системы JJiGolem#7069
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Timers;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.Fractions;
using NeptuneEVO.GUI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core.Character;

namespace NeptuneEVO.Core
{
    class MetroSystem : Script
    {
        private static List<MetroRoute> _routes;
        private static int _ticketPrice = 300;

        [ServerEvent(Event.ResourceStart)]
        public void ResourceStart()
        {
            _routes = new List<MetroRoute>()
            {
                new MetroRoute(1, new List<MetroStation>()
                {
                    new MetroStation(1, "LSIA Terminal", new Vector3(-1089.08, -2722.46, -8.5), new Vector3(-1080.33, -2723.51, -9.42), new Vector3(-1091.47, -2713.67, -9.42)),
                    new MetroStation(2, "LSIA Parking", new Vector3(-880.618, -2311.557, -12.2), new Vector3(-867.76, -2297.91, -12.75), new Vector3(-882.04, -2292.92, -13.75)),
                    new MetroStation(3, "Puerto Del Sol", new Vector3(-534.58, -1272.25, 25.8), new Vector3(-533.88, -1279.40, 24.78), new Vector3(-542.53, -1275.31, 24.78)),
                    new MetroStation(4, "Strawberry", new Vector3(278.71, -1205.81, 38), new Vector3(277.61, -1210.10, 36.95), new Vector3(279.32, -1198.60, 36.95)),
                    new MetroStation(5, "Burton", new Vector3(-291.53, -318.96, 9), new Vector3(-287.01, -319.07, 8.06), new Vector3(-302.09, -318.65, 8.06)),
                    new MetroStation(6, "Portola Drive", new Vector3(-817.60, -139.27, 19), new Vector3(-818.40, -130.52, 17.93), new Vector3(-810.50, -143.64, 17.93)),
                    new MetroStation(7, "Del Perro", new Vector3(-1350.61, -466.92, 14), new Vector3(-1358.88, -466.93, 13.03), new Vector3(-1345.65, -459.87, 13.03)),
                    new MetroStation(8, "Little Seoul", new Vector3(-498.40, -673.42, 10.7), new Vector3(-502.36, -680.68, 9.79), new Vector3(-502.59, -665.63, 9.79)),
                    new MetroStation(9, "Pillbox South", new Vector3(-213.49, -1030.22, 29), new Vector3(-217.25, -1030.57, 28.20), new Vector3(-208.98, -1035.07, 28.20)),
                    new MetroStation(10, "Davis", new Vector3(111.75, -1724.17, 29), new Vector3(111.31, -1727.83, 27.93), new Vector3(118.89, -1723.10, 28))
                }),

            };
        }

        public static void InteractionPressed(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;

            if (player.HasData("METRO_STATION"))
            {
                MetroStation station = player.GetData<MetroStation>("METRO_STATION");
                if (station != null)
                {
                    MetroRoute route = _routes.Find(x => x.Stations.First(x => x == station) != null);
                    if (route != null)
                    {
                        Trigger.PlayerEvent(player, "metro::open_menu", _ticketPrice, station.Id);
                    }
                }
            }
        }
                                                                                                        
        [RemoteEvent("metro::buy_ticket")]
        public static void buyTicket_Server(Player player, int stationId)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (player.HasData("METRO_STATION"))
                {
                    MetroStation station = player.GetData<MetroStation>("METRO_STATION");
                    if (station != null)
                    {
                        if (Main.Players[player].Money < _ticketPrice)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас недостаточно денег" ,3000);
                            return;
                        }
                        if (station.Id == stationId)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы уже находитесь на этой станции" ,3000);
                            return;
                        }
                        MetroRoute route = _routes.Find(x => x.Stations.FirstOrDefault(x => x == station) != null);
                        if (route != null)
                        {
                            MetroStation wStation = route.Stations.FirstOrDefault(x => x.Id == stationId);
                            if (wStation == null)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Попробуйте снова!" ,3000);
                                return;
                            }

                            int aIndex = route.Stations.FindIndex(x => x == station);
                            int wIndex = route.Stations.FindIndex(x => x == wStation);
                            station.SpawnTrain(player, wStation, aIndex > wIndex);
                            
                            MoneySystem.Wallet.Change(player, -_ticketPrice);
                        }
                    }
                }
            }
            catch (Exception) {  }
        }

        [RemoteEvent("metro::race_finish")]
        public void MetroRaceFinish(Player player)
        {
            Core.Dimensions.DismissPrivateDimension(player);
            NAPI.Entity.SetEntityDimension(player, 0);
        }

        private class MetroRoute
        {
            public int Id { get; }
            public List<MetroStation> Stations;

            public MetroRoute(int id, List<MetroStation> stations)
            {
                Id = id;
                Stations = stations;
            }
        }

        private class MetroStation
        {
            public int Id { get; }
            public string Name { get; }

            [JsonIgnore]
            public Vector3 Position { get; }

            [JsonIgnore]
            public List<Vector3> TrainSpawnPositions { get; }

            public MetroStation(int id, string name, Vector3 shapePosition, Vector3 spawnPosition1, Vector3 spawnPosition2)
            {
                Id = id;
                Name = name;

                Position = shapePosition;

                TrainSpawnPositions = new List<Vector3>(2)
                {
                    spawnPosition1, spawnPosition2
                };

                GreateGTAElements();
            }

            public void SpawnTrain(Player player, MetroStation station, bool forward)
            {
                uint privateDimension = Core.Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(player, privateDimension);

                Vector3 spawnPos = TrainSpawnPositions[forward ? 1 : 0];
                Vector3 pointPos = station.TrainSpawnPositions[forward ? 1 : 0];
                Trigger.PlayerEvent(player, "metro::start_race", spawnPos, pointPos, station.Position, privateDimension);
            }

            [JsonIgnore]
            private ColShape _shape;
            [JsonIgnore]
            private Marker _marker;

            private void GreateGTAElements()
            {
                _marker = NAPI.Marker.CreateMarker(1, Position, new Vector3(), new Vector3(), 0.7f, new Color(67, 140, 239, 200), false, 0);
                _shape = NAPI.ColShape.CreateCylinderColShape(Position, 1f, 2f, 0);
                _shape.OnEntityEnterColShape += (s, e) =>
                {
                    e.SetData("INTERACTIONCHECK", 817);
                    e.SetData("METRO_STATION", this);
                    Trigger.PlayerEvent(e, "client::showhintHUD", true, $"Станция {Name}");
                };
                _shape.OnEntityExitColShape += (s, e) =>
                {
                    e.ResetData("METRO_STATION");
                    e.SetData("INTERACTIONCHECK", -1);
                    Trigger.PlayerEvent(e, "client::showhintHUD", false, "");
                };
            }
        }
    }
}

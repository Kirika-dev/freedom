using GTANetworkAPI;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using System;
using MySqlConnector;

namespace NeptuneEVO.Businesses
{
    class AutoShopI : Script
    {

        private static nLog Log = new nLog("AUTOSHOP");

        private static Dictionary<string, Color> carColors = new Dictionary<string, Color>
        {
            { "Белый", new Color(255, 255, 255) },
            { "Серый", new Color(110, 110, 110) },
            { "Черный", new Color(0, 0, 0) },
            { "Красный", new Color(255, 0, 0) },
            { "Оранжевый", new Color(255, 132, 57) },
            { "Фиолетовый", new Color(127, 57, 255) },
            { "Синий", new Color(57, 126, 255) }, 
        };

        public static List<Dictionary<string, int>> ProductsList = new List<Dictionary<string, int>>
        {
            new Dictionary<string, int> {
                {"merse63", 660000},
				{"m5comp", 1100000},
				{"f10", 1000000},
				{"gle63", 670000},
				{"m3f80", 340000},
                {"bmwg07", 850000},
				{"g65go", 2500000},
                {"cullinan", 2400000},
                {"escalade", 700000},
                {"63gls", 200000},
                {"cls17", 520000},
                {"s63w222", 900000},
                {"rs62", 1500000},
                {"fordraptor", 470000},
                {"teslax", 850000},
                {"lx2018", 750000},
                {"rs5audi", 600000},
                {"kiastinger", 230000},
                {"jeepsrt", 500000},
                {"16charger", 800000},
                {"shelbygt500", 640000},
                {"MGT", 800000},
                {"camry70", 500000},
                {"audiq7", 1000000},
                {"cayman", 1500000},
                {"challenger", 600000},
                {"bmwx5mc", 2000000},
                {"m6f06", 1500000},
                {"f812", 4900000},
                {"laferrari17", 5000000 }, 
                {"rr14", 900000},
                {"s3sedan", 300000},
                {"w221s63", 450000},
                { "m4comp", 4000000 },
            }, // 0
            new Dictionary<string, int> {
				{"lada2107", 2000},
                {"apriora", 4000},
				{"octaviavrs", 110000},
				{"w140s600", 90000},
                {"e39m5", 110000},
                {"golfgti", 120000},
                {"w210", 60000},
                {"500w124", 76000},
                {"bmwe38", 60000},
                {"s15", 350000},
                {"bmwe34", 32000},
                {"g65", 850000},
                {"e63s", 1500000},
                {"m4f82", 650000},
                {"e63sf", 1900000},
                {"e60", 500000},
                {"bentayga", 4500000},
                {"camryv55", 500000},
                {"vesta", 100000},
                {"continental", 1700000},
                {"tsgr20", 600000},
                {"a80", 750000},
                {"c63scoupe", 1050000},
            }, // 1
            new Dictionary<string, int> {
                {"sf90", 21000000 },
                {"rmodbacalar", 15000000 },   
                { "mcgts",850000},
                { "m750li",1200000},
                { "v447",1500000},
                { "bmwm8",1800000},
                { "panamera_st",550000},
                { "gtr17",950000},
                { "r8v10",800000},
                { "gt63",1800000},
                { "mp1",6000000},
                { "teslaroad",1000000},
                { "agerars",1700000},
                { "huracan",3900000},
                { "taycan",2000000},
                { "rs7c8",4000000},
                { "s500",5500000},
                { "bmwm2",1700000},
                { "chiron19",29000000},
                { "rmodbugatti",79000000},
                { "skyline",400000},
                { "divo", 37000000},
                { "svr",800000},
                { "z4vp",900000},
                { "jzx100",900000},
                { "impala96",600000},
                { "toyotalc200",800000},
                { "x6m2",2500000},
                { "mercxclass",450000},
                { "lanxvp",120000},
                { "g63amg6x6",15000000},
                { "urus",1400000},
                { "amggt16",2000000},

            }, // 2 -эксклюзивный
            new Dictionary<string, int> {
                { "Bmx",1000},
				{ "Scorcher",2000},
				{ "Faggio2",2000},
				{ "Blazer", 3300 },
				{ "Enduro",3500},
				{ "Thrust",5000},
				{ "PCJ",6000},
				{ "Hexer",6780},
				{ "lectro",7000},
				{ "Nemesis",7200},
				{ "Double",8000},
				{ "Diablous",8200},
				{ "Cliffhanger",8300},
				{ "Nightblade", 8300 },
				{ "Vindicator", 8500 },
				{ "Gargoyle", 8900 },
				{ "Sanchez2",8900 },
				{ "Akuma",9000 },
				{ "Ratbike", 9300 },
				{ "CarbonRS",10000 },
				{ "Ruffian",10000 },
				{ "Hakuchou",12000 },
				{ "Bati",14000 },
				{ "BF400",14500 },
				{ "Sanctus", 15000 },
                { "verus", 40000 },
                //
                { "ninjah2r", 15000000 },
                { "cbr17", 9000000 },

            }, // 3 - moto
            new Dictionary<string, int> {
                { "amgone", 2199 },
                { "go650", 1599 },
                { "rmodi8ks", 1999 },
                { "rolls08", 1799 },
                { "veloci6x6", 1799 },
                { "m1procar", 2399 },
				{ "922smg", 2299},
                { "f8t", 2799},
                { "rrwraith", 2699 },
                { "asvj", 2899},
                { "675ltsp", 2899 },
                { "pullman", 3699 },
                { "BMWM5CS", 1399 },
                { "sian", 3599 },
                { "fastback", 1399 },
            }, // 4 - донатный
            new Dictionary<string, int> {
				{ "boxville4", 75000 },
                { "msprinter", 250000 },
				{ "nspeedo", 425000 },
                { "pounder2", 575000 },
            }, // 5 - грузовики
            new Dictionary<string, int> { // КЕЙСЫ
            }, // 6
            new Dictionary<string, int> {
                { "microlight", 30000000 },
                { "howard", 30000000 },
                { "buzzard2", 30000000 },
                { "seasparrow2", 30000000 },
                { "frogger", 30000000 },
                { "havok", 30000000 },
                { "maverick", 35000000 },
                { "seasparrow", 29000000 },
                { "supervolito", 26000000 },
                { "supervolito2", 30000000 },
                { "swift", 30000000 },
                { "swift2", 30000000 },
                { "volatus", 30000000 },
            } // 7
        };

        [ServerEvent(Event.PlayerDeath)]
        public void onPlayerDeathHandler(Player player, Player entityKiller, uint weapon)
        {
            try {
                if (player.IsInVehicle && player.Vehicle.HasData("ACCESS") && player.Vehicle.GetData<string>("ACCESS") == "TESTDRIVE")
                {
                    endtestdrive(player, player.Vehicle);
                    NAPI.Task.Run(() => { 
                        NAPI.Entity.SetEntityPosition(player, BCore.BizList[player.GetData<int>("MALADOY")].GetPos() + new Vector3(0, 0, 0.5f));
                        Main.Players[player].ExteriorPos = new Vector3();
                        Dimensions.DismissPrivateDimension(player);

                        player.ResetData("IDS");
                        player.ResetData("GP");
                        NAPI.Entity.SetEntityDimension(player, 0);
                        if (player.HasData("ROOMCAR"))
                        {
                            var uveh = player.GetData<Vehicle>("ROOMCAR");
                            uveh.Delete();
                            player.ResetData("ROOMCAR");
                        }
                        Trigger.PlayerEvent(player, "destroyCamera");
                    }, 200);
                }
            }
            catch { }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public static void onPlayerDissonnectedHandler(Player player, DisconnectionType type, string reason)
        {
            try
            {
                if (player.HasData("ROOMCAR"))
                {
                    if (player != null)
                    {
                        var uveh = player.GetData<Vehicle>("ROOMCAR");
                        uveh.Delete();
                        player.ResetData("ROOMCAR");
                    }
                }
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }
        [RemoteEvent("createlveh")]
        public static void createveh(Player player, string name, int color1, int color2, int color3, int x, int y, int z)
        {
            try
            {
                if (!player.HasData("MALADOY")) return;
                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
                }
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(name);
                Vehicle veh = NAPI.Vehicle.CreateVehicle(vh, new Vector3(x, y, z-1), new Vector3(0, 0, 155), 0, 0);
                NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, color1, color2, color3);
                NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, color1, color2, color3);
                NAPI.Entity.SetEntityDimension(veh, player.Dimension);
                //player.SetIntoVehicle(veh, 0);
                Trigger.PlayerEvent(player, "client::sendkilogramsinfoCar", VehicleInventory.GetWeightVeh(name));
                player.SetData("ROOMCAR", veh);
            }
            catch { }
        }

        [RemoteEvent("vehchangecolor")]
        public static void vehchangecolor(Player player, int color1, int color2, int color3)
        {
            try
            {
                if (!player.HasData("MALADOY")) return;
                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    NAPI.Vehicle.SetVehicleCustomSecondaryColor(uveh, color1, color2, color3);
                    NAPI.Vehicle.SetVehicleCustomPrimaryColor(uveh, color1, color2, color3);
                }
            }
            catch { }
        }

        public class AirShops : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-1143.0817, -2863.8562, 18.826023);
            public static Vector3 LookAt = new Vector3(-1146.0817, -2863.8562, 12.826023);
            public static Vector3 CarPoint = new Vector3(-1146.0817, -2863.8562, 12.826023);
            public AirShops(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 25;
                Name = "Воздушный транспорт";
                BlipColor = 4;
                BlipType = 251;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                    if (!Main.Players.ContainsKey(player)) return;
                    player.SetData("IDS", 7);
                    player.SetData("MALADOY", ID);
                    EnterAutoShop(player, ProductsList[7], CamPosition, LookAt, CarPoint);
            }
        }

        public class AutoShopTrucks : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-943.2822, -2098.955, 12.3664);
            public static Vector3 LookAt = new Vector3(-942.2006, -2087.55, 8.179263);
            public static Vector3 CarPoint = new Vector3(-942.2006, -2087.55, 8.179263);
            public AutoShopTrucks(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 23;
                Name = "Грузовой";
                BlipColor = 4;
                BlipType = 67;
                Range = 2f;

                CreateStuff();
                UpdateLabel();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 5);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[5], CamPosition, LookAt, CarPoint);
            }
        }

        public class AutoShopDonate : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-1506.6466, -2985.047, -82.200745);
            public static Vector3 LookAt = new Vector3(-1505.4131, -2992.079, -83.19269);
            public static Vector3 CarPoint = new Vector3(-1506.4131, -2992.079, -82.19269);
            public AutoShopDonate(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 22;
                Name = "Автосалон Эксклюзивного транспорта";
                BlipColor = 4;
                BlipType = 820;
                Range = 2f;

                CreateStuff();
                UpdateLabel();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 4);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[4], CamPosition, LookAt, CarPoint, false);
            }
        }

        public class AutoShopMoto : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-1506.6466, -2985.047, -82.200745);
            public static Vector3 LookAt = new Vector3(-1505.4131, -2992.079, -83.19269);
            public static Vector3 CarPoint = new Vector3(-1506.4131, -2992.079, -82.19269);
            public AutoShopMoto(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 5;
                Name = "Мотосалон";
                BlipColor = 4;
                BlipType = 522;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 3);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[3], CamPosition, LookAt, CarPoint);
            }
        }
        public class AutoShopMiddle : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-1506.6466, -2985.047, -82.200745);
            public static Vector3 LookAt = new Vector3(-1505.4131, -2992.079, -83.19269);
            public static Vector3 CarPoint = new Vector3(-1506.4131, -2992.079, -82.19269);
            public AutoShopMiddle(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 4;
                Name = "Автосалон Люксовых автомобилей";
                BlipColor = 4;
                BlipType = 669;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 2);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[2], CamPosition, LookAt, CarPoint);
            }
        }
        public class AutoShopEkonom: BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-1506.6466, -2985.047, -82.200745);
            public static Vector3 LookAt = new Vector3(-1505.4131, -2992.079, -83.19269);
            public static Vector3 CarPoint = new Vector3(-1506.4131, -2992.079, -82.19269);
            public AutoShopEkonom(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 3;
                Name = "Автосалон Экономных авто";
                BlipColor = 4;
                BlipType = 530;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 1);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[1], CamPosition, LookAt, CarPoint);
            }
        }
        public class AutoShopPremium : BCore.Bizness
        {
            public static Vector3 CamPosition = new Vector3(-1506.6466, -2985.047, -82.200745);
            public static Vector3 LookAt = new Vector3(-1505.4131, -2992.079, -83.19269);
            public static Vector3 CarPoint = new Vector3(-1506.4131, -2992.079, -82.19269);
            public AutoShopPremium(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 2;
                Name = "Автосалон Премиальных авто";
                BlipColor = 4;
                BlipType = 825;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                player.SetData("IDS", 0);
                player.SetData("MALADOY", ID);
                EnterAutoShop(player, ProductsList[0], CamPosition, LookAt, CarPoint);
            }
        }

        [RemoteEvent("carroomBuy")]
        public static void Buy(Player player, string vName, string color, bool card)
        {
            try
            {
                if (!player.HasData("IDS")) return;
                else if (player.GetData<int>("IDS") == 7)
                {
                    AutoShopI.BuyAir(player, vName, color);
                    return;
                }

                AutoShopI.BuyF(player, vName, color, card);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("carroomCancel")]
        public static void Cancel(Player player)
        {
            try
            {
                AutoShopI.CancelF(player);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        // OFF TOP //
        public static void EnterAutoShop(Player player, Dictionary<string, int> produ, Vector3 campos, Vector3 angle, Vector3 car, bool donate = true)
        {
            try
            {
                if (NAPI.Player.IsPlayerInAnyVehicle(player)) return;
                Trigger.PlayerEvent(player, "screenFadeOut", 500);
                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        Main.Players[player].ExteriorPos = player.Position;
                        NAPI.Entity.SetEntityPosition(player, new Vector3(campos.X, campos.Y - 2, campos.Z - 1));
                        Random rnd = new Random();
                        uint dim = (uint)Convert.ToUInt32(rnd.Next(230, 500));
                        NAPI.Entity.SetEntityDimension(player, dim);
                        player.SetData("INTERACTIONCHECK", 0);
                        Trigger.PlayerEvent(player, "carRoom", campos.X, campos.Y, campos.Z, angle.X, angle.Y, angle.Z);

                        List<string> vehnames = new List<string> { };
                        var prices = new List<int>();
                        foreach (var p in produ)
                        {
                            prices.Add(produ != ProductsList[4] ? (int)Math.Floor(p.Value * 6f) : p.Value); vehnames.Add(p.Key);
                        }
                        Trigger.PlayerEvent(player, "openAuto", JsonConvert.SerializeObject(vehnames), JsonConvert.SerializeObject(prices), car.X, car.Y, car.Z, Businesses.BCore.BizList[player.GetData<int>("MALADOY")].GetName());
                    }
                }, 400);
            }
            catch { }
        }

        public static void BuyAir(Player player, string vName, string color)
        {
            try
            {
                int carroom = player.GetData<int>("IDS");
                Dictionary<string, int> products = ProductsList[carroom];
                NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos);

                Trigger.PlayerEvent(player, "destroyCamera");
                NAPI.Entity.SetEntityDimension(player, 0);
                Main.Players[player].ExteriorPos = new Vector3();
                player.ResetData("IDS");

                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
                }

                if (AirVehicles.getAllAirVehicles(player.Name).Count >= 3)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нет мест в аэропорту!", 3000);
                    return;
                }


                int cost = (int)Math.Floor(products[vName] * 6f);
                if (Main.Players[player].Money < cost)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -cost);

                GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", cost, $"buyCar({vName})");

                AirVehicles.Create(player.Name, vName, carColors[color]);

                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили {Utilis.VehiclesName.GetRealVehicleName(vName)}", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени транспорт будет доставен в аэропорт", 5000);
            }
            catch { }

        }

        public static void BuyF(Player player, string vName, string color, bool card)
        {
            try
            {
                int carroom = player.GetData<int>("IDS");
                Dictionary<string, int> products = ProductsList[carroom];
                NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos);

                Trigger.PlayerEvent(player, "destroyCamera");
                NAPI.Entity.SetEntityDimension(player, 0);
                Main.Players[player].ExteriorPos = new Vector3();
                //player.ResetData("IDS");

                if (player.HasData("ROOMCAR"))
                {
                    var uveh = player.GetData<Vehicle>("ROOMCAR");
                    uveh.Delete();
                    player.ResetData("ROOMCAR");
                }

                var house = Houses.HouseManager.GetHouse(player, true);

                var apartament = Houses.HouseManager.GetApart(player, true);

                if (house == null)
                {
                    if (apartament != null)
                    {
                        house = apartament;
                    }
                }


                if (house == null && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нельзя иметь больше 1 машины без дома", 3000);
                    return;
                }
                if (house != null)
                {

                    if (house.GarageID == 0 && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет гаража", 3000);
                        return;
                    }
                    var garage = Houses.GarageManager.Garages[house.GarageID];
                    if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.HouseManager.GetMaxCarsInHouse(player))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
                        return;
                    }
                }

                int cost = products[vName];

                if (Main.Players[player].Money < cost)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }

                if (player.GetData<int>("IDS") == 4)
                {
                    if (Convert.ToInt32((int)Main.Accounts[player].RedBucks) < cost)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                        return;
                    }
                    Main.Accounts[player].RedBucks -= cost;
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.CommandText = "update `accounts` set `redbucks`=@redbucks where `login`=@login";
                    cmd.Parameters.AddWithValue("@redbucks", Main.Accounts[player].RedBucks);
                    cmd.Parameters.AddWithValue("@login", Main.Accounts[player].Login);
                    MySQL.Query(cmd);
                }
                else
                {
                    if (card)
                    {
                        if (MoneySystem.Bank.Accounts[ Main.Players[player].Bank].Balance < cost)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                            return;
                        }
                        MoneySystem.Bank.Change(Main.Players[player].Bank, -cost, false);
                    }
                    else
                    {
                        if (Main.Players[player].Money < cost)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                            return;
                        }
                        MoneySystem.Wallet.Change(player, -cost);
                    }
                }

                GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", cost, $"buyCar({vName})");

                var vNumber = VehicleManager.Create(player.Name, vName, carColors[color], carColors[color], carColors[color]);
                var vehdata = VehicleManager.Vehicles[vNumber];
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили {Utilis.VehiclesName.GetRealVehicleName(vehdata.Model)} с номером {vNumber}", 3000);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина стоит на паркове", 5000);
                nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{vNumber}_{VehicleManager.Vehicles[vNumber].KeyNum}"));
                ParkManager.SpawnCarOnAuto(player, player.GetData<int>("IDS"), vNumber);
            }
            catch (Exception e) { Log.Write("Error on buy: " + e.ToString());}
        }

        public static void CancelF(Player player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos + new Vector3(0, 0, 0.5f));
                    Main.Players[player].ExteriorPos = new Vector3();
                    player.ResetData("IDS");
                    NAPI.Entity.SetEntityDimension(player, 0);
                    if (player.HasData("ROOMCAR"))
                    {
                        var uveh = player.GetData<Vehicle>("ROOMCAR");
                        uveh.Delete();
                        player.ResetData("ROOMCAR");
                    }
                    Trigger.PlayerEvent(player, "destroyCamera");
                    Trigger.PlayerEvent(player, "screenFadeIn", 500);
                }, 400);
            }
            catch { }
        }

        [RemoteEvent("carromtestdrive")]
        public static void RemoteEvent_carromtestDrive(Player player, string vName, int color1, int color2, int color3)
        {
            try
            {
                var licensec = Main.Players[player].Licenses;
                if (ProductsList[7].ContainsKey(vName))
                {
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            Trigger.PlayerEvent(player, "destroyCamera");
                        }
                        catch { }
                    }, 500);
                    CancelF(player);
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"На воздушном транспроте запрещён тест драйв", 3000);
                    return;
                }
                NAPI.Entity.SetEntityPosition(player, new Vector3(-843.9716, -172.6021, 40.4));
                Random rnd = new Random();
                uint dim = Convert.ToUInt32(rnd.Next(110, 220));
                VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vName);

                var veh = NAPI.Vehicle.CreateVehicle(vh, new Vector3(-843.9716, -172.6021, 39.4), new Vector3(0, 0, 30), 0, 0);
                veh.Dimension = dim;
                Trigger.PlayerEvent(player, "AGMTestDrive", true);
                NAPI.Vehicle.SetVehicleCustomSecondaryColor(veh, color1, color2, color3);
                NAPI.Vehicle.SetVehicleCustomPrimaryColor(veh, color1, color2, color3);
                VehicleStreaming.SetEngineState(veh, true);
                VehicleStreaming.SetLockStatus(veh, true);

                NAPI.Task.Run(() =>
                {
                    try
                    {
                        if (player != null)
                        {
                            player.SetIntoVehicle(veh, 0);
                            Trigger.PlayerEvent(player, "destroyCamera");
                            NAPI.Task.Run(() =>
                            {
                                if (player != null)
                                {
                                    Trigger.PlayerEvent(player, "screenFadeIn", 500);
                                }
                            }, 500);
                        }
                    }
                    catch { }
                }, 1000);
                player.SetData("VEHTEST", veh);
                player.SetData("TEST_TIMER", Timers.StartOnceTask(60000, () => endtestdrive(player, veh)));

                veh.SetData("ACCESS", "TESTDRIVE");
                veh.SetData("OWNER", player);
                veh.SetSharedData("PETROL", 100);
                NAPI.Vehicle.SetVehicleNumberPlate(veh, "SHOWROOM");
                player.SetData("TESTDRIVESTATE", true);
                NAPI.Entity.SetEntityDimension(player, dim);
                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"У Вас есть 2 минуты на тест драйв!", 3000);
            }
            catch (Exception e) { Log.Write("CarRoomTestDrive: " + e.Message, nLog.Type.Error); }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void onPlayerExitVehicleHandler(Player player, Vehicle vehicle)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "TESTDRIVE")
                        {
                            if (player.GetData<bool>("TESTDRIVESTATE") == false) return;
                            endtestdrive(player, player.GetData<Vehicle>("VEHTEST"));
                            Trigger.PlayerEvent(player, "AGMTestDrive", false);
                        }
                    }
                }, 400);
            }
            catch (Exception e) { Log.Write("PlayerExitVehicle: " + e.Message, nLog.Type.Error); }
        }

        private static void endtestdrive(Player player, Entity veh)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    if (veh == null) return;
                    if (!player.HasData("IDS")) return;
                    if (player.GetData<bool>("TESTDRIVESTATE") == false) return;
                    if (player.HasData("TEST_TIMER"))
                    {
                        Timers.Stop(player.GetData<string>("TEST_TIMER"));
                        player.ResetData("TEST_TIMER");
                    }
                    player.ResetData("VEHTEST");
                    player.SetData("TESTDRIVESTATE", false);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваш тест драйв закончился!", 3000);
                    NAPI.Entity.DeleteEntity(veh);
                    player.SetData("INTERACTIONCHECK", 0);
                    NAPI.Entity.SetEntityPosition(player, Main.Players[player].ExteriorPos);
                    BCore.BizList[player.GetData<int>("MALADOY")].InteractPress(player);
                    NAPI.Entity.SetEntityDimension(player, 0);
                    Trigger.PlayerEvent(player, "AGMTestDrive", false);
                    return;
                }
                catch (Exception e) { Log.Write("TEST DRIVE END: " + e.Message, nLog.Type.Error); }
            });
        }

    }

}

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
using NeptuneEVO.Houses;
using static NeptuneEVO.Core.VehicleManager;
using System.Security.Cryptography;
using static NeptuneEVO.Houses.GarageManager;
using NeptuneEVO.Businesses;

// code: koltr

namespace NeptuneEVO.Core
{
    class ParkManager : Script
    {

        private static nLog Log = new nLog("ParkManager");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var colsp = new ParkBuy(new Vector3(4.42, -1073.18, 38.15));
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"PARKMAMAGER\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        static List<Vector3> ParkList = new List<Vector3>()
        {
            new Vector3(24.95, -1060.78, 38.15), // 1
            new Vector3(23.96, -1063.61, 38.15), // 2
            new Vector3(22.90, -1066.22, 38.15), // 3
            new Vector3(21.75, -1068.68, 38.15), // 4
        };
        public static void BuyParkPlace(Player player)
        {
            var costcar = 5;
            if (Main.Players[player].Money < costcar)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств, цена: [{costcar}$]", 3000);
                return;
            }

            if (Houses.HouseManager.GetHouse(player, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть дом!", 3000);
                return;
            }
            if (Houses.HouseManager.GetApart(player, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас уже есть квартира!", 3000);
                return;
            }
            var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());
            var vehicle = "";
            foreach (var num in targetVehicles)
            {
                vehicle = num;
                break;
            }
            if (vehicle == "" || vehicle == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет машины", 3000);
                return;
            }
            foreach (var v in NAPI.Pools.GetAllVehicles())
            {
                if (v.HasData("ACCESS") && (v.GetData<string>("ACCESS") == "PERSONAL" || v.GetData<string>("ACCESS") == "INPARK") && NAPI.Vehicle.GetVehicleNumberPlate(v) == vehicle)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Машина была уже вызвана ранее!", 3000);
                    return;
                }
            }
            MoneySystem.Wallet.Change(player, -costcar);
            SetCarInFreeParkPlace(player, vehicle);
        }

        public static string FindFirstCarNum(Player player)
        {
            var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());
            var vehicle = "";
            foreach (string num in targetVehicles)
            {
                vehicle = num;
                break;
            }
            return vehicle;
        }
        public static void interactionPressed(Player player, int id)
        {
            try
            {
                switch (id)
                {
                    case 52:
                        BuyParkPlace(player);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"PARK_INTERACTION\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static Dictionary<int, List<Vector3>> BizVector = new Dictionary<int, List<Vector3>>
        {
            {3, new List<Vector3> { new Vector3(-899.6517, -232.3591, 38.78878), new Vector3(-899.6517, -232.3591, 38.78878), new Vector3(-899.6517, -232.3591, 38.78878), new Vector3(-899.6517, -232.3591, 38.78878) }  },
            {2, new List<Vector3> { new Vector3(-809.2053, -227.3651, 37.20964), new Vector3(-809.2053, -227.3651, 37.20964), new Vector3(-809.2053, -227.3651, 37.20964), new Vector3(-809.2053, -227.3651, 37.20964) }  },
            {4, new List<Vector3> { new Vector3(-37.76995, -1101.887, 25.30233), new Vector3(-37.76995, -1101.887, 25.30233), new Vector3(-37.76995, -1101.887, 25.30233), new Vector3(-37.76995, -1101.887, 25.30233) }  },
            {5, new List<Vector3> { new Vector3(264.9019, -1159.411, 28.10543), new Vector3(264.9019, -1159.411, 28.10543), new Vector3(264.9019, -1159.411, 28.10543), new Vector3(264.9019, -1159.411, 28.10543) }  },
        };

        private static Dictionary<int, List<Vector3>> BizAngle = new Dictionary<int, List<Vector3>>
        {
            {3, new List<Vector3> { new Vector3(0, 0, 150.1884), new Vector3(0, 0, 150.1884), new Vector3(0, 0, 150.1884), new Vector3(0, 0, 150.1884) }  },
            {2, new List<Vector3> { new Vector3(0.56777227, 2.295356, 27.989887), new Vector3(0.56777227, 2.295356, 27.989887), new Vector3(0.56777227, 2.295356, 27.989887), new Vector3(0.56777227, 2.295356, 27.989887) }  },
            {4, new List<Vector3> { new Vector3(0, 0, 336.5976), new Vector3(0, 0, 336.5976), new Vector3(0, 0, 336.5976), new Vector3(0, 0, 336.5976) }  },
            {5, new List<Vector3> { new Vector3(0, 0, 88.00644), new Vector3(0, 0, 88.00644), new Vector3(0, 0, 88.00644), new Vector3(0, 0, 88.00644) }  },
        };

        public static void SpawnCarOnAuto(Player player, int ids, string number)
        {
            var table = BizVector[ids];
            var table2 = BizAngle[ids];

            var rnd = new Random();
            var count = rnd.Next(1, table.Count);
            var vehdata = VehicleManager.Vehicles[number];
            VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vehdata.Model);
            var veh = NAPI.Vehicle.CreateVehicle(vh, table[count], table2[count], 0, 0);

            var house = Houses.HouseManager.GetHouse(player, true);

            var apartament = Houses.HouseManager.GetApart(player, true);

            if (house == null)
            {
                if (apartament != null)
                {
                    house = apartament;
                }
            }

            if (house != null)
            {
                if (house.GarageID != 0)
                {
                    Houses.Garage Garage = Houses.GarageManager.Garages[house.GarageID];
                    Garage.SetOutVehicle(number, veh);
                }
            }

            
            VehicleStreaming.SetLockStatus(veh, true);
            vehdata.Holder = player.Name;
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("ITEMS", vehdata.Items);
            veh.SetData("OWNER", player);
            veh.SetSharedData("PETROL", vehdata.Fuel);

            VehicleStreaming.SetEngineState(veh, true);

            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);
            VehicleManager.ApplyCustomization(veh);

        }
        public static void SetCarInFreeParkPlace(Player player, string number)
        {
            var rnd = new Random();
            var id = rnd.Next(1, ParkList.Count);
            var vehdata = VehicleManager.Vehicles[number];
            VehicleHash vh = (VehicleHash)NAPI.Util.GetHashKey(vehdata.Model);
            var veh = NAPI.Vehicle.CreateVehicle(vh, ParkList[id], new Vector3(0, 0, 70), 0, 0);

            VehicleStreaming.SetEngineState(veh, false);
            VehicleStreaming.SetLockStatus(veh, true);
            vehdata.Holder = player.Name;
            veh.SetData("ACCESS", "PERSONAL");
            veh.SetData("ITEMS", vehdata.Items);
            veh.SetData("OWNER", player);
            veh.SetSharedData("PETROL", vehdata.Fuel);
            NAPI.Vehicle.SetVehicleNumberPlate(veh, number);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Парковочное место: №{id}", 3000);
            VehicleManager.ApplyCustomization(veh);

        }

        internal class ParkBuy
        {
            public Vector3 Position { get; }

            [JsonIgnore]
            private Blip blip = null;
            [JsonIgnore]
            private ColShape shape = null;
            [JsonIgnore]
            private TextLabel label = null;
            [JsonIgnore]
            private Marker marker = null;

            public ParkBuy(Vector3 pos)
            {
                Position = pos;
                blip = NAPI.Blip.CreateBlip(255, pos, 1, 4, "Парковка", 225, 0, true);
                shape = NAPI.ColShape.CreateCylinderColShape(pos, 2f, 3, 0);
                shape.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 52);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                shape.OnEntityExitColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 0);
                    }
                    catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
                };
                label = NAPI.TextLabel.CreateTextLabel("~w~Cтоянка", new Vector3(pos.X, pos.Y, pos.Z), 20F, 0.5F, 0, new Color(255, 255, 255), true, 0);
                marker = NAPI.Marker.CreateMarker(1, pos - new Vector3(0, 0, 1f), new Vector3(), new Vector3(), 1f, new Color(0, 175, 250, 220), false, 0);
            }
        }

        public static void OpenMenu(Player player)
        {
            Menu menu = new Menu("parkcars", false, false);
            menu.Callback = callback_cars;
            menu.SetBackGround("../images/phone/pages/gps.png");

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = "Парковка";
            menu.Add(menuItem);

            foreach (var v in VehicleManager.getAllPlayerVehicles(player.Name))
            {
                var num = v;
                if (num.Contains("TARNSIT"))
                    num = "TRANSIT";
                menuItem = new Menu.Item(v, Menu.MenuItem.Button);
                menuItem.Text = $"{Utilis.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[v].Model)} <br> Номер: {num} <br> Пробег {Convert.ToInt32( VehicleManager.Vehicles[v].Sell)}";
                menu.Add(menuItem);
                break;
            }

            // menuItem = new Menu.Item("close", Menu.MenuItem.Button);
            // menuItem.Text = "Закрыть";
            // menu.Add(menuItem);

            menuItem = new Menu.Item("back", Menu.MenuItem.closeBtn); // полоска закрытия
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }

        private static void callback_cars(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    MenuManager.Close(player);
                    if (item.ID == "back")
                    {
                        MenuManager.Close(player);
                        Main.OpenPlayerMenu(player).Wait();
                        return;
                    }
                    OpenSelectedCarMenu(player, item.ID);
                }
                catch (Exception e) { Log.Write("callback_cars: " + e.Message + e.Message, nLog.Type.Error); }
            });
        }

        public static void OpenSelectedCarMenu(Player player, string number)
        {
            Menu menu = new Menu("selectedcar", false, false);
            menu.Callback = callback_selectedcar;
            menu.SetBackGround("../images/phone/pages/gps.png");

            var vData = VehicleManager.Vehicles[number];

            Menu.Item menuItem = new Menu.Item("header", Menu.MenuItem.Header);
            menuItem.Text = number;
            menu.Add(menuItem);

            menuItem = new Menu.Item("model", Menu.MenuItem.Card);
            menuItem.Text = Utilis.VehiclesName.GetRealVehicleName(vData.Model);
            menu.Add(menuItem);

            var vClass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(vData.Model));

            menuItem = new Menu.Item("repair", Menu.MenuItem.Button);
            menuItem.Text = $"Восстановить {VehicleManager.VehicleRepairPrice[vClass]}$";
            menu.Add(menuItem);

            menuItem = new Menu.Item("key", Menu.MenuItem.Button);
            menuItem.Text = $"Получить дубликат ключа";
            menu.Add(menuItem);

            menuItem = new Menu.Item("changekey", Menu.MenuItem.Button);
            menuItem.Text = $"Сменить замки";
            menu.Add(menuItem);

            menuItem = new Menu.Item("evac", Menu.MenuItem.Button);
            menuItem.Text = $"Эвакуировать машину";
            menu.Add(menuItem);


            var price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

            menuItem = new Menu.Item("sell", Menu.MenuItem.Button);
            menuItem.Text = $"Продать ({price}$)";
            menu.Add(menuItem);

            menuItem = new Menu.Item("close", Menu.MenuItem.closeBtn);
            menuItem.Text = "";
            menu.Add(menuItem);

            menu.Open(player);
        }
        private static void callback_selectedcar(Player player, Menu menu, Menu.Item item, string eventName, dynamic data)
        {
            try
            {
                MenuManager.Close(player);
                switch (item.ID)
                {
                    case "sell":
                        player.SetData("CARSELLGOV", menu.Items[0].Text);
                        VehicleManager.VehicleData vData = VehicleManager.Vehicles[menu.Items[0].Text];
                        var price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

                        MenuManager.Close(player);
                        Trigger.PlayerEvent(player, "openDialog", "CAR_SELL_TOGOV", $"Вы действительно хотите продать государству {Utilis.VehiclesName.GetRealVehicleName(vData.Model)} ({menu.Items[0].Text}) за ${price}?");
                        return;
                    case "repair":
                        vData = VehicleManager.Vehicles[menu.Items[0].Text];
                        if (vData.Health > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не нуждается в восстановлении", 3000);
                            return;
                        }

                        var vClass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(vData.Model));
                        if (!MoneySystem.Wallet.Change(player, -VehicleManager.VehicleRepairPrice[vClass]))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно средств", 3000);
                            return;
                        }
                        vData.Items = new List<nItem>();
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", VehicleManager.VehicleRepairPrice[vClass], $"carRepair({vData.Model})");
                        vData.Health = 1000;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы восстановили {Utilis.VehiclesName.GetRealVehicleName(vData.Model)} ({menu.Items[0].Text})", 3000);
                        return;
                    case "evac":
                        if (!Main.Players.ContainsKey(player)) return;

                        var number = menu.Items[0].Text;

                        if (Main.Players[player].Money < 15)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {15 - Main.Players[player].Money}$)", 3000);
                            return;
                        }
                        var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());
                        var vehicle = "";
                        foreach (var num in targetVehicles)
                        {
                            vehicle = num;
                            break;
                        }
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v != null && v.HasData("ACCESS") && (v.GetData<string>("ACCESS") == "PERSONAL" || v.GetData<string>("ACCESS") == "INPARK") && v.NumberPlate == vehicle)
                            {
                                NAPI.Task.Run(() =>
                                {
                                   try 
                                   { 
                                        var veh = v;
                                        if (veh == null) return;
                                        VehicleManager.Vehicles[number].Fuel = (!veh.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[veh.Class] : veh.GetSharedData<int>("PETROL");
                                        veh.Delete();

                                        MoneySystem.Wallet.Change(player, -15);
                                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 15, $"carEvac");
                                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была отогнана на стоянку", 3000);
                                    }
                                   catch { }
                                });
                                break;
                                }
                            }
                        return;
                    case "key":
                        if (!Main.Players.ContainsKey(player)) return;

                        var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                        if (tryAdd == -1 || tryAdd > 0)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                            return;
                        }

                        nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{menu.Items[0].Text}_{VehicleManager.Vehicles[menu.Items[0].Text].KeyNum}"));
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили ключ от машины с номером {menu.Items[0].Text}", 3000);
                        return;
                    case "changekey":
                        if (!Main.Players.ContainsKey(player)) return;

                        if (!MoneySystem.Wallet.Change(player, -100))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Смена замков стоит $100", 3000);
                            return;
                        }

                        VehicleManager.Vehicles[menu.Items[0].Text].KeyNum++;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сменили замки на машине {menu.Items[0].Text}. Теперь старые ключи не могут быть использованы", 3000);
                        return;
                    case "close":
                        OpenMenu(player);
                        return;
                }
            }
            catch (Exception e) { Log.Write(e.ToString()); }
        }

    }
}

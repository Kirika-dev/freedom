using GTANetworkAPI;
using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using System.Linq;

namespace NeptuneEVO.Businesses
{
    class TuningI : Script
    {
        public static int CostForTuning = 5;

        private static nLog Log = new nLog("TUNING");

        public class Tuning : BCore.Bizness
        {

            public Tuning(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 12;
                Name = "Los Santos Customs";
                BlipColor = 45;
                BlipType = 72;
                Range = 5f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                if (!player.IsInVehicle || !player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в личной машине", 3000);
                    return;
                }
                if (player.Vehicle.Class == 13)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Велосипед не может быть затюнингован", 3000);
                    return;
                }
                /*if (player.Vehicle.Class == 8)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Тюнинг пока что недоступен для мотоциклов :( Скоро исправим", 3000);
                    return;
                }*/
                if(player.Vehicle.DisplayName == "pounder2")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта машина не может быть затюнингована", 3000);
                    return;
                }
                var vdata = VehicleManager.Vehicles[player.Vehicle.NumberPlate];
                /*if (!TuningS.ContainsKey(vdata.Model))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент для Вашего т/с тюнинг не доступен", 3000);
                    return;
                }*/

                //var occupants = VehicleManager.GetVehicleOccupants(player.Vehicle);
                //for(int i = occupants.Count; i > -1; i--)
                //    if (occupants[i] != player)
                //    VehicleManager.WarpPlayerOutOfVehicle(occupants[i]);
                //
                //Trigger.PlayerEvent(player, "tuningSeatsCheck");

                var occupants = VehicleManager.GetVehicleOccupants(player.Vehicle);
                foreach (var p in occupants)
                {
                    if (p != player)
                        VehicleManager.WarpPlayerOutOfVehicle(p);
                }

                Trigger.PlayerEvent(player, "tuningSeatsCheck");

            }

            public static int GetModelPrice(List<Dictionary<string, int>> table, string model)
            {
                int result = -1;
                foreach (Dictionary<string, int> vit in table)
                    foreach (KeyValuePair<string, int> fat in vit)
                        if (fat.Key == model)
                            result = fat.Value;
                return result;
            }
                
        }

        [RemoteEvent("tuningSeatsCheck")]
        public static void RemoteEvent_tuningSeatsCheck(Player player)
        {
            try
            {
                if (!player.IsInVehicle || !player.Vehicle.HasData("ACCESS") || player.Vehicle.GetData<string>("ACCESS") != "PERSONAL")
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в личной машине", 3000);
                    return;
                }
                if (player.Vehicle.Class == 13)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Велосипед не может быть затюнингован", 3000);
                    return;
                }
                /*if (player.Vehicle.Class == 8)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Тюнинг пока что недоступен для мотоциклов :( Скоро исправим", 3000);
                    return;
                }*/
                var vdata = VehicleManager.Vehicles[player.Vehicle.NumberPlate];
                /*if (!TuningS.ContainsKey(vdata.Model))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент для Вашего т/с тюнинг не доступен", 3000);
                    return;
                }*/

                if (player.GetData<int>("BIZ_ID") == -1) return;
                if (player.HasData("FOLLOWING"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                    return;
                }

                Main.Players[player].TuningShop = player.GetData<int>("BIZ_ID");

                var veh = player.Vehicle;
                var dim = Dimensions.RequestPrivateDimension(player);
                NAPI.Entity.SetEntityDimension(veh, dim);
                NAPI.Entity.SetEntityDimension(player, dim);

                player.SetIntoVehicle(veh, 0);

                NAPI.Entity.SetEntityPosition(veh, new Vector3(-337.7784, -136.5316, 38.7032));
                NAPI.Entity.SetEntityRotation(veh, new Vector3(0.0, 0.0, 148.9986));

                var modelPrice = Tuning.GetModelPrice(AutoShopI.ProductsList, VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model);
                var modelPriceMod = (modelPrice < 150000) ? 1 : 2;

                //NAPI.Entity.SetEntityVelocity(veh, new Vector3(0, 0, 0));

                Trigger.PlayerEvent(player, "openTun", CostForTuning, VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model, modelPriceMod, JsonConvert.SerializeObject(VehicleManager.Vehicles[player.Vehicle.NumberPlate].Components));
            }
            catch (Exception e) { Log.Write("tuningSeatsCheck: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("exitTuning")]
        public static void Exit(Player player)
        {
            try
            {
                int bizID = Main.Players[player].TuningShop;

                var veh = player.Vehicle;
                NAPI.Entity.SetEntityDimension(veh, 0);
                NAPI.Entity.SetEntityDimension(player, 0);

                player.SetIntoVehicle(veh, 0);

                NAPI.Entity.SetEntityPosition(veh, BCore.BizList[bizID].GetPos() + new Vector3(0, 0, 1));
                VehicleManager.ApplyCustomization(veh);
                Dimensions.DismissPrivateDimension(player);
                Main.Players[player].TuningShop = -1;
            }
            catch (Exception e) { Log.Write("ExitTuning: " + e.Message, nLog.Type.Error); }
        }

        static Dictionary<int, int> ArmorPrice = new Dictionary<int, int>{
	        {-1, 120000},
	        {0, 220000},
	        {1, 320000},
	        {2, 420000},
	        {3, 520000},
	        {4, 620000},
        };

        [RemoteEvent("buyTuning")]
        public static void Buy(Player player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;

                int bizID = Main.Players[player].TuningShop;

                var cat = Convert.ToInt32(arguments[0].ToString());
                var id = Convert.ToInt32(arguments[1].ToString());

                var wheelsType = -1;
                var r = 0;
                var g = 0;
                var b = 0;

                if (cat == 19)
                    wheelsType = Convert.ToInt32(arguments[2].ToString());
                else if (cat == 20)
                {
                    r = Convert.ToInt32(arguments[2].ToString());
                    g = Convert.ToInt32(arguments[3].ToString());
                    b = Convert.ToInt32(arguments[4].ToString());
                }

                var vehModel = VehicleManager.Vehicles[player.Vehicle.NumberPlate].Model;

                var modelPrice = Tuning.GetModelPrice(AutoShopI.ProductsList, vehModel);
                var modelPriceMod = (modelPrice < 150000) ? 1 : 2;

                var price = 0;
                if (cat <= 9)
                    price = Convert.ToInt32(TuningPrices[cat][id.ToString()] * modelPriceMod * CostForTuning / 100.0);
                else if (cat <= 18)
					price = Convert.ToInt32(TuningPrices[cat][id.ToString()] * modelPriceMod * CostForTuning / 100.0);
                else if (cat == 19)
                    price = Convert.ToInt32(TuningWheels[wheelsType][id] * CostForTuning / 100.0);
                else if (cat == 21)
                    price = 5000;
				else if (cat == 26)
                    price = ArmorPrice[id];
                else
                    price = Convert.ToInt32(5000 * CostForTuning / 100.0);
                 
                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вам не хватает ещё {price - Main.Players[player].Money}$ для покупки этой модификации", 3000);
                    Trigger.PlayerEvent(player, "tunBuySuccess", -2);
                    return;
                }

                var amount = Convert.ToInt32(price * 0.75 / 2000);
                if (amount <= 0) amount = 1;

                //GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", price, $"buyTuning({player.Vehicle.NumberPlate},{cat},{id})");
                MoneySystem.Wallet.Change(player, -price);
                Trigger.PlayerEvent(player, "tunBuySuccess", id);

                var number = player.Vehicle.NumberPlate;

                switch (cat)
                {
                    case 0:
                        VehicleManager.Vehicles[number].Components.Muffler = id;
                        break;
                    case 1:
                        VehicleManager.Vehicles[number].Components.SideSkirt = id;
                        break;
                    case 2:
                        VehicleManager.Vehicles[number].Components.Hood = id;
                        break;
                    case 3:
                        VehicleManager.Vehicles[number].Components.Spoiler = id;
                        break;
                    case 4:
                        VehicleManager.Vehicles[number].Components.Lattice = id;
                        break;
                    case 5:
                        VehicleManager.Vehicles[number].Components.Wings = id;
                        break;
                    case 6:
                        VehicleManager.Vehicles[number].Components.Roof = id;
                        break;
                    case 7:
                        VehicleManager.Vehicles[number].Components.Vinyls = id;
                        break;
                    case 8:
                        VehicleManager.Vehicles[number].Components.FrontBumper = id;
                        break;
                    case 9:
                        VehicleManager.Vehicles[number].Components.RearBumper = id;
                        break;
                    case 10:
                        VehicleManager.Vehicles[number].Components.Engine = id;
                        break;
                    case 11:
                        VehicleManager.Vehicles[number].Components.Turbo = id;
                        break;
                    case 12:
                        VehicleManager.Vehicles[number].Components.Horn = id;
                        break;
                    case 13:
                        VehicleManager.Vehicles[number].Components.Transmission = id;
                        break;
                    case 14:
                        VehicleManager.Vehicles[number].Components.WindowTint = id;
                        break;
                    case 15:
                        VehicleManager.Vehicles[number].Components.Suspension = id;
                        break;
                    case 16:
                        VehicleManager.Vehicles[number].Components.Brakes = id;
                        break;
                    case 17:
                        VehicleManager.Vehicles[number].Components.Headlights = id;
                        player.Vehicle.SetSharedData("hlcolor", id);
                        Trigger.PlayerEvent(player, "VehStream_SetVehicleHeadLightColor", player.Vehicle.Handle, id);
                        break;
                    case 18:
                        VehicleManager.Vehicles[number].Components.NumberPlate = id;
                        break;
                    case 19:
                        VehicleManager.Vehicles[number].Components.Wheels = id;
                        VehicleManager.Vehicles[number].Components.WheelsType = wheelsType;
                        break;
                    case 20:
                        if (id == 0)
                            VehicleManager.Vehicles[number].Components.PrimColor = new Color(r, g, b);
                        else if (id == 1)
                            VehicleManager.Vehicles[number].Components.SecColor = new Color(r, g, b);
                        else if (id == 2)
                            VehicleManager.Vehicles[number].Components.NeonColor = new Color(r, g, b);
                        else if (id == 3)
                            VehicleManager.Vehicles[number].Components.NeonColor = new Color(0, 0, 0, 0);
                        break;
                    case 21:
                        VehicleManager.Vehicles[number].Components.PrimModColor = id;
                        VehicleManager.Vehicles[number].Components.SecModColor = id;
                        break;
					case 26:
                        VehicleManager.Vehicles[number].Components.Armor = id;
                        break;
                }

                VehicleManager.Save(number);
                VehicleManager.ApplyCustomization(player.Vehicle);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы купили и установили данную модификацию", 3000);
                Trigger.PlayerEvent(player, "tuningUpd", JsonConvert.SerializeObject(VehicleManager.Vehicles[number].Components));
            }
            catch (Exception e) { Log.Write("buyTuning: " + e.Message, nLog.Type.Error); }
        }



        public static Dictionary<int, int> ColorTypes = new Dictionary<int, int> { 
            { 1, 500 }, 
        };


        public static Dictionary<int, Dictionary<string, int>> TuningPrices = new Dictionary<int, Dictionary<string, int>>()
        {
            { 0, new Dictionary<string, int>() { // Глушитель
                { "-1", 1000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
                { "8", 90000 },
                { "9", 90000 },
                { "10", 90000 },
                { "11", 90000 },
                { "12", 90000 },
                { "13", 90000 },
                { "14", 90000 },
                { "15", 90000 },
            }},
             { 1, new Dictionary<string, int>() { //Пороги
                { "-1", 1000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
                { "8", 90000 },
                { "9", 90000 },
                { "10", 90000 },
            }},
              { 2, new Dictionary<string, int>() { // Капот
                { "-1", 5000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
                { "8", 90000 },
                { "9", 90000 },
                { "10", 90000 },
                { "11", 90000 },
                { "12", 90000 },
                { "13", 90000 },
                { "14", 90000 },
                { "15", 90000 },
                { "16", 90000 },
                { "17", 90000 },
                { "18", 90000 },
                { "19", 90000 },
                { "20", 90000 },
            }},
             { 3, new Dictionary<string, int>() { //Спойлер
                { "-1", 5000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
                { "8", 90000 },
                { "9", 90000 },
                { "10", 90000 },
                { "11", 90000 },
                { "12", 90000 },
                { "13", 90000 },
                { "14", 90000 },
                { "15", 90000 },
                { "16", 90000 },
                { "17", 90000 },
                { "18", 90000 },
                { "19", 90000 },
                { "20", 90000 },
            }},
             { 4, new Dictionary<string, int>() { // Решетка
                { "-1", 5000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
                { "8", 90000 },
                { "9", 90000 },
                { "10", 90000 },
                { "11", 90000 },
                { "12", 90000 },
                { "13", 90000 },
                { "14", 90000 },
                { "15", 90000 },
                { "16", 90000 },
                { "17", 90000 },
                { "18", 90000 },
                { "19", 90000 },
                { "20", 90000 },
            }},
            { 5, new Dictionary<string, int>() { // Разширение
                { "-1", 5000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
                { "8", 90000 },
                { "9", 90000 },
                { "10", 90000 },
                { "11", 90000 },
                { "12", 90000 },
                { "13", 90000 },
                { "14", 90000 },
                { "15", 90000 },
                { "16", 90000 },
                { "17", 90000 },
                { "18", 90000 },
                { "19", 90000 },
                { "20", 90000 },
            }},
             { 6, new Dictionary<string, int>() { // Крыша
                { "-1", 5000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
            }},
             { 7, new Dictionary<string, int>() { //Винил
                { "-1", 5000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
                { "8", 90000 },
                { "9", 90000 },
                { "10", 90000 },
                { "11", 90000 },
                { "12", 90000 },
                { "13", 90000 },
                { "14", 90000 },
                { "15", 90000 },
                { "16", 90000 },
                { "17", 90000 },
                { "18", 90000 },
                { "19", 90000 },
                { "20", 90000 },
            }},
            { 8, new Dictionary<string, int>() { // Пер Бампер
                { "-1", 5000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
                { "8", 90000 },
                { "9", 90000 },
                { "10", 90000 },
                { "11", 90000 },
                { "12", 90000 },
                { "13", 90000 },
                { "14", 90000 },
                { "15", 90000 },
                { "16", 90000 },
                { "17", 90000 },
                { "18", 90000 },
                { "19", 90000 },
                { "20", 90000 },
            }},
             { 9, new Dictionary<string, int>() { // Зад бамп
                { "-1", 5000 },
                { "0", 90000 },
                { "1", 90000 },
                { "2", 90000 },
                { "3", 90000 },
                { "4", 90000 },
                { "5", 90000 },
                { "6", 90000 },
                { "7", 90000 },
                { "8", 90000 },
                { "9", 90000 },
                { "10", 90000 },
                { "11", 90000 },
                { "12", 90000 },
                { "13", 90000 },
                { "14", 90000 },
                { "15", 90000 },
                { "16", 90000 },
                { "17", 90000 },
                { "18", 90000 },
                { "19", 90000 },
                { "20", 90000 },
            }},
            { 10, new Dictionary<string, int>() { // engine_menu
                {"-1", 50000 },
                {"0", 80000 },
                {"1", 100000 },
                {"2", 120000 },
                {"3", 150000 },
            }},
            { 11, new Dictionary<string, int>() { // turbo_menu
                {"-1", 50000 },
                {"0", 200000 },
            }},
            { 12, new Dictionary<string, int>() { // horn_menu
                { "-1", 5000 },
                { "0", 7000 },
                { "1", 8000 },
                { "2", 10000 },
                { "3", 10000 },
                { "4", 10000 },
                { "5", 10000 },
                { "6", 10000 },
                { "7", 10000 },
                { "8", 10000 },
                { "9", 10000 },
                { "10", 10000 },
                { "11", 10000 },
                { "12", 10000 },
                { "13", 10000 },
                { "14", 10000 },
                { "15", 10000 },
                { "16", 10000 },
                { "17", 10000 },
                { "18", 10000 },
                { "19", 10000 },
                { "20", 10000 },
                { "21", 10000 },
                { "22", 10000 },
                { "23", 10000 },
                { "24", 10000 },
                { "25", 10000 },
                { "26", 10000 },
                { "27", 10000 },
                { "28", 10000 },
                { "29", 10000 },
                { "30", 10000 },
                { "31", 10000 },
                { "32", 10000 },
                { "33", 10000 },
                { "34", 10000 },
            }},
            { 13, new Dictionary<string, int>() { // transmission_menu
                {"-1", 50000 },
                {"0", 60000 },
                {"1", 105000 },
                {"2", 120000 },
            }},
            { 14, new Dictionary<string, int>() { // glasses_menu
                {"0", 20000 },
                {"3", 30000 },
                {"2", 40000 },
                {"1", 50000 },
            }},
            { 15, new Dictionary<string, int>() { // suspention_menu
                {"-1", 30000 },
                {"0", 45000 },
                {"1", 50000 },
                {"2", 65000 },
                {"3", 80000 },
            }},
            { 16, new Dictionary<string, int>() { // brakes_menu
                {"-1", 50000 },
                {"0", 45000 },
                {"1", 70000 },
                {"2", 105000 },
            }},
            { 17, new Dictionary<string, int>() { // lights_menu
                {"-1", 5000 },
                {"0", 50000 },
                {"1", 50000 },
                {"2", 50000 },
                {"3", 50000 },
                {"4", 50000 },
                {"5", 50000 },
                {"6", 50000 },
                {"7", 50000 },
                {"8", 50000 },
                {"9", 50000 },
                {"10", 50000 },
                {"11", 50000 },
                {"12", 50000 },
            }},
            { 18, new Dictionary<string, int>() { // numbers_menu
                {"0", 20000 },
                {"1", 20000 },
                {"2", 20000 },
                {"3", 20000 },
                {"4", 20000 },
            }},
			{ 26, new Dictionary<string, int>() { // armor_menu
                {"-1", 120000 },
                {"0", 220000 },
                {"1", 320000 },
                {"2", 420000 },
                {"3", 520000 },
				{"4", 620000 },
            }},
        };
        public static Dictionary<int, Dictionary<int, int>> TuningWheels = new Dictionary<int, Dictionary<int, int>>()
        {
            // спортивные
            { 0, new Dictionary<int, int>() {
				{ -1, 2500000 },
                { 50, 3000000 },
                { 51, 3000000 },
                { 52, 3000000 },
                { 53, 3000000 },
                { 54, 3000000 },
                { 55, 3000000 },
                { 56, 3000000 },
                { 57, 3000000 },
                { 58, 3000000 },
                { 59, 3000000 },
                { 60, 3000000 },
                { 61, 3000000 },
                { 62, 3000000 },
                { 63, 3000000 },
                { 64, 3000000 },
                { 65, 3000000 },
                { 66, 3000000 },
                { 67, 3000000 },
                { 68, 3000000 },
                { 69, 3000000 },
                { 70, 3000000 },
                { 71, 3000000 },
                { 72, 3000000 },
                { 73, 3000000 },
                { 74, 3000000 },
                { 75, 3000000 },
                { 76, 3000000 },
                { 77, 3000000 },
                { 78, 3000000 },
                { 79, 3000000 },
                { 80, 3000000 },
                { 81, 3000000 },
                { 82, 3000000 },
                { 83, 3000000 },
                { 84, 3000000 },
                { 85, 3000000 },
                { 86, 3000000 },
                { 87, 3000000 },
                { 88, 3000000 },
                { 89, 3000000 },
                { 90, 3000000 },
                { 91, 3000000 },
                { 92, 3000000 },
                { 93, 3000000 },
                { 94, 3000000 },
                { 95, 3000000 },
                { 96, 3000000 },
                { 97, 3000000 },
                { 98, 3000000 },
                { 99, 3000000 },
                { 100, 3000000 },
                { 101, 3000000 },
                { 102, 3000000 },
                { 103, 3000000 },
                { 104, 3000000 },
                { 105, 3000000 },
                { 106, 3000000 },
                { 107, 3000000 },
                { 108, 3000000 },
                { 109, 3000000 },
                { 110, 3000000 },
                { 111, 3000000 },
                { 112, 3000000 },
                { 113, 3000000 },
                { 114, 3000000 },
                { 115, 3000000 },
                { 116, 3000000 },
                { 117, 3000000 },
                { 118, 3000000 },
                { 119, 3000000 },
                { 120, 3000000 },
                { 121, 3000000 },
                { 122, 3000000 },
                { 123, 3000000 },
                { 124, 3000000 },
                { 125, 3000000 },
                { 126, 3000000 },
                { 127, 3000000 },
                { 128, 3000000 },
                { 129, 3000000 },
                { 130, 3000000 },
                { 131, 3000000 },
                { 132, 3000000 },
                { 133, 3000000 },
                { 134, 3000000 },
                { 135, 3000000 },
                { 136, 3000000 },
                { 137, 3000000 },
                { 138, 3000000 },
                { 139, 3000000 },
                { 140, 3000000 },
                { 141, 3000000 },
                { 142, 3000000 },
                { 143, 3000000 },
                { 144, 3000000 },
                { 145, 3000000 },
                { 146, 3000000 },
                { 147, 3000000 },
                { 148, 3000000 },
            }},
            // маслкары
            { 1, new Dictionary<int, int>() {
                { -1, 2500 },
				{ 0, 40000 },
				{ 1, 40000 },
				{ 2, 40000 },
				{ 3, 40000 },
				{ 4, 40000 },
				{ 5, 40000 },
				{ 6, 40000 },
				{ 7, 40000 },
				{ 8, 40000 },
				{ 9, 40000 },
				{ 10, 40000 },
				{ 11, 40000 },
				{ 12, 40000 },
				{ 13, 40000 },
				{ 14, 40000 },
				{ 15, 40000 },
				{ 16, 40000 },
				{ 17, 40000 },
            }},
            // лоурайдер
            { 2, new Dictionary<int, int>() {
                { -1, 70000 },
                { 0, 70000 },
                { 1, 70000 },
                { 2, 70000 },
                { 3, 70000 },
                { 4, 70000 },
                { 5, 70000 },
                { 6, 70000 },
                { 7, 70000 },
                { 8, 70000 },
                { 9, 70000 },
                { 10, 70000 },
                { 11, 70000 },
                { 12, 70000 },
                { 13, 70000 },
                { 14, 70000 },
            }},
            // вездеход
            { 3, new Dictionary<int, int>() {
                { -1, 60000 },
                { 0, 60000 },
                { 1, 60000 },
                { 2, 60000 },
                { 3, 60000 },
                { 4, 60000 },
                { 5, 60000 },
                { 6, 60000 },
                { 7, 60000 },
                { 8, 60000 },
                { 9, 60000 },
            }},
            // внедорожник
            { 4, new Dictionary<int, int>() {
                { -1, 50000 },
                { 0, 50000 },
                { 1, 50000 },
                { 2, 50000 },
                { 3, 50000 },
                { 4, 50000 },
                { 5, 50000 },
                { 6, 50000 },
                { 7, 50000 },
                { 8, 50000 },
                { 9, 50000 },
                { 10, 50000 },
                { 11, 50000 },
                { 12, 50000 },
                { 13, 50000 },
                { 14, 50000 },
                { 15, 50000 },
                { 16, 50000 },
            }},
            // тюннер
            { 5, new Dictionary<int, int>() {
                { -1, 60000 },
                { 0, 60000 },
                { 1, 60000 },
                { 2, 60000 },
                { 3, 60000 },
                { 4, 60000 },
                { 5, 60000 },
                { 6, 60000 },
                { 7, 60000 },
                { 8, 60000 },
                { 9, 60000 },
                { 10, 60000 },
                { 11, 60000 },
                { 12, 60000 },
                { 13, 60000 },
                { 14, 60000 },
                { 15, 60000 },
                { 16, 60000 },
                { 17, 60000 },
                { 18, 60000 },
                { 19, 60000 },
                { 20, 60000 },
                { 21, 60000 },
                { 22, 60000 },
                { 23, 60000 },
            }},
            // эксклюзивные
            { 7, new Dictionary<int, int>() {
                { -1, 100000 },
                { 0, 100000 },
                { 1, 100000 },
                { 2, 100000 },
                { 3, 100000 },
                { 4, 100000 },
                { 5, 100000 },
                { 6, 100000 },
                { 7, 100000 },
                { 8, 100000 },
                { 9, 100000 },
                { 10, 100000 },
                { 11, 100000 },
                { 12, 100000 },
                { 13, 100000 },
                { 14, 100000 },
                { 15, 100000 },
                { 16, 100000 },
                { 17, 100000 },
                { 18, 100000 },
                { 19, 100000 },
            }},
        };

    }
}

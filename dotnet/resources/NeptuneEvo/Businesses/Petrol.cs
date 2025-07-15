using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using System;
using static NeptuneEVO.Businesses.BCore;

namespace NeptuneEVO.Businesses
{
    class RefillI : Script
    {
        private static nLog Log = new nLog("REFILL");

        public static int CostForFuel = 13;
        public class Refill : BCore.Bizness
        {
            public Refill(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 1;
                Name = "Заправка";
                BlipColor = 5;
                BlipType = 361;
                Range = 10f;
                Core.SafeZones.CreateSafeZone(position, 20, 10, 0, 28, false);

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                if (!player.IsInVehicle || player.IsInVehicle && player.VehicleSeat != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                    return;
                }
                OpenPetrolMenu(player);
                
            }

           public static void OpenPetrolMenu(Player player)
           {
                Bizness biz = BizList[player.GetData<int>("BIZ_ID")];
                Vehicle vehicle = player.Vehicle;
                int fuel = vehicle.GetSharedData<int>("PETROL");
                Trigger.PlayerEvent(player, "client::petrol:open", CostForFuel, fuel, VehicleManager.VehicleTank[vehicle.Class]);
                VehicleStreaming.SetEngineState(vehicle, false);
           }
        }

        public static bool GetVehiclePetrol(Vehicle veh, int idpetrol)
        {
            switch (idpetrol)
            {
                case 0:
                    if (!VehicleManager.PremiumPetrolVehicles.Contains((VehicleHash)veh.Model) && !VehicleManager.PremiumPetrolVehicles.Contains((VehicleHash)veh.Model))
                        return true;
                    break;
                case 1:
                    if (VehicleManager.PremiumPetrolVehicles.Contains((VehicleHash)veh.Model))
                        return true;
                    break;
                case 2:
                    if (VehicleManager.DieselVehicles.Contains((VehicleHash)veh.Model))
                        return true;
                    break;
            }
            return false;
        }
        public static double GetMultiplayer(int type)
        {
            switch (type)
            {
                case 0:
                    return 1;
                case 1:
                    return 1.65;
                case 2:
                    return 0.78;
                default:
                    return 1;
            }
        }

        [RemoteEvent("server::petrol:buyother")]
        public static void SERVER_PETROL_BUYOTHER(Player player, int id)
        {
            nItem item = new nItem(ItemType.Debug);
            int price = 0;
            switch(id)
            {
                case 0:
                    price = 2000;
                    item = new nItem(ItemType.GasCan, 1, 20);
                    break;
                case 1:
                    price = 3500;
                    item = new nItem(ItemType.Repair, 1);
                    break;
            }
            if (Main.Players[player].Money < price)
            {
                Notify.Error(player, $"Недостаточно средств");
                return;
            }
            var tryAdd = nInventory.TryAdd(player, item);
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                return;
            }
            MoneySystem.Wallet.Change(player, -price);
            nInventory.Add(player, item);
            Notify.Succ(player, $"Вы купили {nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Name}");
        }

        [RemoteEvent("server::petrol:buy")]
        public static void SERVER_BUYPETROL(Player player, int petrol, int type)
        {
            if (player == null || !Main.Players.ContainsKey(player)) return;
            Vehicle vehicle = player.Vehicle;
            if (vehicle == null && player.VehicleSeat != 0) return;
            bool checkPetrol = GetVehiclePetrol(vehicle, type);
            int price = petrol * CostForFuel * Convert.ToInt32(GetMultiplayer(type));
            if (checkPetrol == false)
            {
                Notify.Error(player, "Этот вид топлива не для вашего авто");
                return;
            }
            if (vehicle.GetSharedData<int>("PETROL") >= VehicleManager.VehicleTank[vehicle.Class])
            {
                Notify.Error(player, "У транспорта полный бак");
                return;
            }
            int tfuel = vehicle.GetSharedData<int>("PETROL") + petrol;
            if (petrol <= 0 || tfuel > VehicleManager.VehicleTank[vehicle.Class])
            {
                Notify.Error(player, "Введите корректные данные");
                return;
            }
            if (!vehicle.HasSharedData("PETROL"))
            {
                Notify.Error(player, "Невозможно заправить эту машину");
                return;
            }
            if (Main.Players[player].Money < price)
            {
                Notify.Error(player, $"Недостаточно средств");
                return;
            }
            vehicle.SetSharedData("PETROL", tfuel);

            if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "PERSONAL")
            {
                var number = NAPI.Vehicle.GetVehicleNumberPlate(vehicle);
                VehicleManager.Vehicles[number].Fuel += petrol;
            }
            MoneySystem.Wallet.Change(player, -price);
            Trigger.PlayerEvent(player, "client::petrol:load", vehicle.GetSharedData<int>("PETROL"));
        }

        [RemoteEvent("server::petrol:endload")]
        public static void SERVER_PETROL_ENDLOAD(Player player)
        {
            Notify.Succ(player, "Транспорт заправлен");
            Commands.RPChat("me", player, $"заправил(а) транспортное средство");
            Trigger.PlayerEvent(player, "client::petrol:close");
        }
        /*
     [RemoteEvent("petrol")]
     public static void fillCar(Player player, int lvl, bool card)
     {
         try
         {
             if (player == null || !Main.Players.ContainsKey(player)) return;
             Vehicle vehicle = player.Vehicle;
             if (vehicle == null) return; //check
             if (player.VehicleSeat != 0) return;
             if (lvl <= 0)
             {
                 Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                 return;
             }
             if (!vehicle.HasSharedData("PETROL"))
             {
                 Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно заправить эту машину", 3000);
                 return;
             }
             if (Core.VehicleStreaming.GetEngineState(vehicle))
             {
                 Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Чтобы начать заправляться - заглушите транспорт.", 3000);
                 return;
             }
             int fuel = vehicle.GetSharedData<int>("PETROL");
             if (fuel >= VehicleManager.VehicleTank[vehicle.Class])
             {
                 Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У транспорта полный бак", 3000);
                 return;
             }

             var isGov = false;
             if (lvl == 9999)
                 lvl = VehicleManager.VehicleTank[vehicle.Class] - fuel;
             else if (lvl == 99999)
             {
                 isGov = true;
                 lvl = VehicleManager.VehicleTank[vehicle.Class] - fuel;
             }

             if (lvl < 0) return;

             int tfuel = fuel + lvl;
             if (tfuel > VehicleManager.VehicleTank[vehicle.Class])
             {
                 Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Введите корректные данные", 3000);
                 return;
             }
             if (isGov)
             {
                 int frac = Main.Players[player].FractionID;
                 if (Fractions.Manager.FractionTypes[frac] != 2)
                 {
                     Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Чтобы заправить транспорт за гос. счет, Вы должны состоять в гос. организации", 3000);
                     return;
                 }
                 if (!vehicle.HasData("ACCESS") || vehicle.GetData<string>("ACCESS") != "FRACTION" || vehicle.GetData<int>("FRACTION") != frac)
                 {
                     Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете заправить за государственный счет не государственный транспорт", 3000);
                     return;
                 }
                 if (Fractions.Stocks.fracStocks[frac].FuelLeft < lvl * CostForFuel)
                 {
                     Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Лимит на заправку гос. транспорта за день исчерпан", 3000);
                     return;
                 }
             }
             else
             {
                 if (!card)
                 {
                     if (Main.Players[player].Money < lvl * CostForFuel)
                     {
                         Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {lvl * CostForFuel - Main.Players[player].Money}$)", 3000);
                         return;
                     }
                 }
                 else if (MoneySystem.Bank.Accounts[Main.Players[player].Bank].Balance < lvl * CostForFuel)
                 {
                     Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств (не хватает {lvl * CostForFuel - MoneySystem.Bank.Accounts[Main.Players[player].Bank].Balance}$)", 3000);
                     return;
                 }
             }
             if (isGov)
             {
                 Fractions.Stocks.fracStocks[6].Money -= lvl * CostForFuel;
                 Fractions.Stocks.fracStocks[Main.Players[player].FractionID].FuelLeft -= lvl * CostForFuel;
             }
             else
             {
                 if (!card)
                     MoneySystem.Wallet.Change(player, -lvl * CostForFuel);
                 else
                     MoneySystem.Bank.Change(Main.Players[player].Bank, -lvl * CostForFuel);
             }

             Trigger.PlayerEvent(player, "VEHICLE::FREEZE", vehicle, true);
             Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Транспорт заправляется", 3000);
             Trigger.PlayerEvent(player, "open_electropetroltimer");
             NAPI.Task.Run(() =>
             {
                 vehicle.SetSharedData("PETROL", tfuel);
                 if (NAPI.Data.GetEntityData(vehicle, "ACCESS") == "PERSONAL")
                 {
                     var number = NAPI.Vehicle.GetVehicleNumberPlate(vehicle);
                     try
                     {
                         VehicleManager.Vehicles[number].Fuel += lvl;
                     }
                     catch { }
                 }
                 Trigger.PlayerEvent(player, "VEHICLE::FREEZE", vehicle, false);
                 Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Транспорт заправлен", 3000);
                 Commands.RPChat("me", player, $"заправил(а) транспортное средство");
                 Trigger.PlayerEvent(player, "close_electropetroltimer");
             }, 10000);
         }
         catch (Exception e) { Log.Write("Petrol: " + e.Message, nLog.Type.Error); }
     }             */

    }
}

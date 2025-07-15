using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using static NeptuneEVO.Core.VehicleManager;
using NeptuneEVO.Houses;

namespace NeptuneEVO.Casino
{
    class CarLottery : Script
    {
        private static nLog Log = new nLog("CarLottery");
        private static bool CompleteFlag = false;
        public static string vModel;
        private static int _price = 5000;
        private static int _minCountMembers = 3;
        private static Vector3 _mainShapePosition = new Vector3(1105.8865, 220.15826, -48.99499);

        private static ColShape _mainShape;
        private static ColShape _podiumShape;

        public static List<string> MemberNames = new List<string>();
        private static List<string> CarsfoGive = new List<string>() {
            "mp1",  
            "modelx", 
        };

        [ServerEvent(Event.ResourceStart)]
        public static void onResourceStart()
        {
            try
            {
                Randomcar();
                _mainShape = NAPI.ColShape.CreateCylinderColShape(_mainShapePosition, 1, 2, 0);
                _podiumShape = NAPI.ColShape.CreateCylinderColShape(new Vector3(1100.077, 219.9723, -50.07865), 50, 50, 0);
                _mainShape.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 806);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                _mainShape.OnEntityExitColShape += (s, ent) =>
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                };

                _podiumShape.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        Trigger.PlayerEvent(ent, "CAR_LOTTERY::PODIUM_LOAD_CAR_MODEL", vModel);
                    }
                    catch (Exception ex) { Console.WriteLine("podiumcolshape.OnEntityEnterColShape: " + ex.Message); }
                };
                Log.Write("Loaded", nLog.Type.Info);

            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }
        public static void CallBackShape(Player player)
        {
            if (!isAccessToTakePart(player)) return;
            Trigger.PlayerEvent(player, "openDialog", "RANDOMMEMBER_ADD", $"Сегодня разыгрывается {Utilis.VehiclesName.GetRealVehicleName(vModel)}. Стоимость участия: {_price}$. Учавствовать?");
        }
        public static void Randomcar()
        {
            try
            {
                int rand = new Random().Next(0, CarsfoGive.Count);
                vModel = CarsfoGive[rand];
            }
            catch (Exception e) { Log.Write("Randomcar: " + e.Message, nLog.Type.Error); }
        }

        [Command("carlottery")]
        public static void CMD_FinishCompetition(Player player, int timeMS = 1000)
        {
            if (!Core.Group.CanUseCmd(player, "carlottery")) return;
            NAPI.Task.Run(() => {
                FinishCompetition(true);
                Notify.Succ(player, "Вы вручную закончили розыгрыш автомобиля");
            }, timeMS);
        }

        public static void FinishCompetition(bool isSendAdmin = false)
        {
            try
            {
                if (DateTime.Now.Hour != 22 && !isSendAdmin && !CompleteFlag) return;
                if(MemberNames.Count < _minCountMembers)
                {
                    NAPI.Chat.SendChatMessageToAll("!{#438cef} [Diamond Casino]: !{#ffffff}" + $"Из-за недостатка участников, розыгрыш автомобиля {Utilis.VehiclesName.GetRealVehicleName(vModel)}, отменяется! Следующий розыгрыш завтра!");
                    MemberNames.Clear();
                    CompleteFlag = true;
                    return;
                }
                int rnd = new Random().Next(0, MemberNames.Count);
                string memberName = MemberNames[rnd];
                var vNumber = VehicleManager.Create(memberName, $"{vModel}", new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                var house = Houses.HouseManager.GetHouse(memberName, true);
                if (house != null)
                {
                    if (house.GarageID != 0)
                    {
                        var garage = Houses.GarageManager.Garages[house.GarageID];
                        if (VehicleManager.getAllPlayerVehicles(memberName).Count < Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                        {
                            garage.SpawnCar(vNumber);
                        }
                    }
                }
                NAPI.Chat.SendChatMessageToAll("!{#438cef} [Diamond Casino]: !{#ffffff}" + 
                    $"В розыгрыше автомобиля выиграл {memberName} и забрал {Utilis.VehiclesName.GetRealVehicleName(vModel)} Поздравим! Следующий розыгрыш завтра!");
                MemberNames.Clear();
                CompleteFlag = true;
            }
            catch (Exception e) { Log.Write("RandomWinner: " + e.Message, nLog.Type.Error); }
        }
        public static void AcceptTakePart(Player player)
        {
            if (!isAccessToTakePart(player)) return;
            if (!Main.PlayerNames.ContainsValue(player.Name)) return;
            if (!MoneySystem.Wallet.Change(player, -_price))
            {
                Notify.Error(player, "У вас недостаточно средств");
                return;
            }
            MemberNames.Add(player.Name);
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы приняли участие в розыгрыше!", 2500);
        }
        private static bool isAccessToTakePart(Player player)
        {
            if (MemberNames.Contains(player.Name))
            {
                Notify.Error(player, "Вы уже учавствуете в розыгрыше");
                return false;
            }
            if (CompleteFlag)
            {
                Notify.Error(player, "Розыгрыш на сегодня закончен");
                return false;
            }
            return true;
        }
    }
}
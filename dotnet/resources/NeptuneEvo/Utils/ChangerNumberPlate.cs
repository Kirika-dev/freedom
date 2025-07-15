using GTANetworkAPI;
using NeptuneEVO.SDK;
using System;
using NeptuneEVO.Houses;
using System.Collections.Generic;
using Newtonsoft.Json;
using NeptuneEVO.Core;

namespace NeptuneEVO.Utilis
{
    class VehChangeNumber : Script
    {
        private static nLog RLog = new nLog("ChangeNumber Place");

        public static Vector3 PositionChangeNumber = new Vector3(-759.11163, -1317.4202, 3.8803802);
        private static Vector3 PositionNPC = new Vector3(441.54263, -987.05426, 29.489315);
        public static List<string> VehcileNumbers = new List<string>();

        public static int Price = 300000; 

        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        {
            try
            {
                NAPI.Marker.CreateMarker(1, PositionChangeNumber, new Vector3(), new Vector3(), 0.5f, new Color(217, 207, 255), false, 0);
                NAPI.Marker.CreateMarker(1, PositionNPC, new Vector3(), new Vector3(), 0.5f, new Color(217, 207, 255), false, 0);
                NAPI.Blip.CreateBlip(793, PositionChangeNumber, 0.8f, 4, "Смена номеров для ТС", shortRange: true, dimension: 0);

                ColShape shapeChangeNum = NAPI.ColShape.CreateCylinderColShape(PositionChangeNumber, 4, 4, 0);
                shapeChangeNum.OnEntityEnterColShape += (s, ent) =>{try{NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 901); Trigger.PlayerEvent(ent, "client::showhintHUD", true, "Установка номера на ТС"); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }};
                shapeChangeNum.OnEntityExitColShape += (s, ent) =>{try{NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0); Trigger.PlayerEvent(ent, "client::showhintHUD", false, ""); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }};

                ColShape shapeNPC = NAPI.ColShape.CreateCylinderColShape(PositionNPC, 2, 2, 0);
                shapeNPC.OnEntityEnterColShape += (s, ent) =>{try{NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 902); Trigger.PlayerEvent(ent, "client::showhintHUD", true, "Покупка номера"); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }};
                shapeNPC.OnEntityExitColShape += (s, ent) => {try{NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0); Trigger.PlayerEvent(ent, "client::showhintHUD", false, ""); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message);}};

                RLog.Write("Loaded", nLog.Type.Info);
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }
        }

        public static void OpenMenuChangeNumber(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!nInventory.Items.ContainsKey(Main.Players[player].UUID)) return;
            List<nItem> items = new List<nItem>(nInventory.Items[Main.Players[player].UUID]);
            if (!player.IsInVehicle)
            {
                Notify.Error(player, "Вы не в машине");
                return;
            }
            if (player.VehicleSeat != 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны быть на водительском месте", 3000);
                return;
            }
            var veh = player.Vehicle;
            if (!veh.HasData("ACCESS") && (veh.GetData<string>("ACCESS") != "PERSONAL" || veh.GetData<string>("ACCESS") != "GARAGE"))
            {
                var access = VehicleManager.canAccessByNumber(player, veh.NumberPlate);
                if (!access)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Это не ваш автомобиль", 3000);
                    return;
                }
            }
            List<object> data = new List<object>();
            foreach (nItem item in items)
            {
                if (item.Type == ItemType.NumberPlate)
                {
                    data.Add(item.Data);
                }
            }

            string json = JsonConvert.SerializeObject(data);
            if (!player.Vehicle.NumberPlate.Contains("TRANSIT"))
                Trigger.PlayerEvent(player, "client::openvehmenuchangenum", player.Vehicle.NumberPlate, json);
            else
                Trigger.PlayerEvent(player, "client::openvehmenuchangenum", null, json);
        }

        public static void OPENMENU_NPCBUYNUMBER(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (player.IsInVehicle) return;
            Trigger.PlayerEvent(player, "client::openmenunpcnumber", Price);
            Trigger.PlayerEvent(player, "NPC.cameraOn", "ChangeNumNPC", 500);
        }

        [RemoteEvent("server::randomvehnum")]
        public static void EVENT_RANDOMNUMBER(Player player, int a)
        {
            bool card = Convert.ToBoolean(a);
            if (!Main.Players.ContainsKey(player)) return;
            if (card)
            {
                if (!MoneySystem.Bank.Change(Main.Players[player].Bank, -Price))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно средств", 3000);
                    return;
                }
                //MoneySystem.Bank.Change(Main.Players[player].Bank, -Price);
            }
            else
            {
                if (Main.Players[player].Money < Price)
                {
                    Notify.Error(player, "Недостаточно средств");
                    return;
                }
                MoneySystem.Wallet.Change(player, -Price);
            }
            var vehnum = Core.VehicleManager.GenerateNumber();
            Trigger.PlayerEvent(player, "client::setvehnum", vehnum);
        }
        [RemoteEvent("server::buynumbers")]
        public static void EVENT_BUYNUMBERS(Player player, string number)
        {
            if (!Main.Players.ContainsKey(player)) return;
            int tryAdd = nInventory.TryAdd(player, new nItem(ItemType.NumberPlate, 1));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                return;
            }
            nInventory.Add(player, new nItem(ItemType.NumberPlate, 1, number));
            Notify.Succ(player, $"Вы купили номера {number}");
        }

        [RemoteEvent("server::setnumberveh")]
        public static void EVENT_SETNUMVEH(Player player, string number)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                nItem item = nInventory.Find(Main.Players[player].UUID, ItemType.NumberPlate);
                nItem itemNum = null;
                if (item == null)
                {
                    Notify.Error(player, "Предмет Номерной знак не найден");
                    return;
                }
                List<nItem> items = new List<nItem>(nInventory.Items[Main.Players[player].UUID]);
                foreach (nItem itemnn in items)
                {
                    if (itemnn.Type == ItemType.NumberPlate)
                    {
                        if (itemnn.Data == number)
                        {
                            itemNum = itemnn;
                            continue;
                        }
                    }
                }
                if (itemNum == null)
                {
                    return;
                }

                if (!player.IsInVehicle)
                {
                    Notify.Error(player, "Вы не в машине");
                    return;
                }

                var veh = player.Vehicle;
                if (!veh.HasData("ACCESS") && (veh.GetData<string>("ACCESS") != "PERSONAL" || veh.GetData<string>("ACCESS") != "GARAGE"))
                {
                    var access = VehicleManager.canAccessByNumber(player, veh.NumberPlate);
                    if (!access)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы можете установить номера только на личное авто", 3000);
                        return;
                    }
                }
                if (VehicleManager.Vehicles.ContainsKey(number) || number.Contains("TRANSIT"))
                {
                    Notify.Error(player, "Такой номер уже стоит на авто");
                    return;
                }
                string OldNum = veh.NumberPlate;
                if (OldNum.Contains("TRANSIT"))
                {
                    var OldVData = VehicleManager.Vehicles[OldNum];

                    VehicleManager.Vehicles.Remove(OldNum);
                    VehicleManager.Vehicles.Add(number, OldVData);
                    Notify.Succ(player, $"Вы установили номера {number}");
                    MySQL.Query($"UPDATE vehicles SET number='{number}' WHERE number='{OldNum}'");
                    VehicleManager.Save(number);

                    nInventory.Remove(player, itemNum);
                }
                else
                {
                    var OldVData = VehicleManager.Vehicles[OldNum];

                    VehicleManager.Vehicles.Remove(OldNum);
                    VehicleManager.Vehicles.Add(number, OldVData);
                    Notify.Succ(player, $"Вы установили номера {number}");
                    MySQL.Query($"UPDATE vehicles SET number='{number}' WHERE number='{OldNum}'");
                    VehicleManager.Save(number);

                    nInventory.Remove(player, itemNum);
                    nInventory.Add(player, new nItem(ItemType.NumberPlate, 1, OldNum));
                    GameLog.Items($"Numberplate ({OldNum} to {number})", $"player({Main.Players[player].UUID})", Convert.ToInt32(ItemType.NumberPlate), 1, $"{OldNum}");
                }
                player.Vehicle.NumberPlate = number;
            }
            catch (Exception e) { RLog.Write("Changenumber: " + e.Message, nLog.Type.Error);  }
        }

        /*NOT USED*/
        [RemoteEvent("server::takenumber")]
        public void EVENT_TAKENUMBER(Player player, string number)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (VehicleManager.Vehicles.ContainsKey(number)) return;

                if (!player.IsInVehicle)
                {
                    Notify.Error(player, "Вы не в машине");
                    return;
                }

                var veh = player.Vehicle;
                if (!veh.HasData("ACCESS") && (veh.GetData<string>("ACCESS") != "PERSONAL" || veh.GetData<string>("ACCESS") != "GARAGE"))
                {
                    var access = VehicleManager.canAccessByNumber(player, veh.NumberPlate);
                    if (!access)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы можете установить номера только на личное авто", 3000);
                        return;
                    }
                }
                if (player.Vehicle.NumberPlate.Contains("TRANSIT"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас нет номера", 3000);
                    return;
                }
                string oldNum = player.Vehicle.NumberPlate;
                VehicleManager.VehicleData data = VehicleManager.Vehicles[player.Vehicle.NumberPlate];
                if (data == null) return;
                if (data.Holder != player.Name)
                {
                    Notify.Error(player, "Вы не владелец этого транспорта");
                    return;
                }
                int tryAdd = nInventory.TryAdd(player, new nItem(ItemType.NumberPlate, 1));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                    return;
                }

                string nullText = "        ";
                string transitNum = nullText + "TRANSIT-" + VehicleManager.GenerateNumber();
                player.Vehicle.NumberPlate = transitNum;

                VehicleManager.Vehicles.Remove(oldNum);
                VehicleManager.Vehicles.Add(transitNum, data);
                MySQL.Query($"UPDATE vehicles SET number='{transitNum}' WHERE number='{oldNum}'");
                VehicleManager.Save(transitNum);

                nInventory.Add(player, new nItem(ItemType.NumberPlate, 1, number));
                Notify.Succ(player, "Вы сняли номера");
            }
            catch (Exception e) { RLog.Write("changeNumber: " + e.Message, nLog.Type.Error); }
        }
    }
}
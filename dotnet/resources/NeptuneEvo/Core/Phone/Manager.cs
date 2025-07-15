using GTANetworkAPI;
using NeptuneEVO;
using NeptuneEVO.Businesses;
using NeptuneEVO.Core;
using NeptuneEVO.Houses;
using NeptuneEVO.SDK;
using NeptuneEVO.Voice;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Trigger = NeptuneEVO.Trigger;

namespace NeptuneEvo.Core.Phone
{
    public class Manager : Script
    {
        private static nLog Log = new nLog("Phone");
        [RemoteEvent("server::phone:open")]
        public static void Open(Player player)
        {
            if ((player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") != ItemType.Debug) && (player.HasSharedData("INVENTORY_ITEMINHANDS") && player.GetSharedData<bool>("INVENTORY_ITEMINHANDS") == true))
            {
                Notify.Error(player, "Сначала уберите предмет из рук");
                return;
            }
            Trigger.PlayerEvent(player, "client::phone:open", JsonConvert.SerializeObject(GetHouseInfo(player)), JsonConvert.SerializeObject(GetApartInfo(player)), JsonConvert.SerializeObject(GetVehicleInfo(player)), JsonConvert.SerializeObject(Forbes.Top), Main.Players[player].Bank, NeptuneEVO.MoneySystem.Bank.Accounts[Main.Players[player].Bank].Balance, Main.Players[player].Sim, Main.Players[player].WorkID, JsonConvert.SerializeObject(NeptuneEVO.Jobs.DeliveryClub.Orders), JsonConvert.SerializeObject(NeptuneEVO.Jobs.DeliveryClub.GetItemsInMarket()));
            if (!player.IsInVehicle)
            {
                player.PlayAnimation("cellphone@", "cellphone_text_in", 49);
                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        player.PlayAnimation("cellphone@", "cellphone_text_read_base", 49);
                    }
                }, 1000);
                BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.11, 0.03, -0.01), new Vector3(85, -15, 120));
            }
        }
        [RemoteEvent("server::phone:close")]
        public static void Close(Player player)
        {
            Trigger.PlayerEvent(player, "client::phone:close");
            if (!player.IsInVehicle)
            {
                player.PlayAnimation("cellphone@", "cellphone_text_out", 49);
                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        player.PlayAnimation("rcmcollect_paperleadinout@", "kneeling_arrest_get_up", 33);
                        BasicSync.DetachObject(player);
                    }
                }, 1000);
            }
        }
        [RemoteEvent("server::phone:callstart")]
        public static void ClientEvent_CallToPlayer(Player player, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            int num;
            if (Int32.TryParse(text, out num))
            {
                if (!Main.SimCards.ContainsKey(num))
                {
                    Trigger.PlayerEvent(player, "client::phone:calloff", 2);
                    return;
                }
                Player t = Main.GetPlayerByUUID(Main.SimCards[num]);
                Voice.PhoneCallCommand(player, t);
            }
        }
        [RemoteEvent("server::phone:transfermoney")]
        public static void ClientEvent_TransferMoney(Player player, string bankid, string transfermoney)
        {
            try
            {
                if (Convert.ToString(transfermoney) == null || Convert.ToString(bankid) == null)
                {
                    Trigger.PlayerEvent(player, "client::phone:notify", 0, $"Введите корректные данные!", 2000);
                    return;
                }
                if (Main.Players[player].LVL < 1)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Перевод денег доступен после первого уровня", 3000);
                    return;
                }
                if (player.HasData("NEXT_BANK_TRANSFER") && DateTime.Now < player.GetData<DateTime>("NEXT_BANK_TRANSFER"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Следующая транзакция будет возможна в течение минуты", 3000);
                    return;
                }
                if (Convert.ToInt32(bankid) == Main.Players[player].Bank)
                {
                    Trigger.PlayerEvent(player, "client::phone:notify", 0, $"Ошибка перевода", 2000);
                    return;
                }
                if (!NeptuneEVO.MoneySystem.Bank.Accounts.ContainsKey(Convert.ToInt32(bankid)) || Convert.ToInt32(bankid) <= 0)
                {
                    Trigger.PlayerEvent(player, "client::phone:notify", 0, $"Счет не найден", 2000);
                    return;
                }
                if (NeptuneEVO.MoneySystem.Bank.Accounts[Main.Players[player].Bank].Balance < Convert.ToInt32(transfermoney))
                {
                    Trigger.PlayerEvent(player, "client::phone:notify", 0, $"Недостаточно средств", 2000);
                    return;
                }
                NeptuneEVO.MoneySystem.Bank.Change(Main.Players[player].Bank, -Convert.ToInt32(transfermoney), false);
                NeptuneEVO.MoneySystem.Bank.Change(Convert.ToInt32(bankid), Convert.ToInt32(transfermoney), false);
                Trigger.PlayerEvent(player, "client::phone:notify", 6, $"Списание средств -{transfermoney}$", 2000);
            }
            catch (Exception e) { Log.Write("Phone_ClientEvent_TransferMoney: " + e.ToString(), nLog.Type.Error); }
        }
        public static List<Dictionary<string, object>> GetVehicleInfo(Player player)
        {
            try
            {
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                foreach (var number in VehicleManager.getAllPlayerVehicles(player.Name))
                {
                    if (VehicleManager.Vehicles.ContainsKey(number))
                    {
                        Dictionary<string, object> dataveh = new Dictionary<string, object>()
                        {
                            { "Name", NeptuneEVO.Utilis.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[number].Model) },
                            { "Number", number },
                            { "Price", BCore.GetVipCost(player, BCore.CostForCar(VehicleManager.Vehicles[number].Model)) },
                        };
                        data.Add(dataveh);
                    }
                };
                return data;
            }
            catch { return null; }
        }
        public static Dictionary<string, object> GetHouseInfo(Player player)
        {
            try
            { 
                House house = HouseManager.GetHouse(player);
                if (house != null)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>()
                    {
                        { "have", true },
                        { "id", house.ID },
                        { "price", house.Price },
                        { "sellprice", BCore.GetVipCost(player, house.Price) },
                    };
                    return data;
                }
                else
                {
                    Dictionary<string, object> data = new Dictionary<string, object>()
                    {
                        { "have", false },
                        { "id", -1 },
                        { "price", -1 },
                        { "sellprice", -1 },
                    };
                    return data;
                }
            }
            catch { return null; }
        }
        public static Dictionary<string, object> GetApartInfo(Player player)
        {
            try
            {
                House house = HouseManager.GetApart(player);
                if (house != null)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>()
                    {
                        { "have", true },
                        { "id", house.ID },
                        { "price", house.Price },
                        { "sellprice", BCore.GetVipCost(player, house.Price) },
                    };
                    return data;
                }
                else
                {
                    Dictionary<string, object> data = new Dictionary<string, object>()
                    {
                        { "have", false },
                        { "id", -1 },
                        { "price", -1 },
                        { "sellprice", -1 },
                    };
                    return data;
                }
            }
            catch { return null; }
        }
    }
}

using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using NeptuneEVO.Businesses;
using NeptuneEVO.Core;
using System.Data;
using System.Linq;
using Newtonsoft.Json;

namespace NeptuneEVO.Utilis
{
    class Forbes : Script
    {

        private static nLog Log = new nLog("Forbes");

        private static Dictionary<string, int> Majors = new Dictionary<string, int>();

        [RemoteEvent("server::getbuy")]
        public static void GetBuy(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player) || !player.HasData("BIZ_ID") || player.GetData<int>("BIZ_ID") == -1 || !BCore.BizList.ContainsKey(player.GetData<int>("BIZ_ID"))) return;

               BCore.BuyBiz(player);

            }
            catch { }
        }

        [RemoteEvent("getforbes")]
        public static void RM_getforbes(Player player)
        {
            try
            {
                List<object> data = new List<object>();
                foreach (KeyValuePair<string, int> obj in Majors)
                    data.Add(new List<object> { obj.Key, obj.Value });

                List<object> cars = new List<object>();
                List<object> bizlist = new List<object>();
                foreach (string number in VehicleManager.getAllPlayerVehicles(player.Name))
                    cars.Add($"{VehicleManager.Vehicles[number].Model} ({number})");

                foreach (int id in Main.Players[player].BizIDs)
                    if (Businesses.BCore.BizList.ContainsKey(id))
                    {
                        BCore.Bizness biz = Businesses.BCore.BizList[id];
                        bizlist.Add(new List<object> { $"{biz.GetName()} #{biz.ID}", MoneySystem.Bank.Accounts[biz.BankID].Balance, biz.GetNalog() * 24, biz.GetDay(), biz.Materials + "/" + biz.GetMaxMaterials(), BCore.GetVipCost(player, biz.Cost) });
                    }

                Trigger.PlayerEvent(player, "setforbes", JsonConvert.SerializeObject(data), JsonConvert.SerializeObject(cars), JsonConvert.SerializeObject(bizlist));
            }
            catch (Exception e) { Log.Write("GETFORBES: " + e.ToString(), nLog.Type.Error); }
        }

    }
}

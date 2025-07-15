using GTANetworkAPI;
using NeptuneEVO.Businesses;
using NeptuneEVO.Core;
using NeptuneEVO.Houses;
using NeptuneEVO.MoneySystem;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace NeptuneEvo.Core.Phone
{
    class Forbes
    {
        private static nLog Log = new nLog("Forbes");
        public static List<Dictionary<string, object>> Top = new List<Dictionary<string, object>>();
        private static int Max = 15;

        public static void Load()
        {
            NAPI.Task.Run(() => {
                try
                {
                    Top.Clear();
                    var database = MySQL.QueryRead($"SELECT * FROM `characters`");
                    Dictionary<string, int> nosync = new Dictionary<string, int> { };
                    foreach (DataRow Row in database.Rows)
                    {
                        if (Convert.ToInt32(Row["adminlvl"]) != 0) continue;

                        string nick = Row["firstname"].ToString() + "_" + Row["lastname"].ToString();
                        int money = GetPlayerAllMoney(nick, Convert.ToInt32(NeptuneEVO.MoneySystem.Bank.Accounts[Convert.ToInt32(Row["bank"])].Balance) + Convert.ToInt32(Row["money"]));
                        nosync.Add(nick, money);
                    }

                    nosync = nosync.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                    int i = 0;
                    foreach (KeyValuePair<string, int> pair in nosync.Reverse().ToDictionary(x => x.Key, x => x.Value))
                    {
                        if (i > Max - 1) break;
                        Dictionary<string, object> pData = new Dictionary<string, object>()
                        {
                            { "Name", pair.Key },
                            { "Money", pair.Value },
                        };
                        Top.Add(pData);
                        i++;
                    }
                }
                catch (Exception e) { Log.Write("Forbes_Load: " + e.ToString(), nLog.Type.Error); }
            }, 2000);
        }

        public static int GetPlayerAllMoney(string Name, int add)
        {
            try
            {
                int result = add;
                foreach (House house in HouseManager.Houses)
                    if (house.Owner == Name)
                    {
                        result += house.Price;
                        break;
                    }
                foreach (string number in VehicleManager.getAllPlayerVehicles(Name))
                {
                    result += BCore.CostForCar(VehicleManager.Vehicles[number].Model);
                }

                foreach (AirVehicle air in AirVehicles.getAllAirVehicles(Name).Values)
                {
                    result += BCore.CostForCar(air.Model);
                }


                foreach (BCore.Bizness biz in BCore.BizList.Values)
                    if (biz.Owner == Name)
                    {
                        result += biz.Cost;
                        break;
                    }

                return result;
            }
            catch (Exception e) { Log.Write("Forbes_GetPlayerAllMoney: " + e.ToString(), nLog.Type.Error); return 0; }
        }
    }
}

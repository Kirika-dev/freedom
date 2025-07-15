using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.MoneySystem;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using static NeptuneEVO.Core.GarbageCans;

namespace NeptuneEVO.GUI
{
    class GameUI : Script
    {
        private static nLog Log = new nLog("GameUI");
        
        [RemoteEvent("server::playermenu:open")]
        public static void OpenPlayerMenu(Player player)
        {
            Trigger.PlayerEvent(player, "client::playermenu:open", JsonConvert.SerializeObject(Main.Players[player].Statictica), JsonConvert.SerializeObject(GetStats(player)), JsonConvert.SerializeObject(GetHouseInformation(player)), JsonConvert.SerializeObject(GetApartamentInformation(player)), JsonConvert.SerializeObject(GetBusinessInformation(player)), JsonConvert.SerializeObject(GetTransportInformation(player)), Houses.HouseManager.GetMaxCarsInHouse(player));
        }

        public static List<object> GetStats(Player player)
        {
            Core.Character.Character acc = Main.Players[player];
            long bank = (acc.Bank != 0) ? Bank.Accounts[acc.Bank].Balance : 0;
            Houses.House h = Houses.HouseManager.GetNearestHouse(player);
            string gethouse = h == null ? "" : h.Apart != -1 ? $"{Houses.Apartments.ApartmentList[h.Apart].Name}" : $"Дом #{h.ID}"; 
            string house = h == null ? "Нет прописки" : gethouse;
            string work = (acc.WorkID > 0) ? Jobs.WorkManager.JobStats[acc.WorkID - 1] : "Безработный";
            string fraction = (acc.FractionID > 0) ? Fractions.Manager.FractionNames[acc.FractionID] : "Отсутствует";
            string fractionRank = Fractions.Manager.getNickname(acc.FractionID, acc.FractionLVL) != null ? Fractions.Manager.getNickname(acc.FractionID, acc.FractionLVL) : "Отсутствует";

            string status = (Main.Accounts[player].VipLvl > 0) ? $"{Group.GroupNames[Main.Accounts[player].VipLvl]}" : $"{Group.GroupNames[Main.Accounts[player].VipLvl]}";

            List<object> data = new List<object>
            {
                acc.LVL,
                acc.EXP,
                Convert.ToInt32(3 + acc.LVL *  3),
                acc.CreateDate.ToString("dd.MM.yyyy"),
                acc.Money,
                bank,
                24,//Age,
                house,
                work,
                "Не женат",
                fractionRank,
                fraction,
                status,
                Main.Accounts[player].VipLvl > 0 ? Main.Accounts[player].VipDate.ToString("dd.MM.yyyy") : null,
            };
            return data;
        }
        public static HouseInfo GetHouseInformation(Player player)
        {
            Houses.House h = Houses.HouseManager.GetHouse(player);
            if (h == null) return new HouseInfo(false, 0);
            return new HouseInfo(true, h.ID);
        }
        public static List<TransportInfo> GetTransportInformation(Player player)
        {
            List<TransportInfo> Transports = new List<TransportInfo>();
            VehicleManager.getAllPlayerVehicles(player.Name)?.ForEach(number =>
            {
                if (VehicleManager.Vehicles.ContainsKey(number)) 
                    Transports.Add(new TransportInfo(Utilis.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[number].Model), number.ToUpper(), VehicleManager.Vehicles[number].Sell));
            });
            return Transports;
        }
        public static ApartamentInfo GetApartamentInformation(Player player)
        {
            Houses.House h = Houses.HouseManager.GetApart(player);
            if (h == null || h.Apart == -1) return new ApartamentInfo(false, "");
            return new ApartamentInfo(true, Houses.Apartments.ApartmentList[h.Apart].Name);
        }
        public static BusinessInfo GetBusinessInformation(Player player)
        {
            int bizid = Main.Players[player].BizIDs.Count != 0 ? Main.Players[player].BizIDs[0] : -1;
            if (bizid == -1) return new BusinessInfo(false, 0, "");
            return new BusinessInfo(true, bizid, Businesses.BCore.BizList[bizid].GetName());
        }
        public static void AddToStatistic(Player player, int type, int add)
        {
            if (Main.Players[player].Statictica == null)
            {
                Main.Players[player].Statictica = new Statictic(0, 0, 0, 0, 0, 0);
            }
            Statictic stat = Main.Players[player].Statictica;
            switch(type)
            {
                case 0:
                    stat.Arrest += add;
                    break;
                case 1:
                    stat.Warns += add;
                    break;
                case 2:
                    stat.Kills += add;
                    break;
                case 3:
                    stat.Deaths += add;
                    break;
                case 4:
                    stat.MoneyEarn += add;
                    break;  
                case 5:
                    stat.MoneySpent += add;
                    break;
            }
        }

        public static string GetPlayerTime(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return "ERROR";
            int offminutes = Main.Players[player].TimeMinutes;
            int hours = 0;
            int minutes = offminutes;

            
            if (offminutes >= 60)
            {
                hours = offminutes / 60;
                minutes = offminutes - hours * 60;
            }

            string minuts = minutes.ToString();

            if (minutes < 10)
                minuts = "0" + minutes.ToString();
                

            return $"{hours} ч. {minuts} мин.";
        }

        public static string GetPlayerTimeWheell(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return "ERROR";
            int offminutes = 180 - Main.Players[player].TimeMinutes % 60;
            int hours = 0;
            int minutes = offminutes;


            if (offminutes >= 60)
            {
                hours = offminutes / 60;
                minutes = offminutes - hours * 60;
            }

            string minuts = minutes.ToString();

            if (minutes < 10)
                minuts = "0" + minutes.ToString();


            return $"{hours} ч. {minuts} мин.";
        }
        public class HouseInfo
        {
            public bool have { get; set; }
            public int ID { get; set; }
            public HouseInfo(bool h, int id)
            {
                have = h;
                ID = id;
            }
        }
        public class ApartamentInfo
        {
            public bool have { get; set; }
            public string name { get; set; }
            public ApartamentInfo(bool h, string n)
            {
                have = h;
                name = n;
            }
        }
        public class BusinessInfo
        {
            public bool have { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public BusinessInfo(bool h, int i, string n)
            {
                have = h;
                id = i;
                name = n;
            }
        }
        public class TransportInfo
        {
            public string Name { get; set; }
            public string Number { get; set; }
            public float Mile { get; set; }
            public TransportInfo(string name, string plate, float mile)
            {
                Name = name;
                Number = plate;
                Mile = mile;
            }
        }
    }
}

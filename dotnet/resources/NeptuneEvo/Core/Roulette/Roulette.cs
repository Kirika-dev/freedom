using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;

namespace NeptuneEVO.Core
{
    class CaseController : Script
    {
        public static Dictionary<int, List<CaseItem>> CasesPrizeList = new Dictionary<int, List<CaseItem>>();

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                DataTable result = MySQL.QueryRead($"SELECT * FROM `caseprize`");
                if (result == null || result.Rows.Count == 0) return;
                foreach (DataRow Row in result.Rows)
                {
                    int UUID = Convert.ToInt32(Row["UUID"]);
                    string Cases = Row["Prize"].ToString();
                    List<CaseItem> ListCases = JsonConvert.DeserializeObject<List<CaseItem>>(Cases);
                    CasesPrizeList.Add(UUID, ListCases);
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public static Dictionary<String, Case> Cases = new Dictionary<String, Case>
        {

            {"DEFCASE",new Case(new Dictionary<int, CaseItem>
                {
                    { 31, new CaseItem(0,"Money", 1, 0, 70, "5500000", "5.500.000$", "money1.png") },
                    { 32, new CaseItem(1,"Money", 1, 0, 140, "10500000", "10.500.000$", "money1.png") },
                    { 22, new CaseItem(2,"Money", 1, 0, 220, "15500000", "15.500.000$", "money1.png") },

                    { 35, new CaseItem(3,"DonatePoint", 1, 0, 50, "50", "50 MC", "donate.png") },
                    { 34, new CaseItem(4,"DonatePoint", 1, 0, 150, "150", "150 MC", "donate.png") },
                    { 33, new CaseItem(5,"DonatePoint", 1, 0, 250, "250", "250 MC", "donate.png") },
                    { 11, new CaseItem(6,"DonatePoint", 2, 0, 500, "500", "500 MC", "donate.png") },

                    { 26, new CaseItem(7,"Car", 1, 1, 300, "bmwg07", "BMW X7", "bmwx7.png") },
                    { 25, new CaseItem(8,"Car", 1, 1, 200, "16charger", "Dodge Charger 16", "16charger.png") },
                    { 10, new CaseItem(9,"Car", 2, 0, 700, "urus", "Lamborghini Urus", "urus.png") },

                    { 2, new CaseItem(10,"Car", 3, 0, 2399, "m1procar", "BMW M1", "e60.png") },
                    { 1, new CaseItem(11,"Car", 3, 0, 1799, "rmodbugatti", "Bugatti La Voiture Noire", "rmodbugatti.png") },
                    { 5, new CaseItem(12,"Car", 2, 0, 900, "huracan", "Lamborghini Huracan", "huracan.png") },
                    { 4, new CaseItem(13,"Car", 3, 0, 1599, "go650", "Mercedes-Maybach G 650 Landaule", "go650.png") },
                    { 3, new CaseItem(14,"Car", 3, 0, 1999, "rmodi8ks", "BMW I8 Liberty Walk", "rmodi8ks.png") },
                })
            },
            {"VTOROI",new Case(new Dictionary<int, CaseItem>
                {
                    { 22, new CaseItem(0,"Money", 1, 0, 100, "7500000", "7.500.000$", "money1.png") },
                    { 23, new CaseItem(1,"Money", 1, 0, 200, "12500000", "12.500.000$", "money1.png") },
                    { 20, new CaseItem(2,"Money", 1, 0, 250, "19500000", "19.500.000$", "money1.png") },
                    { 13, new CaseItem(3,"Money", 2, 0, 550, "50500000", "50.500.000$", "money1.png") },

                    { 35, new CaseItem(4,"DonatePoint", 1, 0, 200, "200", "200 MC", "donate.png") },
                    { 34, new CaseItem(5,"DonatePoint", 1, 0, 350, "350", "350 MC", "donate.png") },
                    { 33, new CaseItem(6,"DonatePoint", 1, 0, 490, "490", "490 MC", "donate.png") },
                    { 11, new CaseItem(7,"DonatePoint", 2, 0, 800, "800", "800 MC", "donate.png") },

                    { 21, new CaseItem(8,"Car", 1, 0, 400, "e60", "BMW E60", "e60.png") },
                    { 19, new CaseItem(9,"Car", 1, 0, 400, "63gls", "Mercedes-Benz GLS 63", "63gls.png") },
                    { 18, new CaseItem(10,"Car", 1, 0, 400, "z4vp", "BMW Z4 M", "z4.png") },

                    { 6, new CaseItem(11,"Car", 3, 0, 1399, "BMWM5CS", "BMW M5 CS", "BMWM5CS.png") },
                    { 1, new CaseItem(12,"Car", 3, 0, 2899, "asvj", "Lamborghini SVJ", "asvj.png") },
                    { 2, new CaseItem(13,"Car", 3, 0, 2399, "m1procar", "BMW M1", "e60.png") },
                    { 4, new CaseItem(14,"Car", 3, 0, 1799, "rolls08", "Rolls-Royce Sweptail", "rolls08.png") },

                    { 3, new CaseItem(15,"Car", 2, 0, 1799, "rmodbugatti", "Bugatti La Voiture Noire", "rmodbugatti.png") },
                    { 17, new CaseItem(16,"Car", 2, 0, 900, "huracan", "Lamborghini Huracan", "huracan.png") },
                    { 5, new CaseItem(17,"Car", 2, 0, 1799, "chiron19", "Bugatti Chiron", "chiron19.png") },
                })
            }
        };
        [RemoteEvent("r:GetCase")]
        public void GetCase(Player player, int id)
        {
            try
            {
                List<CaseItem> Case = new List<CaseItem>();
                switch (id)
                {
                    case 1:
                        {
                            Case = Cases["DEFCASE"].Citems;
                        }
                        break;
                    case 2:
                        {
                            Case = Cases["VTOROI"].Citems;
                        }
                        break;
                }
                Trigger.PlayerEvent(player, "r:setcase", JsonConvert.SerializeObject(Case));
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        [RemoteEvent("r:getWinId")]
        public void GetWinIdCase(Player player, string type, int caseid)
        {
            try
            {
                string name = GetCaseName(player, caseid);
                int id = -1;
                switch (name)
                {
                    case "DEFCASE":
                        if (Main.Accounts[player].RedBucks < 300)
                        {
                            Notify.Error(player, "Недостаточно средств");
                            return;
                        }
                        Main.Accounts[player].RedBucks -= 300;
                        id = Cases["DEFCASE"].GetRandom().id;
                        Trigger.PlayerEvent(player, "r:getWinIdCallback", id, type);
                        return;
                    case "VTOROI":
                        if (Main.Accounts[player].RedBucks < 700)
                        {
                            Notify.Error(player, "Недостаточно средств");
                            return;
                        }
                        Main.Accounts[player].RedBucks -= 700;
                        id = Cases["VTOROI"].GetRandom().id;
                        Trigger.PlayerEvent(player, "r:getWinIdCallback", id, type);
                        return;
                }
                Trigger.PlayerEvent(player, "redset", Main.Accounts[player].RedBucks);
                MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                Trigger.PlayerEvent(player, "r:getWinIdCallback", id, type);
                Trigger.PlayerEvent(player, "SetRedBucksInMenu", Main.Accounts[player].RedBucks);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        [RemoteEvent("r:getPrize")]
        public void getPrize(Player player, string type, int id, int caseid)
        {
            try
            {
                string casename = GetCaseName(player, caseid);
                if (type != "sellList" && type != "take" && Cases[casename].Citems[id].rarity == 4)
                {
                    NAPI.Chat.SendChatMessageToAll($"Игрок {player.Name} ~r~Выбил с Кейса: {Cases[casename].Citems[id].title}");
                }
                switch (type)
                {
                    case "get":
                        {
                            GetCaseItem(player, id, casename);
                        }
                        break;
                    case "sell":
                        {
                            Main.Accounts[player].RedBucks += Cases[casename].Citems[id].cost;
                            Trigger.PlayerEvent(player, "SetRedBucksInMenu", Main.Accounts[player].RedBucks);
                        }
                        break;
                    case "sellList":
                        {
                            Main.Accounts[player].RedBucks += CasesPrizeList[Main.Players[player].UUID][id].cost;
                            CasesPrizeList[Main.Players[player].UUID].RemoveAt(id);
                            SendCaseList(player);
                            Trigger.PlayerEvent(player, "SetRedBucksInMenu", Main.Accounts[player].RedBucks);
                        }
                        break;
                    case "take":
                        {
                            if (!CasesPrizeList.ContainsKey(Main.Players[player].UUID) || CasesPrizeList[Main.Players[player].UUID].ElementAt(id) == null) return;
                            if (CasesPrizeList[Main.Players[player].UUID][id].Type == "Vip")
                            {
                                if (Main.Accounts[player].VipLvl >= 1)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У вас уже есть VIP статус!", 3000);
                                    return;
                                }
                            }
                            RouletteTakeItem(player, CasesPrizeList[Main.Players[player].UUID][id]);
                            CasesPrizeList[Main.Players[player].UUID].RemoveAt(id);
                            SendCaseList(player);
                        }
                        break;
                }
                Trigger.PlayerEvent(player, "SetRedBucksInMenu", Main.Accounts[player].RedBucks);
                Trigger.PlayerEvent(player, "redset", Main.Accounts[player].RedBucks);
                MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void RouletteTakeItem(Player player, CaseItem itemtype)
        {
            try
            {
                switch (itemtype.Type)
                {
                    case "DonatePoint":
                        Main.Accounts[player].RedBucks += itemtype.cost;
                        break;
                    case "Car":
                        var house = Houses.HouseManager.GetHouse(player, true);
                        if (house == null)
                        {
                            var vNumber = VehicleManager.Create(player.Name, itemtype.carname, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выйграли {itemtype.title} с номером {vNumber}", 5000);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени она будет доставлена на стоянку", 5000);
                        }
                        else
                        {
                            var garage = Houses.GarageManager.Garages[house.GarageID];
                            var vNumber = VehicleManager.Create(player.Name, itemtype.carname, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выйграли {itemtype.title} с номером {vNumber}", 5000);
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени она будет доставлена в Ваш гараж", 5000);
                            garage.SpawnCar(vNumber);
                        }
                        break;
                    case "Clothes":
                        Customization.AddClothes(player, (ItemType)Convert.ToInt32(itemtype.carname.Split("_")[0]), Convert.ToInt32(itemtype.carname.Split("_")[1]), Convert.ToInt32(itemtype.carname.Split("_")[2]));
                        break;
                    case "EXP":
                        Main.Players[player].EXP += Convert.ToInt32(itemtype.carname) * Group.GroupEXP[Main.Accounts[player].VipLvl] * Main.oldconfig.ExpMultiplier;
                        break;
                    case "Money":
                        MoneySystem.Wallet.Change(player, Convert.ToInt32(itemtype.carname));
                        break;
                    case "Vip":
                        Main.Accounts[player].VipLvl = Convert.ToInt32(itemtype.carname);
                        Main.Accounts[player].VipDate = DateTime.Now.AddDays(30);
                        InvInterface.sendStats(player);
                        break;

                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public void GetCaseItem(Player player, int id, string casename)
        {
            try
            {
                if (!CasesPrizeList.ContainsKey(Main.Players[player].UUID))
                {
                    List<CaseItem> item = new List<CaseItem>();
                    item.Add(Cases[casename].Citems[id]);
                    CasesPrizeList.Add(Main.Players[player].UUID, item);
                }
                else
                {
                    CasesPrizeList[Main.Players[player].UUID].Add(Cases[casename].Citems[id]);
                }
                MySQL.Query($"UPDATE `caseprize` SET `Prize`='{JsonConvert.SerializeObject(CasesPrizeList[Main.Players[player].UUID])}' WHERE `uuid`={Main.Players[player].UUID}");
                SendCaseList(player);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public string GetCaseName(Player player, int idcase)
        {

            string casename = null;
            switch (idcase)
            {
                case 1:
                    {
                        casename = "DEFCASE";
                    }
                    break;
                case 2:
                    {
                        casename = "VTOROI";
                    }
                    break;
            }
            return casename;
        }
        [RemoteEvent("r:SendCasePrize")]
        public static void SendCaseList(Player player)
        {
            try
            {
                if (!CasesPrizeList.ContainsKey(Main.Players[player].UUID))
                {
                    List<CaseItem> item = new List<CaseItem>();
                    CasesPrizeList.Add(Main.Players[player].UUID, item);
                }
                Trigger.PlayerEvent(player, "r:SendCasePrize", JsonConvert.SerializeObject(CasesPrizeList[Main.Players[player].UUID]), Main.Accounts[player].RedBucks);
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }
        public class CaseItem
        {
            public Int32 id { get; private set; }
            public string Type { get; private set; }
            public Int32 rarity { get; private set; }
            public Int32 chance { get; private set; }
            public Int32 cost { get; private set; }
            public String title { get; private set; }
            public String carname { get; private set; }
            public String background { get; private set; }
            public CaseItem(Int32 ID, String Type, Int32 rarity = 1, Int32 chance = 1, Int32 cost = 1, String carname = "", String Title = "Default", String background = "Default")
            {
                this.id = ID;
                this.Type = Type;
                this.rarity = rarity;
                this.chance = chance;
                this.cost = cost;
                this.title = Title;
                this.carname = carname;
                this.background = background;
            }
        }
        public class Case
        {
            private Dictionary<(Int32 Min, Int32 Max), CaseItem> Items { get; set; }
            private Int32 ChanceLenght { get; set; }
            public List<CaseItem> Citems { get; set; }
            public Case(Dictionary<Int32, CaseItem> Items)
            {
                this.Citems = new List<CaseItem>();
                this.ChanceLenght = 0;
                this.Items = new Dictionary<(Int32 Min, Int32 Max), CaseItem>();
                foreach (KeyValuePair<Int32, CaseItem> Item in Items)
                {
                    this.Citems.Add(Item.Value);
                    this.Items.Add((this.ChanceLenght, this.ChanceLenght + Item.Key), Item.Value);
                    this.ChanceLenght += Item.Key;
                }
            }
            public CaseItem GetRandom()
            {
                Int32 Random = new Random().Next(1, this.ChanceLenght);
                foreach (KeyValuePair<(Int32 Min, Int32 Max), CaseItem> Item in this.Items) if (Item.Key.Min <= Random && Random <= Item.Key.Max) return Item.Value;
                return null;
            }
        }
    }
}

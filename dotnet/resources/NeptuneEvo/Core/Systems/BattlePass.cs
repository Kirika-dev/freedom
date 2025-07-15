using GTANetworkAPI;
using NeptuneEVO.SDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NeptuneEVO.Core
{
    class BattlePass : Script
    {
        private static nLog Log = new nLog("BattlePass");
        public static string NamePass = "Летнего пропуска";

        private static int MaxTop = 100; //RU: Длина списка лидеров BattlePass. EN: Leaderboard Length
        public static DateTime EndDay = new DateTime(2022, 8, 30, 0, 0, 0);  //Year  Month Day Hour Minute Second 
        public static int MinutesPlayedFreeExp = 75; //RU: Сколько минут должено пройти, чтобы получить бесплатный опыт. EN: Time that passes to get free EXP
        public static int CountFreeExp = 300; //RU: Количество бесплатных EXP. EN: Count of free EXP
        public static List<int> PricesBUYExp = new List<int>() { 900, 1500, 2700 }; //RU: Список цен на покупку EXP 5/10/20 EN: EXP buy price list 5/10/20

        public static List<QuestPass> Quests = new List<QuestPass>()
        {
            new QuestPass(1, "Охотник за окунями", "Выловите 10 окуней", 10, 5000),
            new QuestPass(2, "Шахтер", "Добудьте руды в карьере", 80, 2000),
            new QuestPass(3, "Любитель азарта", "Посетите казино", 1, 600),
            new QuestPass(4, "Бизнесмен по дереву", "Продайте 10 шт. Древисины", 10, 400),
            new QuestPass(5, "Бомж Валера", "Обшастайте 10 мусорок", 10, 600),
            new QuestPass(6, "Элитная еда", "Съешьте шаурму", 3, 600),
            new QuestPass(7, "Парковщик", "Припаркуйте автомобиль", 1, 600),
            new QuestPass(8, "Полный бак", "Заправьте авто с помощью канистры", 3, 600),
            new QuestPass(9, "Пиротехник", "Установите заряды фейерверка", 5, 600),
            new QuestPass(10, "Здоровяк", "Пополните здоровье аптечкой", 1, 600),
            new QuestPass(11, "Почтальон", "Завершите рейсы на работе GoPostal", 10, 600),
            new QuestPass(12, "Путешественник", "Побывайте на Cayo Perico", 1, 600),
            new QuestPass(13, "Из Зеленой банды", "Завершите рейсы на работе Delivery Club", 10, 600),
            new QuestPass(14, "Пополнение холодильника", "Закажите еду в Delivery Club", 5, 600),
            new QuestPass(15, "Чистота и порядок", "Воспользуйтесь услугами автомойке", 1, 600),
            new QuestPass(16, "Большое кино", "Включите любой видеоролик в кинотеатре", 1, 600),
            new QuestPass(17, "Любитель познакомиться", "Пожмите руку разным игрокам", 10, 600),
            new QuestPass(18, "Механик", "Почините свой транспорт", 2, 600),
            new QuestPass(19, "Настоящий американец", "Съешьте бургеры", 10, 600),
            new QuestPass(20, "Пора лечиться", "Возьмите бесплатный курс лечения в больнице", 3, 600),
            new QuestPass(21, "Обновка", "Купите одежду в магазине одежды", 1, 600),
        };
        public static Dictionary<int, ItemPass> FreeItems = new Dictionary<int, ItemPass>()
        {
            { 1, new ItemPass(new List<IPass>(){ new IPass("До 25 000$", "money1.png", "money", "25000_true") }) },
            { 4, new ItemPass(new List<IPass>(){ new IPass("100 000$", "money2.png", "money", "100000_false") }) },
            { 6, new ItemPass(new List<IPass>(){ new IPass("Красная роза", "60.png", "item", "60_1_null"), new IPass("Плюшевый медведь", "58.png", "item", "58_1_null"), new IPass("Гитара", "59.png", "item", "59_1_null"), new IPass("Assault Rifle", "126.png", "item", "126_1_BATTLEPASS") }) },
            { 8, new ItemPass(new List<IPass>(){ new IPass("100 FRM-Coins", "donate.png", "coins", "200") }) },
            { 10, new ItemPass(new List<IPass>(){ new IPass("100 EXP", "exp.png", "exp", "100") }) },
        };
        public static Dictionary<int, ItemPass> PremItems = new Dictionary<int, ItemPass>()
        {
            { 3, new ItemPass(new List<IPass>(){ new IPass("До 50 000$", "money3.png", "money", "50000_true") }) },
            { 100, new ItemPass(new List<IPass>(){ new IPass("Tesla PD", "telsapd.png", "vehicle", "teslapd") }) },
        };
        public static List<Dictionary<string, object>> Top = new List<Dictionary<string, object>>();

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
                        if (JsonConvert.DeserializeObject<Pass>(Row["battlepass"].ToString()) == null) continue;
                        //if (Convert.ToInt32(Row["adminlvl"]) != 0 || JsonConvert.DeserializeObject<Pass>(Row["battlepass"].ToString()) == null) continue;
                        
                        string nick = Row["firstname"].ToString() + "_" + Row["lastname"].ToString();
                        int lvl = JsonConvert.DeserializeObject<Pass>(Row["battlepass"].ToString()).Lvl - 1;
                        nosync.Add(nick, lvl);
                    }

                    nosync = nosync.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                    int i = 0;
                    foreach (KeyValuePair<string, int> pair in nosync.Reverse().ToDictionary(x => x.Key, x => x.Value))
                    {
                        if (i > MaxTop - 1) break;
                        Dictionary<string, object> pData = new Dictionary<string, object>()
                        {
                            { "Name", pair.Key },
                            { "Lvl", pair.Value },
                        };
                        Top.Add(pData);
                        i++;
                    }       
                }
                catch (Exception e) { Log.Write("BattlePass_Top: " + e.ToString(), nLog.Type.Error); }
            }, 2000);
        }
        [RemoteEvent("server::battlepass:open")]
        public static void OpenBattlePass(Player player)
        {
            Pass bp = GetBattlePass(player);
            if (bp == null)
            {
                StartPass(player);
                bp = GetBattlePass(player);
            }
            if (Convert.ToInt32(GetDayToEnd()) < 0) { Notify.Info(player, "Сезон закончился"); return; }

            if (player.HasData("BATTLEPASS_OPENED") && player.GetData<bool>("BATTLEPASS_OPENED") == true)
                Trigger.PlayerEvent(player, "client::battlepass:opennoif");
            else
                Trigger.PlayerEvent(player, "client::battlepass:open", JsonConvert.SerializeObject(bp.Free), JsonConvert.SerializeObject(bp.Premium), JsonConvert.SerializeObject(bp.QuestList), JsonConvert.SerializeObject(Top), bp.Lvl, bp.EXP, bp.Buyed, bp.TimeGiveExp, JsonConvert.SerializeObject(PricesBUYExp), GetDayToEnd(), MinutesPlayedFreeExp);
            player.SetData<bool>("BATTLEPASS_OPENED", true);
        }
        [RemoteEvent("server::battlepass:takeitem")]
        public static void TakeItem(Player player, bool type, int lvl, int index)
        {
            Pass bp = GetBattlePass(player);
            if (bp == null) return;
            if (lvl + 1 > bp.Lvl) 
            {
                Notify.Error(player, $"Вы не можете забрать награду {lvl} уровня пропуска");
                return;
            }
            Dictionary<int, ItemPass> items = type == true ? bp.Premium : bp.Free;
            if (items[lvl] != null && items[lvl].items[index] != null)
            {
                if (type == true && bp.Buyed == false)
                {
                    Notify.Error(player, $"У вас нет {NamePass}");
                    return;
                }
                IPass item = items[lvl].items[index];
                if (item.Taken == true)
                {
                    Notify.Error(player, "Вы уже забрали награду");
                    return;
                }
                SwitchTypeItem(player, item);
            }
            else
            {
                Notify.Error(player, "Предмета не существует");
                return;
            }
        }
        public static void SwitchTypeItem(Player player, IPass item)
        {
            string type = item.Type;
            string settings = item.Settings;
            switch (type)
            {
                case "money":
                    int money = Convert.ToInt32(settings.Split("_")[0]);
                    bool randomed = Convert.ToBoolean(settings.Split("_")[1]);
                    if (randomed)
                    {
                        int givemoney = new Random().Next(1, money);
                        MoneySystem.Wallet.Change(player, givemoney);
                        Notify.Succ(player, $"Вы получили {String.Format("{0:n0}", givemoney)}$ с {NamePass}");
                    }
                    else
                    {
                        MoneySystem.Wallet.Change(player, money);
                        Notify.Succ(player, $"Вы получили {String.Format("{0:n0}", money)}$ с {NamePass}");
                    }
                    break;
                case "item":
                    ItemType itype = (ItemType)Convert.ToInt32(settings.Split("_")[0]);
                    int icount = Convert.ToInt32(settings.Split("_")[1]);
                    dynamic idata = Convert.ToString(settings.Split("_")[2]);
                    int tryAdd = Core.nInventory.TryAdd(player, new nItem(itype, icount, idata));
                    if (tryAdd == -1 || tryAdd > 0)
                    {          
                        Notify.Error(player, $"Недостаточно места в инвентаре");
                        return;
                    }
                    nInventory.Add(player, new nItem(itype, icount, idata));
                    Notify.Succ(player, $"Вы получили {nInventory.InventoryItems.Find(x => x.ItemType == itype).Name} x{icount} с {NamePass}");
                    break;
                case "coins":
                    Main.Accounts[player].RedBucks += Convert.ToInt32(settings);
                    Notify.Succ(player, $"Вы получили {settings} FRM-Coins с {NamePass}");
                    break;
                case "exp":
                    Main.Players[player].EXP += Convert.ToInt32(settings);
                    Notify.Succ(player, $"Вы получили {settings} EXP с {NamePass}");
                    break;
                case "vehicle":
                    var house = Houses.HouseManager.GetHouse(player, true);
                    var apartament = Houses.HouseManager.GetApart(player, true);
                    if (house == null)
                    {
                        if (apartament != null)
                        {
                            house = apartament;
                        }
                    }

                    if (house == null && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Нельзя иметь больше 1 машины без дома", 3000);
                        return;
                    }
                    if (house != null)
                    {
                        if (house.GarageID == 0 && VehicleManager.getAllPlayerVehicles(player.Name.ToString()).Count > 1)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет гаража", 3000);
                            return;
                        }

                        var garage = Houses.GarageManager.Garages[house.GarageID];
                        if (VehicleManager.getAllPlayerVehicles(player.Name).Count >= Houses.GarageManager.GarageTypes[garage.Type].MaxCars)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас максимальное кол-во машин", 3000);
                            return;
                        }
                    }

                    var vNumber = VehicleManager.Create(player.Name, settings, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                    var vehdata = VehicleManager.Vehicles[vNumber];
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы забрали {Utilis.VehiclesName.GetRealVehicleName(vehdata.Model)} с {NamePass}", 3000);
                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{vNumber}_{VehicleManager.Vehicles[vNumber].KeyNum}"));
                    break;
            }
            item.Taken = true;
            Trigger.PlayerEvent(player, "client::battlepass:update", "freelvls", JsonConvert.SerializeObject(Main.Players[player].BattlePass.Free));
            Trigger.PlayerEvent(player, "client::battlepass:update", "premiumlvl", JsonConvert.SerializeObject(Main.Players[player].BattlePass.Premium));
            MySQL.Query($"UPDATE `characters` SET `battlepass`='{JsonConvert.SerializeObject(Main.Players[player].BattlePass)}' WHERE `uuid`='{Main.Players[player].UUID}'");
        }
        public static void AddProgressToQuest(Player player, int id, int add)
        {
            Pass bp = GetBattlePass(player);
            if (bp == null) return;
            foreach (QuestPass item in bp.QuestList)
            {
                if (item.ID == id && item.Complete == false)
                {
                    if ((item.Progress + add) < item.Max)
                    {
                        item.Progress += add;
                    }
                    else
                    {
                        item.Progress = item.Max;
                        item.Complete = true;
                        bp.CountQuestsComplete += 1;
                        Trigger.PlayerEvent(player, "client::soundplay", "./sounds/completequest.mp3", 0.2);
                        AddExpPass(player, item.Rewards);
                    }
                    Trigger.PlayerEvent(player, "client::battlepass:update", "quests", JsonConvert.SerializeObject(Main.Players[player].BattlePass.QuestList));
                }
            }
        }
        [Command("addexpbp")]
        public static void CMD_AddExpPass(Player player, int id, int add)
        {
            Player target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                return;
            }
            AddExpPass(target, add);
        }
        public static void AddExpPass(Player player, int add)
        {
            Pass bp = GetBattlePass(player);
            if (bp == null) return;
            player.SetData("BP_LAST_LVL", bp.Lvl);
            AddExp(player, add);
        }
        public static void AddExp(Player player, int add)
        {
            Pass bp = GetBattlePass(player);
            if (bp == null) return;
            bp.EXP += add;
            int nextLvl = (bp.Lvl * 480) + 900;
            if (bp.EXP >= nextLvl)
            {
                AddLVLs(player);
                return;
            }
            if (player.HasData("BP_LAST_LVL") && player.GetData<int>("BP_LAST_LVL") != bp.Lvl)
            {
                Notify.Succ(player, $"Поздравляем! У вас новый {bp.Lvl - 1} уровень {NamePass}");
                Trigger.PlayerEvent(player, "client::battlepass:update", "lvl", bp.Lvl);
            }
            Trigger.PlayerEvent(player, "client::battlepass:update", "exp", bp.EXP);
            MySQL.Query($"UPDATE `characters` SET `battlepass`='{JsonConvert.SerializeObject(Main.Players[player].BattlePass)}' WHERE `uuid`='{Main.Players[player].UUID}'");
        }
        public static void AddLVLs(Player player)
        {
            Pass bp = GetBattlePass(player);
            if (bp == null) return;
            int nextLvl = (bp.Lvl * 480) + 900;
            if (bp.EXP >= nextLvl)
            {
                bp.EXP -= nextLvl;
                bp.Lvl += 1;
                AddExp(player, 0);
            }
        }
        [RemoteEvent("server::battlepass:buyexp")]
        public static void BuyLVLs(Player player, int c1, int c2, int c3)
        {
            Pass bp = GetBattlePass(player);
            if (bp == null) return;
            int oldLVL = bp.Lvl;
            if (c1 > 0)
            {
                if (Main.Accounts[player].RedBucks < c1 * PricesBUYExp[0])
                {
                    Notify.Error(player, "Недостаточно FRM-Coins");
                    return;
                }
                Main.Accounts[player].RedBucks -= c1 * PricesBUYExp[0];
                bp.Lvl += c1 * 5;
            }
            if (c2 > 0)
            {
                if (Main.Accounts[player].RedBucks < c2 * PricesBUYExp[1])
                {
                    Notify.Error(player, "Недостаточно FRM-Coins");
                    return;
                }
                Main.Accounts[player].RedBucks -= c2 * PricesBUYExp[1];
                bp.Lvl += c2 * 10;
            }
            if (c3 > 0)
            {
                if (Main.Accounts[player].RedBucks < c3 * PricesBUYExp[2])
                {
                    Notify.Error(player, "Недостаточно FRM-Coins");
                    return;
                }
                Main.Accounts[player].RedBucks -= c3 * PricesBUYExp[2];
                bp.Lvl += c3 * 20;
            }
            if (c1 > 0 || c2 > 0 || c3 > 0)
            {
                MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
                MySQL.Query($"UPDATE `characters` SET `battlepass`='{JsonConvert.SerializeObject(Main.Players[player].BattlePass)}' WHERE `uuid`='{Main.Players[player].UUID}'");
                Notify.Succ(player, $"Вы купили {bp.Lvl - oldLVL} уровней");
                Trigger.PlayerEvent(player, "client::battlepass:update", "lvl", bp.Lvl);
                Trigger.PlayerEvent(player, "client::battlepass:update", "exp", bp.EXP);
            } 
        }
        public static Pass GetBattlePass(Player player)
        {
            if (Main.Players[player].BattlePass != null)
            {
                return Main.Players[player].BattlePass;
            }
            return null;
        }
        public static List<QuestPass> SetQuestOnDay(Player player)
        {
            List<QuestPass> qp = new List<QuestPass>();
            for (var i = 0; i < 6; i++)
            {
                qp.Add(Quests[GetRandomQuest(qp)]);
            }
            return qp;
        }
        public static int GetRandomQuest(List<QuestPass> qp)
        {
            var rnd = new Random();
            int id = rnd.Next(0, Quests.Count);
            if (qp.Contains(Quests[id])) return GetRandomQuest(qp);
            else return id;
        }
        [RemoteEvent("server::battlepass:buypass")]
        public static void BuyPass(Player player, int type)
        {
            Pass bp = GetBattlePass(player);
            if (bp == null)
            {
                Notify.Error(player, $"У вас нет {NamePass}");
                return;
            }
            switch(type)
            {
                case 0:
                    if (Main.Accounts[player].RedBucks < 1000)
                    {
                        Notify.Error(player, "Недостаточно FRM-Coins");
                        return;
                    }
                    Notify.Succ(player, $"Вы купили Обычный пропуск");
                    Main.Accounts[player].RedBucks -= 1000;
                    bp.Buyed = true;
                    break;
                case 1:
                    if (Main.Accounts[player].RedBucks < 5000)
                    {
                        Notify.Error(player, "Недостаточно FRM-Coins");
                        return;
                    }
                    Notify.Succ(player, $"Вы купили Летний проход");
                    Main.Accounts[player].RedBucks -= 5000;
                    bp.Buyed = true;
                    bp.Lvl = 20;
                    break;
                case 2:
                    if (Main.Accounts[player].RedBucks < 20000)
                    {
                        Notify.Error(player, "Недостаточно FRM-Coins");
                        return;
                    }
                    Notify.Succ(player, $"Вы купили Королевский пропуск");
                    Main.Accounts[player].RedBucks -= 20000;
                    bp.Buyed = true;
                    bp.Lvl = 100;
                    break;
            }
            MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
            MySQL.Query($"UPDATE `characters` SET `battlepass`='{JsonConvert.SerializeObject(Main.Players[player].BattlePass)}' WHERE `uuid`='{Main.Players[player].UUID}'");
            Trigger.PlayerEvent(player, "client::battlepass:update", "havepass", bp.Buyed);
            Trigger.PlayerEvent(player, "client::battlepass:update", "lvl", bp.Lvl);
        }
        public static string GetDayToEnd()
        {
            return (EndDay - DateTime.Now).ToString().Split(".")[0];
        }
        [Command("startbattlepass")]
        public static void StartPass(Player player)
        {
            if (GetBattlePass(player) == null)
            {
                Main.Players[player].BattlePass = new Pass(false, FreeItems, PremItems, 1, 0, SetQuestOnDay(player), DateTime.Now, 0, 0);
                Notify.Succ(player, $"Вы начали прохождение {NamePass}!");
                MySQL.Query($"UPDATE `characters` SET `battlepass`='{JsonConvert.SerializeObject(Main.Players[player].BattlePass)}' WHERE `uuid`='{Main.Players[player].UUID}'");
            }
            else
            {
                Notify.Error(player, $"У вас уже есть {NamePass}");
                return;
            }
        }
        [RemoteEvent("server::battlepass:gift")]
        public static void GiftPass(Player player, int id)
        {
            Player target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Игрок с таким ID не найден", 3000);
                return;
            }
            Pass bp = GetBattlePass(target);
            if (bp == null)
            {
                Notify.Error(player, $"Игрок не начал прохождение {NamePass}");
                return;
            }
            if (bp.Buyed == true)
            {
                Notify.Error(player, $"У игрока уже есть пропуск!");
                return;
            }
            if (Main.Accounts[player].RedBucks < 1000)
            {
                Notify.Error(player, "Недостаточно FRM-Coins");
                return;
            }
            Main.Accounts[player].RedBucks -= 1000;
            bp.Buyed = true;
            MySQL.Query($"UPDATE `characters` SET `battlepass`='{JsonConvert.SerializeObject(Main.Players[target].BattlePass)}' WHERE `uuid`='{Main.Players[target].UUID}'");
            MySQL.Query($"update `accounts` set `redbucks`={Main.Accounts[player].RedBucks} where `login`='{Main.Accounts[player].Login}'");
        }
    }
}
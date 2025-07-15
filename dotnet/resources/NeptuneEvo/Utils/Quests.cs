using System;
using System.Collections.Generic;
using System.Text;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using NeptuneEVO.Businesses;
using NeptuneEVO.Core;
using NeptuneEVO.MoneySystem;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using static System.Globalization.CalendarWeekRule;
using System.Globalization;

namespace NeptuneEVO.Utilis
{
    class AchievementsManager : Script
    {
        public static List<Achievements> Achievements = new List<Achievements>()
        {
            new Achievements(1, "Молодость", "Достичь 5 уровня", 5, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(2, "Уже не молод", "Достичь 10 уровня", 10, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(3, "Что-то между", "Достичь 15 уровня", 15, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(4, "Кризис среднего возраста", "Достичь 20 уровня", 20, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(5, "Ветеран", "Достичь 30 уровня", 30, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(6, "Пора шопиться", "Купить любые товары в магазинах", 50, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(7, "Модный приговор", "Купить одежду в любом магазине одежды", 2, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(8, "Лучший работник", "Выполните 200 заказов Delivery Club", 200, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(9, "Еда всегда под рукой", "Закажите еду в приложении Delivery Club", 10, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(10, "Первый заезд", "Купите автомобиль", 1, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(11, "Крыша над головой", "Купите недвижимость", 1, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(12, "Тяжелое ранение", "Попадите в больницу 50 раз", 1, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(13, "Гроза района", "Получите 3 уровень розыска", 3, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(14, "Новичек", "Отыграйте 5 часов на сервере", 5, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(15, "Первые сутки", "Отыграйте 24 часа на сервере", 24, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(16, "На опыте", "Отыграйте 240 часов на сервере", 240, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(17, "Такси-такси, вези...", "Прокатитесь на такси 5 раз", 5, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(18, "Азартный человек", "Выиграйте в казино 1000 фишек", 1000, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(19, "Путешественник", "Побывайте на каждой станции метро по очереди", 10, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
            new Achievements(20, "Без колес", "Арендуйте транспорт 5 раз", 5, new List<nItem>{ new nItem(ItemType.MoneyItem, 1, 30000), new nItem(ItemType.ExpItem, 1, 5) }),
        };

        public static void AddProgressToQuest(Player player, int id, int add)
        {
            foreach (Achievements item in Achievements)
            {
                if (item.ID == id && item.Complete == false)
                {
                    if ((item.Progress += add) < item.Max)
                    {
                        //item.Progress += add;
                    }
                    else
                    {
                        item.Progress = item.Max;
                        item.Complete = true;
                       /* bp.CountQuestsComplete += 1;
                        Trigger.PlayerEvent(player, "client::soundplay", "./sounds/completequest.mp3", 0.2);
                        AddExpPass(player, item.Rewards);                                   */
                    }
                    Trigger.PlayerEvent(player, "client::playermenu:update", "achievements", JsonConvert.SerializeObject(Main.Players[player].BattlePass.QuestList));
                }
            }
        }
    }
}

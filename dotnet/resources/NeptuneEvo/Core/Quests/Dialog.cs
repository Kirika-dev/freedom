using NeptuneEVO;
using NeptuneEVO.SDK;
using GTANetworkAPI;
using static NeptuneEVO.Core.Quests;
using NeptuneEVO.GUI;
using System;

namespace NeptuneEVO.Core
{
    public class Dialog : Script
    {
        [RemoteEvent("server::dialoganswer")]
        public static void DIALOGANSWER(Player player, int id)
        {
            switch (id)
            {
                case 0:
                    Trigger.PlayerEvent(player, "client::closedialog2");
                    Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Бруно Коллинз", "Проводник", "Ты мне поможешь, я тебе помогу. Мне только что звонила моя мачеха, ей нужно помочь она что-то как обычно потеряля. Сможешь помочь?", (new QuestAnswer("Да", 1), new QuestAnswer("Нет", 2)));
                    return;
                case 1:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Main.Players[player].Achievements[0] = true;
                    Character.Character.CheckAchievements(player);
                    Trigger.PlayerEvent(player, "createWaypoint", Quests.positionSecond.X, Quests.positionSecond.Y);
                    return;
                case 2:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    return;
                case 3:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Main.Players[player].Achievements[1] = true;
                    Character.Character.CheckAchievements(player);
                    player.SetData("QUESTARGS", new Random().Next(1, 4));
                    return;
                case 20:
                    Trigger.PlayerEvent(player, "client::closedialog2");
                    Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Джэймс Фостер", "Работа", "Смотри, я тебе даю транспорт Boxwille и 10 посылок. Потом ты развозишь их, если закончаться приезжай обратно и бери еще", (new QuestAnswer("Понятненько...", 25), 0));
                    return;
                case 25:
                    Trigger.PlayerEvent(player, "client::closedialog2");
                    Jobs.WorkManager.openGoPostalStart(player);
                    return;
                case 21:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Jobs.WorkManager.SetWorkId(player);
                    return;
                case 22:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Jobs.WorkManager.callback_gpStartMenu(player, 0);
                    return;
                case 23:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Jobs.WorkManager.callback_gpStartMenu(player, 1);
                    return;
                case 24:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Jobs.WorkManager.callback_gpStartMenu(player, 2);
                    return;

                case 50:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Jobs.Builder.SetWorkId(player);
                    return;
                case 51:
                    Trigger.PlayerEvent(player, "client::closedialog2");
                    Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Рон", "Прораб", "Дело простое, нужно всего-лишь таскать мешки по точкам и за это будешь получать немного зеленых", (new QuestAnswer("Понял", 53), 0));
                    return;
                case 52:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Jobs.Builder.SetWorkState(player);
                    return;
                case 53:
                    Trigger.PlayerEvent(player, "client::closedialog2");
                    Jobs.Builder.OpenMenu(player);
                    return;

                case 54:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Jobs.DeliveryClub.JobState(player);
                    return;
                case 55:
                    Trigger.PlayerEvent(player, "client::closedialog2");
                    Trigger.PlayerEvent(player, "client::opendialogmenu", true, "Брэндон Кремер", "Работодатель", "Все очень просто, устанавливаешь приложение Delivery Club и входишь в свой аккаунт. Выбираешь заказ и едешь на загрузку еды, после отвозишь на дом клиента. Все клиенты только в пределах города", (new QuestAnswer("Понял", 57), 0));
                    return;
                case 56:
                    Trigger.PlayerEvent(player, "client::closedialog");
                    Jobs.DeliveryClub.WorkState(player);
                    return;
                case 57:
                    Trigger.PlayerEvent(player, "client::closedialog2");
                    Jobs.DeliveryClub.OpenMenu(player);
                    return;
            }
        }
    }
}
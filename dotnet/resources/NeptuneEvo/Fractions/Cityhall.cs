using System;
using System.Collections.Generic;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;

namespace NeptuneEVO.Fractions
{
    class Cityhall : Script
    {
        private static nLog Log = new nLog("Cityhall");
        public static int lastHourTax = 0;
        public static int canGetMoney = 99999999;
        public static bool is_warg = false;

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStartHandler()
        {
            try
            {
                Cols.Add(0, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[0], 1f, 2, 0)); // Оружейка
                Cols[0].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[0].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[0].SetData("INTERACT", 9);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~w~Крафт оружия"), new Vector3(CityhallChecksCoords[0].X, CityhallChecksCoords[0].Y, CityhallChecksCoords[0].Z + 0.7), 5F, 0.4F, 4, new Color(255, 255, 255));

                Cols.Add(1, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[1], 1f, 2, 0)); // Раздевалка
                Cols[1].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[1].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[1].SetData("INTERACT", 1);
                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~w~Переодевалка"), new Vector3(CityhallChecksCoords[1].X, CityhallChecksCoords[1].Y, CityhallChecksCoords[1].Z + 0.7), 5F, 0.4F, 4, new Color(255, 255, 255));

                for (int i = 2; i < 4; i++)
                {
                    Cols.Add(i, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[i], 1, 2, 0));
                    Cols[i].OnEntityEnterColShape += city_OnEntityEnterColShape;
                    Cols[i].OnEntityExitColShape += city_OnEntityExitColShape;
                    Cols[i].SetData("INTERACT", 5);
                    NAPI.Marker.CreateMarker(21, CityhallChecksCoords[i] + new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 0.8f, new Color(255, 255, 255, 60));
                }

                Cols.Add(6, NAPI.ColShape.CreateCylinderColShape(new Vector3(255.2283, 223.976, 102.3932), 3, 2, 0));
                Cols[6].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[6].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[6].SetData("INTERACT", 4);

                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[0] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[1] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));
                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[6] - new Vector3(0, 0, 0.7), new Vector3(), new Vector3(), 1, new Color(255, 255, 255, 220));

                Cols.Add(7, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[6], 1f, 2, 0)); // Оружейка
                Cols[7].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[7].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[7].SetData("INTERACT", 62);

                Cols.Add(8, NAPI.ColShape.CreateCylinderColShape(CityhallChecksCoords[7], 1f, 2, 0)); // Оружейка
                Cols[8].OnEntityEnterColShape += city_OnEntityEnterColShape;
                Cols[8].OnEntityExitColShape += city_OnEntityExitColShape;
                Cols[8].SetData("INTERACT", 941);
                NAPI.Marker.CreateMarker(1, CityhallChecksCoords[6], new Vector3(), new Vector3(), 0.5f, new Color(217, 207, 255), false, 0);

                NAPI.TextLabel.CreateTextLabel(Main.StringToU16("~w~Склад оружия"), new Vector3(CityhallChecksCoords[6].X, CityhallChecksCoords[6].Y, CityhallChecksCoords[6].Z + 0.7), 5F, 0.4F, 4, new Color(255, 255, 255));
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT\"FRACTIONS_CITYHALL\":\n" + e.ToString(), nLog.Type.Error);
            }
        }

        private static Dictionary<int, ColShape> Cols = new Dictionary<int, ColShape>();
        public static List<Vector3> CityhallChecksCoords = new List<Vector3>
        {
            new Vector3(-532.99854, -189.57237, 41.71659), // оружейка в мэрии 0
            new Vector3(-526.20056, -190.3905, 41.71655), // раздевалка в мэрии 1
            new Vector3(-545.0524, -204.0801, 20.09514), // main door enter 2
            new Vector3(233.312, 216.0169, 20.1667), // main door exit 3
            new Vector3(256.9124, 220.4567, 105.2864), // door 1 4
            new Vector3(265.8495, 218.1592, 109.283), // door 2  5
            new Vector3(-530.7673, -193.4257, 41.71659), // gun stock 6
            new Vector3(-554.94415, -186.95253, 37.4) //ID-Card
        };


        public static void Government_GETIDCard(Player player)
        {
            if (nInventory.Find(Main.Players[player].UUID, ItemType.IDCard) != null)
            {
                Notify.Error(player, "У вас уже есть ID-Карта");
                return;
            }
            else
            {
                var tryadd = nInventory.TryAdd(player, new nItem(ItemType.IDCard, 1, Main.Players[player].UUID));
                if (tryadd == -1 || tryadd > 0)
                {
                    Notify.Error(player, "Недостаточно места в инвентаре");
                    return;
                }
                nInventory.Add(player, new nItem(ItemType.IDCard, 1, Main.Players[player].UUID));
                Notify.Succ(player, "Поздравляем! Вы получили ID-Карту");
            }
        }

        private void city_OnEntityEnterColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", shape.GetData<int>("INTERACT"));
                if (shape.HasData("DOOR")) NAPI.Data.SetEntityData(entity, "DOOR", shape.GetData<int>("DOOR"));
            }
            catch (Exception e) { Log.Write("city_OnEntityEnterColShape: " + e.Message, nLog.Type.Error); }
        }

        private void city_OnEntityExitColShape(ColShape shape, Player entity)
        {
            try
            {
                NAPI.Data.SetEntityData(entity, "INTERACTIONCHECK", 0);
            }
            catch (Exception e) { Log.Write("city_OnEntityExitColShape: " + e.Message, nLog.Type.Error); }
        }

        public static void interactPressed(Player player, int interact)
        {
            switch (interact)
            {
                case 3:
                    if (Main.Players[player].FractionID == 6 && Main.Players[player].FractionLVL > 1)
                    {
                        Doormanager.SetDoorLocked(player.GetData<int>("DOOR"), !Doormanager.GetDoorLocked(player.GetData<int>("DOOR")), 0);
                        string msg = "Вы открыли дверь";
                        if (Doormanager.GetDoorLocked(player.GetData<int>("DOOR"))) msg = "Вы закрыли дверь";
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, msg, 3000);
                    }
                    return;
                case 5:
                    if (player.IsInVehicle) return;
                    if (player.HasData("FOLLOWING"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вас кто-то тащит за собой", 3000);
                        return;
                    }
                    if (player.Position.Z < 50)
                    {
                        NAPI.Entity.SetEntityPosition(player, CityhallChecksCoords[3] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(player, CityhallChecksCoords[3] + new Vector3(0, 0, 1.12));
                    }
                    else
                    {
                        NAPI.Entity.SetEntityPosition(player, CityhallChecksCoords[2] + new Vector3(0, 0, 1.12));
                        Main.PlayerEnterInterior(player, CityhallChecksCoords[2] + new Vector3(0, 0, 1.12));
                    }
                    return;
                case 62:
                    if (Main.Players[player].FractionID != 6)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник мэрии", 3000);
                        return;
                    }
                    if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                        return;
                    }
                    if (!Stocks.fracStocks[6].IsOpen)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                        return;
                    }
                    if (!Manager.canUseCommand(player, "openweaponstock")) return;
                    player.SetData("ONFRACSTOCK", 6);
                    InvInterface.OpenOut(player, Stocks.fracStocks[6].Weapons, "Склад оружия", 6);
                    return;
            }
        }

        public static void beginWorkDay(Player player)
        {
            if (Main.Players[player].FractionID == 6)
            {
                if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы начали рабочий день", 3000);
                    Manager.setSkin(player, 6, Main.Players[player].FractionLVL);
                    NAPI.Data.SetEntityData(player, "ON_DUTY", true);
                    if (Main.Players[player].FractionLVL >= 3)
                        player.Armor = 100;
                    return;
                }
                else
                {
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закончили рабочий день", 3000);
                    Customization.ApplyCharacter(player);
                    if (player.HasData("HAND_MONEY")) player.SetClothes(5, 45, 0);
                    else if (player.HasData("HEIST_DRILL")) player.SetClothes(5, 41, 0);
                    NAPI.Data.SetEntityData(player, "ON_DUTY", false);
                    return;
                }
            }
            else Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник мэрии", 3000);
        }

        #region menu
        public static void OpenCityhallGunMenu(Player player)
        {

            if (Main.Players[player].FractionID != 6)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа", 3000);
                return;
            }
            if (!Stocks.fracStocks[6].IsOpen)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Склад закрыт", 3000);
                return;
            }
            Trigger.PlayerEvent(player, "govguns");
        }
        [RemoteEvent("govgun")]
        public static void callback_cityhallGuns(Player Player, int index)
        {
            try
            {
                switch (index)
                {
                    case 0: //"stungun":
                        Fractions.Manager.giveGun(Player, Weapons.Hash.StunGun, "stungun");
                        return;
                    case 1: //"pistol":
                        Fractions.Manager.giveGun(Player, Weapons.Hash.Pistol, "pistol");
                        return;
                    case 2: //"assaultrifle":
                        Fractions.Manager.giveGun(Player, Weapons.Hash.AdvancedRifle, "assaultrifle");
                        return;
                    case 3: //"gusenberg":
                        Fractions.Manager.giveGun(Player, Weapons.Hash.Gusenberg, "gusenberg");
                        return;
                    case 4: //"armor":
                        if (!Manager.canGetWeapon(Player, "armor")) return;

                        var aItem = nInventory.Find(Main.Players[Player].UUID, ItemType.BodyArmor);
                        if (aItem != null)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть бронежилет", 3000);
                            return;
                        }
                        nInventory.Add(Player, new nItem(ItemType.BodyArmor, 1, 100.ToString()));
                        GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "armor", 1, false);
                        Manager.FracLogs[Main.Players[Player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "" )}{DateTime.Now.Minute}", Player.Name, "Бронежилет", 1, "скрафтил" });
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили бронежилет", 3000);
                        return;
                    case 5:
                        if (!Manager.canGetWeapon(Player, "Medkits")) return;

                        if (Fractions.Stocks.fracStocks[6].Medkits == 0)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "На складе нет аптечек", 3000);
                            return;
                        }
                        var hItem = nInventory.Find(Main.Players[Player].UUID, ItemType.HealthKit);
                        if (hItem != null)
                        {
                            Notify.Send(Player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас уже есть аптечка", 3000);
                            return;
                        }
                        Fractions.Stocks.fracStocks[6].Medkits--;
                        Fractions.Stocks.fracStocks[6].UpdateLabel();
                        nInventory.Add(Player, new nItem(ItemType.HealthKit, 1));
                        GameLog.Stock(Main.Players[Player].FractionID, Main.Players[Player].UUID, "medkit", 1, false);
                        Manager.FracLogs[Main.Players[Player].FractionID].Add(new List<object> { DateTime.Now.ToString("dd.MM.yyyy"), $"{DateTime.Now.Hour}:{(DateTime.Now.Minute < 10 ? "0" : "" )}{DateTime.Now.Minute}", Player.Name, "Аптечку", 1, "скрафтил" });
                        Notify.Send(Player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили аптечку", 3000);
                        return;
                    case 6:
                        if (!Manager.canGetWeapon(Player, "PistolAmmo")) return;
                        Fractions.Manager.giveAmmo(Player, ItemType.PistolAmmo, 12);
                        return;
                    case 7:
                        if (!Manager.canGetWeapon(Player, "SMGAmmo")) return;
                        Fractions.Manager.giveAmmo(Player, ItemType.SMGAmmo, 30);
                        return;
                    case 8:
                        if (!Manager.canGetWeapon(Player, "RiflesAmmo")) return;
                        Fractions.Manager.giveAmmo(Player, ItemType.RiflesAmmo, 30);
                        return;
                }
            }
            catch (Exception e) { Log.Write("Govgun: " + e.Message, nLog.Type.Error); }
        }
        #endregion
    }
}

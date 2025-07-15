using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;
using System.IO;

namespace NeptuneEVO.Fractions
{
    class GiveLic : Script
    {
        private static nLog RLog = new nLog("IssuanceLicences");
        private static GTANetworkAPI.ColShape shapeMed;
        private static GTANetworkAPI.Marker markerMed;
        private static Vector3 Med = new Vector3(308.66675, -592.2332, 42.164062);
        private static GTANetworkAPI.ColShape shapeGun;
        private static GTANetworkAPI.Marker markerGun;
        private static Vector3 Gun = new Vector3(441.5113, -980.1197, 29.56932);
        public static int PriceMed = 15000000; // цена на мед.карту
        public static int PriceGun = 25000000; // цена на лицензию на оружие 

        [ServerEvent(Event.ResourceStart)]
        public static void EnterShapeRealtor()
        {
            try
            {
                #region Creating Marker & Colshape
                //мед.карта
                markerMed = NAPI.Marker.CreateMarker(1, Med, new Vector3(), new Vector3(), 0.5f, new Color(217, 207, 255), false, 0);
                shapeMed = NAPI.ColShape.CreateCylinderColShape(Med + new Vector3(0, 0, 0.65), 1, 1, 0);
                shapeMed.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 807);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shapeMed.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };
                //Лицензия на оружие
                markerGun = NAPI.Marker.CreateMarker(1, Gun, new Vector3(), new Vector3(), 0.5f, new Color(217, 207, 255), false, 0);
                shapeGun = NAPI.ColShape.CreateCylinderColShape(Gun + new Vector3(0, 0, 0.65), 1, 1, 0);
                shapeGun.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 808);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shapeGun.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };
                #endregion

                RLog.Write("Loaded", nLog.Type.Success);
            }
            catch (Exception e) { RLog.Write(e.ToString(), nLog.Type.Error); }
        }
        public static void MedLic(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (nInventory.Find(Main.Players[player].UUID, ItemType.IDCard) == null)
                {
                    Notify.Error(player, "У вас нет ID-Карты. Получите ее в мэрии");
                    return;
                }
                if (Main.Players[player].Licenses[7])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть мед.карта.", 3000);
                    return;
                }
                if (!MoneySystem.Wallet.Change(player, -PriceMed))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств.", 3000);
                    return;
                }
                if (Manager.countOfFractionMembers(8) > 10)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В штате есть медики, обратитесь к ним.", 3000);
                    return;
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили мед.карту", 3000);
                Main.Players[player].Licenses[7] = true;
            }
            catch (Exception e) { RLog.Write("GiveLic: " + e.Message, nLog.Type.Error); }
        }
        public static void GunLic(Player player)
        {
            try
            {

                if (!Main.Players.ContainsKey(player)) return;
                if (nInventory.Find(Main.Players[player].UUID, ItemType.IDCard) == null)
                {
                    Notify.Error(player, "У вас нет ID-Карты. Получите ее в мэрии");
                    return;
                }
                if (!Main.Players[player].Licenses[7])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет Медицинской карты! Получить ее можно в EMS", 3000);
                    return;
                }
                if (Main.Players[player].Licenses[6])
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть лицензия на оружие.", 3000);
                    return;
                }
                if (!MoneySystem.Wallet.Change(player, -PriceGun))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас недостаточно средств.", 3000);
                    return;
                }
                if (Manager.countOfFractionMembers(7) > 10)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В штате есть полицейсике, обратитесь к ним.", 3000);
                    return;
                }
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили лицензию на оружие.", 3000);
                Main.Players[player].Licenses[6] = true;
            }
            catch (Exception e) { RLog.Write("GiveLic: " + e.Message, nLog.Type.Error); }
        }
    }
}
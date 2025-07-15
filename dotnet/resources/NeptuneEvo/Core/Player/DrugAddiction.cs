using GTANetworkAPI;
using System;
using System.Linq;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class DrugAddiction : Script
    {

        private static nLog Log = new nLog("Narkos");
        private static Vector3 ColPos = new Vector3(307.05243, -595.0272, 42.164055);
        private static int healdrugmoney = 2000000;
        private static int healdrugdonate = 200;
        [ServerEvent(Event.ResourceStart)]
        public static void OnResourceStart()
        {
            try
            {
                NAPI.Marker.CreateMarker(1, ColPos, new Vector3(), new Vector3(), 0.5f, new Color(217, 207, 255), false, 0);

                ColShape shapeChangeNum = NAPI.ColShape.CreateCylinderColShape(ColPos, 2, 2, 0);
                shapeChangeNum.OnEntityEnterColShape += (s, ent) => { try { NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 160); } catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); } };
                shapeChangeNum.OnEntityExitColShape += (s, ent) => { try { NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0); } catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); } };

                Log.Write("Loaded", nLog.Type.Info);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }
        [RemoteEvent("server::drug:buy")]
        public static void Inter(Player player, int id)
        {
            if (Main.Players[player].Drug == 0)
            {
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вам не требуется лечение.", 3000);
                return;
            }
            switch (id)
            {
                case 0:
                    if (Main.Accounts[player].RedBucks < healdrugdonate)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно MCoins", 3000);
                        return;
                    }
                    Main.Players[player].Drug = 0;
                    Main.Accounts[player].RedBucks -= healdrugdonate;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы вылечились от наркозависимости.", 3000);
                    return;
                case 1:
                    if (Main.Players[player].Money < healdrugmoney)
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                        return;
                    }
                    MoneySystem.Wallet.Change(player, -healdrugmoney);
                    Main.Players[player].Drug -= 10;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы прошли часть курса по лечению от зависимости. ПРиходите еще.", 3000);
                    return;
            }
        }
        public static void SetDrug(Player player, int change)
        {
            try
            {
                Main.Players[player].Drug = change;
                Trigger.PlayerEvent(player, "UpdateDrug", Main.Players[player].Drug, Convert.ToString(change));
            }
            catch { Log.Write("ERROR SET WATER", nLog.Type.Error); }
        }
        public static void AddDrug(Player player, int change)
        {
            try
            {
                if (Main.Players[player].Drug + change > 100)
                {
                    Main.Players[player].Drug = 100;
                }
                else if (Main.Players[player].Drug + change < 0)
                {
                    Main.Players[player].Drug = 0;
                }
                else
                {
                    Main.Players[player].Drug += change;
                }
                Trigger.PlayerEvent(player, "UpdateDrug", Main.Players[player].Drug, Convert.ToString(change));
            }
            catch (Exception e)
            {
                Log.Write($"Exeption: {e}", nLog.Type.Error);
            }
        }

        public static void Lomka()
        {
            try
            {
                Log.Write("Check Lomka.", nLog.Type.Info);
                foreach (Player player in Main.Players.Keys.ToList())
                {
                    try
                    {
                        if (Main.Players[player].Drug >= 40 && Main.Players[player].Drug <= 49)
                        {
                            Trigger.PlayerEvent(player, "ragdoll", 3);
                        }
                        else if (Main.Players[player].Drug >= 50 && Main.Players[player].Drug <= 59)
                        {
                            Trigger.PlayerEvent(player, "ragdoll", 2);
                        }
                        else if (Main.Players[player].Drug >= 60 && Main.Players[player].Drug <= 69)
                        {
                            Trigger.PlayerEvent(player, "ragdoll", 1);
                        }  
                        else if (Main.Players[player].Drug >= 70 && Main.Players[player].Drug <= 89)
                        {
                            Trigger.PlayerEvent(player, "ragdoll", 0);
                            Trigger.PlayerEvent(player, "startScreenEffect", "DrugsTrevorClownsFightOut", 30000, false);
                        }  
                        else if (Main.Players[player].Drug >= 90 && Main.Players[player].Drug <= 100)
                        {
                            Trigger.PlayerEvent(player, "ragdoll", 0);
                            Trigger.PlayerEvent(player, "startScreenEffect", "DrugsTrevorClownsFightOut", 30000, false);
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                Log.Write($"Exeption: {e}", nLog.Type.Error);
            }
        }

    }
}

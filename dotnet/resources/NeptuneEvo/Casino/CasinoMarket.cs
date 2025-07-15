using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Casino
{
    class CasinoMarket : Script
    {
        #region Settings
        public static int ChipsBuy = 100;
        public static int ChipsSell = 90;

        private static nLog Log = new nLog("CasinoMarket");

        private static List<Vector3> shape = new List<Vector3>()
        {
            new Vector3(1115.912, 219.99, -50.55512),
        };
        #endregion

        #region Инициализация
        [ServerEvent(Event.ResourceStart)]
        public void Event_MarketStart()
        {
            try
            {
                #region Создание блипа, текста, колшейпа
                foreach (Vector3 vec in shape)
                {
                    var shape = NAPI.ColShape.CreateCylinderColShape(vec, 2f, 2, 0);
                    NAPI.Marker.CreateMarker(1, vec, new Vector3(), new Vector3(), 0.6f, new Color(67, 140, 239, 200));

                    shape.OnEntityEnterColShape += (shape, player) =>
                    {
                        try
                        {
                            player.SetData("INTERACTIONCHECK", 666);
                        }
                        catch (Exception e)
                        {
                            Log.Write(e.ToString(), nLog.Type.Error);
                        }
                    };
                    shape.OnEntityExitColShape += (shape, player) =>
                    {
                        try
                        {
                            player.SetData("INTERACTIONCHECK", 0);
                        }
                        catch (Exception e)
                        {
                            Log.Write(e.ToString(), nLog.Type.Error);
                        }
                    };
                    #endregion
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        public static void OpenMarketMenu(Player player)
        {
            Trigger.PlayerEvent(player, "client::casino::chips:open", ChipsBuy, ChipsSell);
        }

        [RemoteEvent("server::casino::chips:buy")]
        public static void RemoteEvent_BuyChips(Player player, int count)
        {
            try
            {
                int price = count * ChipsBuy;
                if (Main.Players[player].Money < price)
                {
                    Notify.Error(player, "Недостаточно денег!");
                    return;
                }
                MoneySystem.Wallet.Change(player, -price);
                nInventory.Add(player, new nItem(ItemType.CasinoChips, count));
                Notify.Succ(player, $"Вы купили {count} фишек за {String.Format("{0:n0}", price)}$. Фишки добавлены в ваш инвентарь");
            }
            catch(Exception ex) { Log.Write("ERROR: " + ex.Message); }
        }
        [RemoteEvent("server::casino::chips:sell")]
        public static void RemoteEvent_SellChips(Player player, int count)
        {
            int price = count * ChipsSell;
            nItem itemSell = nInventory.Find(Main.Players[player].UUID, ItemType.CasinoChips);
            if (itemSell == null || itemSell.Count < count) { Notify.Error(player, $"В вашем инвентаре нет {count} фишек"); return; };
            MoneySystem.Wallet.Change(player, -price);
            nInventory.Remove(player, ItemType.CasinoChips, count);
            Notify.Succ(player, $"Вы продали {count} фишек за {String.Format("{0:n0}", price)}$");
        }
    }
}

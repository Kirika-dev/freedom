using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Casino
{

    class CasinBar : Script
    {
        #region Settings
        private static Random rnd = new Random();
        private static nLog Log = new nLog("Casino Bar");
        private static List<Vector3> shape = new List<Vector3>()
        {
            new Vector3(1108.4199, 208.28638, -50.56009),
        };
        #endregion

        #region Инициализация Бара
        [ServerEvent(Event.ResourceStart)]
        public void Event_MarketStart()
        {
            try
            {
                #region Создание блипа, текста, колшейпа
                foreach (Vector3 vec in shape)
                {
                    var barshape = NAPI.ColShape.CreateCylinderColShape(vec, 2f, 2, 0);
                    barshape.OnEntityEnterColShape += (shape, player) =>
                    {try{player.SetData("INTERACTIONCHECK", 710);}catch (Exception e){Log.Write(e.ToString(), nLog.Type.Error);}};
                    barshape.OnEntityExitColShape += (shape, player) =>
                    {try{player.SetData("INTERACTIONCHECK", 0);}catch (Exception e){Log.Write(e.ToString(), nLog.Type.Error);}};
                    #endregion
                }
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Предметы в баре
        private static List<Product> BuyItems = new List<Product>()
        {
            new Product(100, 4, "Пиво", true),
            new Product(200, 9, "eCola", true),
            new Product(300, 26, "Champagne Bleuter", true),
            new Product(500, 25, "Jack Janiels", true),
            new Product(400, 24, "Cherenkov", true),
        };
        #endregion

        #region Открыть меню бара
        public static void OpenMarketMenu(Player player, int page)
        {
            if (player.IsInVehicle) return;
            Trigger.PlayerEvent(player, "client::casino:bar:open", Newtonsoft.Json.JsonConvert.SerializeObject(BuyItems));
        }
        #endregion

        #region Покупка
        [RemoteEvent("server::casino:bar:buy")]
        public static void SERVER_CASINO_BAR_BUY(Player player, int id, int count)
        {
            nItem aItem = new nItem((ItemType)id);
            var tryAdd = nInventory.TryAdd(player, new nItem(aItem.Type, count));
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно места в инвентаре", 2000);
                return;
            }
            var item = BuyItems.Find(x => x.ID == id);
            if (item == null)
            {
                Notify.Error(player, "Предмет не найден", 2500);
                return;
            }  
            if (id == 0)
            {
                Notify.Warn(player, "Вы не выбрали напиток", 2500);
                return;
            }
            int price = item.Ordered ? item.Price * count : item.Price * count;
            if (Main.Players[player].Money < price)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, $"Недостаточно денег", 2000);
                return;
            }
            MoneySystem.Wallet.Change(player, -price);
            nInventory.Add(player, new nItem(aItem.Type, count));
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomLeft, $"Вы купили {item.Name}", 2000);
        }
        #endregion

        #region Продукт
        private class Product
        {
            public int Price { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }
            public bool Ordered { get; set; }

            public Product(int price, int id, string name, bool ordered)
            {
                Price = price;
                ID = id;
                Name = name;
                Ordered = ordered;
            }
        }
        #endregion
    }
}

using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using NeptuneEVO.MoneySystem;
using System;
using System.Linq;


namespace NeptuneEVO.Businesses
{
    class BagShopI : Script
    {
        private static nLog Log = new nLog("CLOTHES");
        public class BagShop : BCore.Bizness
        {
            public static int CostForClothes = 1;

            public BagShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 24;
                Name = "Магазин рюкзаков";
                BlipColor = 17;
                BlipType = 676;
                Range = 2f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2) || player.GetData<bool>("ON_WORK"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить рабочий день", 3000);
                    return;
                }
                Main.Players[player].ExteriorPos = player.Position;
                Trigger.PlayerEvent(player, "client::bag:open", CostForClothes);
                player.PlayAnimation("amb@world_human_guard_patrol@male@base", "base", 1);
                NAPI.Entity.SetEntityDimension(player, Dimensions.RequestPrivateDimension(player));
            }

        }

        [RemoteEvent("server::bag:exit")]
        public static void ClientEvent_CLOSEBAG(Player player)
        {
            try
            {
                player.StopAnimation();
                player.Dimension = 0;
                Customization.ApplyCharacter(player);
                Main.Players[player].ExteriorPos = new Vector3();
            }
            catch (Exception e) { Log.Write("ClientEvent_CLOSEBAG: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("server::bag:buy")]
        public static void ClientEvent_BUYBAG(Player player, int variation, int texture)
        {
            try
            {
                Main.Players[player].ExteriorPos = player.Position;
                var tempPrice = Customization.Bags.FirstOrDefault(f => f.Variation == variation).Price;
                var price = Convert.ToInt32((tempPrice / 100.0) * BagShop.CostForClothes);

                if (Main.Players[player].Money < price)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }

                var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Bag));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                    return;
                }
                MoneySystem.Wallet.Change(player, -price);

                Customization.AddClothes(player, ItemType.Bag, variation, texture);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Вы купили новую сумку. Она была добавлена в Ваш инвентарь.", 3000);
                return;
            }
            catch (Exception e) { Log.Write("ClientEvent_BUYBAG: " + e.Message, nLog.Type.Error); }
        }
    }
}

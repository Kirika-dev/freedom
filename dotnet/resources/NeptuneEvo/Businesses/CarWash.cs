using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using System;

namespace NeptuneEVO.Businesses
{
    class CarWashI
    {
        public class CarWash : BCore.Bizness
        {
            public static int CostForWash = 15000;

            public CarWash(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 13;
                Name = "Авто мойка";
                BlipColor = 3;
                BlipType = 100;
                Range = 4f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;

                if (!player.IsInVehicle || player.IsInVehicle && player.VehicleSeat != 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в машине", 3000);
                    return;
                }
                Trigger.PlayerEvent(player, "openDialog", "CARWASH_PAY", $"Вы хотите помыть машину за {String.Format("{0:n0}", CostForWash)}?");
            }

            public static void Buy(Player player)
            {
                if (!player.IsInVehicle || player.IsInVehicle && player.VehicleSeat != 0) return;
                if (Main.Players[player].Money < CostForWash)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                    return;
                }
                GameLog.Money($"player({Main.Players[player].UUID})", $"biz(-1)", CostForWash, "carwash");
                MoneySystem.Wallet.Change(player, -CostForWash);

                VehicleStreaming.SetVehicleDirt(player.Vehicle, 0.0f);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, "Ваш транспорт был помыт", 3000);
                BattlePass.AddProgressToQuest(player, 15, 1);
            }

        }
    }
}

using GTANetworkAPI;
using NeptuneEVO.Fractions;
using Trigger = NeptuneEVO.Trigger;

namespace NeptuneEvo.Core.Phone
{
    public class GPS : Script
    {
        [RemoteEvent("server::phone:gps")]
        public static void ClientEvent_PhoneGPS(Player player, int navigator)
        {
            switch (navigator)
            {
                case 0:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", 435.1756, -981.82, 29);
                    break;   
                case 1:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", 299.77252, -584.85333, 43);
                    break;
                case 2:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", -554.94415, -186.95253, 37);
                    break;
                case 3:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", Fbi.EnterFBI.X, Fbi.EnterFBI.Y, Fbi.EnterFBI.Z);
                    break;
                case 4:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", -2449.677, 3287.597, 31);
                    break;
                case 5:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", LSNews.LSNewsCoords[0].X, LSNews.LSNewsCoords[0].Y, LSNews.LSNewsCoords[0].Z);
                    break;
                case 6:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Fractions.Manager.FractionSpawns[11].X, NeptuneEVO.Fractions.Manager.FractionSpawns[11].Y, NeptuneEVO.Fractions.Manager.FractionSpawns[11].Z);
                    break;
                case 7:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Fractions.Manager.FractionSpawns[13].X, NeptuneEVO.Fractions.Manager.FractionSpawns[13].Y, NeptuneEVO.Fractions.Manager.FractionSpawns[13].Z);
                    break;  
                case 8:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Fractions.Manager.FractionSpawns[18].X, NeptuneEVO.Fractions.Manager.FractionSpawns[18].Y, NeptuneEVO.Fractions.Manager.FractionSpawns[18].Z);
                    break;
                case 9:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Fractions.Manager.FractionSpawns[1].X, NeptuneEVO.Fractions.Manager.FractionSpawns[1].Y, NeptuneEVO.Fractions.Manager.FractionSpawns[1].Z);
                    break;
                case 10:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Fractions.Manager.FractionSpawns[2].X, NeptuneEVO.Fractions.Manager.FractionSpawns[2].Y, NeptuneEVO.Fractions.Manager.FractionSpawns[2].Z);
                    break;
                case 11:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Fractions.Manager.FractionSpawns[3].X, NeptuneEVO.Fractions.Manager.FractionSpawns[3].Y, NeptuneEVO.Fractions.Manager.FractionSpawns[3].Z);
                    break; 
                case 12:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Fractions.Manager.FractionSpawns[4].X, NeptuneEVO.Fractions.Manager.FractionSpawns[4].Y, NeptuneEVO.Fractions.Manager.FractionSpawns[4].Z);
                    break;  
                case 13:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Fractions.Manager.FractionSpawns[5].X, NeptuneEVO.Fractions.Manager.FractionSpawns[5].Y, NeptuneEVO.Fractions.Manager.FractionSpawns[5].Z);
                    break;
                //JOBS
                case 21:
                    //Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.Electrician.Position.X, NeptuneEVO.Jobs.Electrician.Position.Y, NeptuneEVO.Jobs.Electrician.Position.Z);
                    break;
                case 22:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.Gopostal.Coords[1].X, NeptuneEVO.Jobs.Gopostal.Coords[1].Y, NeptuneEVO.Jobs.Gopostal.Coords[1].Z);
                    break;
                case 23:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.Taxi.Position.X, NeptuneEVO.Jobs.Taxi.Position.Y, NeptuneEVO.Jobs.Taxi.Position.Z);
                    break;
                case 24:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.Bus.Position.X, NeptuneEVO.Jobs.Bus.Position.Y, NeptuneEVO.Jobs.Bus.Position.Z);
                    break; 
                case 25:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.Truckers.Position.X, NeptuneEVO.Jobs.Truckers.Position.Y, NeptuneEVO.Jobs.Truckers.Position.Z);
                    break; 
                case 26:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.Collector.TakeMoneyPos.X, NeptuneEVO.Jobs.Collector.TakeMoneyPos.Y, NeptuneEVO.Jobs.Collector.TakeMoneyPos.Z);
                    break;
                case 27:
                    //Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.TrashCar.TakeMoneyPos.X, NeptuneEVO.Jobs.TrashCar.TakeMoneyPos.Y, NeptuneEVO.Jobs.TrashCar.TakeMoneyPos.Z);
                    break;
                case 28:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.Builder.pos.X, NeptuneEVO.Jobs.Builder.pos.Y, NeptuneEVO.Jobs.Builder.pos.Z);
                    break;
                case 29:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.Builder.pos.X, NeptuneEVO.Jobs.Builder.pos.Y, NeptuneEVO.Jobs.Builder.pos.Z);
                    break;
                case 30:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", 1695.806, 43.05446, 160);
                    break;
                case 31:
                   // Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Jobs.Snow.Position.X, NeptuneEVO.Jobs.Snow.Position.Y, NeptuneEVO.Jobs.Snow.Position.Z);
                    break;
                case 32:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", 2016.344, 4987.024, 42);
                    break; 
                //BUSINESS
                case 41:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 1).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 1).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 1).Z);
                    break;
                case 42:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 0).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 0).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 0).Z);
                    break;
                case 43:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 22).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 22).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 22).Z);
                    break;
                case 44:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 2).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 2).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 2).Z);
                    break;
                case 45:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 4).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 4).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 4).Z);
                    break;
                case 46:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 3).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 3).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 3).Z);
                    break;
                case 47:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 5).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 5).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 5).Z);
                    break;
                case 48:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 23).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 23).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 23).Z);
                    break;
                case 49:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 25).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 25).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 25).Z);
                    break;
                case 50:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 10).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 10).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 10).Z);
                    break;
                case 51:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 15).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 15).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 15).Z);
                    break;
                case 52:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 12).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 12).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 12).Z);
                    break;
                case 53:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 13).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 13).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 13).Z);
                    break;
                case 54:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 7).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 7).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 7).Z);
                    break;
                case 55:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 9).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 9).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 9).Z);
                    break;
                case 56:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 6).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 6).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 6).Z);
                    break;
                case 57:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 11).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 11).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 11).Z);
                    break;
                case 58:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 26).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 26).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 26).Z);
                    break;
                case 59:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 21).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 21).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 21).Z);
                    break;
                case 60:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 17).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 17).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 17).Z);
                    break;
                case 61:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Businesses.BCore.getNearestBiz(player, 16).X, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 16).Y, NeptuneEVO.Businesses.BCore.getNearestBiz(player, 16).Z);
                    break;
                //BUSINESS
                //OTHER
                case 71:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.MoneySystem.ATM.GetNearestATM(player).X, NeptuneEVO.MoneySystem.ATM.GetNearestATM(player).Y, NeptuneEVO.MoneySystem.ATM.GetNearestATM(player).Z);
                    break;
                case 72:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.MoneySystem.ATM.GetNearestATM(player).X, NeptuneEVO.MoneySystem.Bank.GetNearestBANK(player).Y, NeptuneEVO.MoneySystem.Bank.GetNearestBANK(player).Z);
                    break;
                case 73:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Core.Rentcar.GetNearestRentArea(player.Position, 8).X, NeptuneEVO.Core.Rentcar.GetNearestRentArea(player.Position, 8).Y, NeptuneEVO.Core.Rentcar.GetNearestRentArea(player.Position, 8).Z);
                    break;
                case 74:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Core.Rentcar.GetNearestRentArea(player.Position, 14).X, NeptuneEVO.Core.Rentcar.GetNearestRentArea(player.Position, 14).Y, NeptuneEVO.Core.Rentcar.GetNearestRentArea(player.Position, 14).Z);
                    break;
                case 75:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Core.DrivingSchool.enterSchool.X, NeptuneEVO.Core.DrivingSchool.enterSchool.Y, NeptuneEVO.Core.DrivingSchool.enterSchool.Z);
                    break;
                case 76:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Casino.CasinoManager.entrancePosition.X, NeptuneEVO.Casino.CasinoManager.entrancePosition.Y, NeptuneEVO.Casino.CasinoManager.entrancePosition.Z);
                    break;
                case 77:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", 0, 0, 0);
                    break;
                case 78:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Entertainment.GunGame.pos.X, NeptuneEVO.Entertainment.GunGame.pos.Y, NeptuneEVO.Entertainment.GunGame.pos.Z);
                    break;
                case 79:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", 0, 0, 0);
                    break;
                case 80:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", -1634.565, -890.6557, 9);
                    break;
                case 81:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", -777.6857, -708.8641, 30);
                    break;
                case 82:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", NeptuneEVO.Utilis.VehChangeNumber.PositionChangeNumber.X, NeptuneEVO.Utilis.VehChangeNumber.PositionChangeNumber.Y, NeptuneEVO.Utilis.VehChangeNumber.PositionChangeNumber.Z);
                    break;
                case 83:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", 136.02351, -3332.7708, 6);
                    break;
            }
        }
    }
}

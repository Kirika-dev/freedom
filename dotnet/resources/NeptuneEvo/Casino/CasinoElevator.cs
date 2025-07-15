using System;
using GTANetworkAPI;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Casino
{
    class CasinoElevator : Script
    {
        private static nLog Log = new nLog("CasinoElevator");
        private static int _priceForAdmission = 0;
        private static Vector3 _entrancePosition = new Vector3(965.0325, 58.471054, 112.65301);
        private static Vector3 _exitPosition = new Vector3(1085.1727, 214.28719, -49.22043);
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var colShapeEnter = NAPI.ColShape.CreateCylinderColShape(_entrancePosition, 1f, 2, 0);
                var colShapeExit = NAPI.ColShape.CreateCylinderColShape(_exitPosition, 1f, 2, 0);

                NAPI.Marker.CreateMarker(2, _entrancePosition - new Vector3(0, 0, 0), new Vector3(), new Vector3(), 0.65f, new Color(67, 140, 239, 200), false, 0);
                NAPI.Marker.CreateMarker(2, _exitPosition - new Vector3(0, 0, 0), new Vector3(), new Vector3(), 0.65f, new Color(67, 140, 239, 200), false, 0);

                colShapeEnter.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 667);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "ENTER");
                        }
                    }
                    catch (Exception ex) { Log.Write("EnterCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeEnter.OnEntityExitColShape += OnEntityExitCasinoMainShape;

                colShapeExit.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 667);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "EXIT");
                        }
                    }
                    catch (Exception ex) { Log.Write("ExitCasino_OnEntityEnterColShape: " + ex.Message, nLog.Type.Error); }
                };
                colShapeExit.OnEntityExitColShape += OnEntityExitCasinoMainShape;
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }
        public static void OnEntityExitCasinoMainShape(ColShape shape, Player player)
        {
            NAPI.Data.SetEntityData(player, "INTERACTIONCHECK", 0);
            NAPI.Data.ResetEntityData(player, "CASINO_MAIN_SHAPE");
        }
        public static void CallBackShape(Player player)
        {
            if (!player.HasData("CASINO_MAIN_SHAPE")) return;
            string data = player.GetData<string>("CASINO_MAIN_SHAPE");
            if (data == "ENTER")
            {
                Trigger.PlayerEvent(player, "showHUD", false);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null)
                        {
                            Trigger.PlayerEvent(player, "screenFadeOut", 1000);
                        }
                    }
                    catch { }
                }, 100);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null)
                        {
                            if (player.IsInVehicle)
                            {
                                return;
                            }
                            else
                            {
                                NAPI.Entity.SetEntityPosition(player, _exitPosition);
                                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -27.5));
                                Trigger.PlayerEvent(player, "screenFadeIn", 1000);
                                Trigger.PlayerEvent(player, "showHUD", true);
                                Trigger.PlayerEvent(player, "enterCasinoWall");
                                player.SetSharedData("PLAYER_IN_CASINO", true);
                            }
                        }
                    }
                    catch { }
                }, 1600);
                return;
            }
            if (data == "EXIT")
            {
                Trigger.PlayerEvent(player, "showHUD", false);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null)
                        {
                            Trigger.PlayerEvent(player, "screenFadeOut", 1000);
                            Trigger.PlayerEvent(player, "showHUD", false);
                        }
                    }
                    catch { }
                }, 100);
                NAPI.Task.Run(() => {
                    try
                    {
                        if (player != null)
                        {
                            if (player.IsInVehicle)
                            {
                                return;
                            }
                            else
                            {
                                NAPI.Entity.SetEntityPosition(player, _entrancePosition);
                                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -27.5));
                                Trigger.PlayerEvent(player, "screenFadeIn", 1000);
                                Trigger.PlayerEvent(player, "showHUD", true);
                                Trigger.PlayerEvent(player, "exitCasinoWall");
                                player.SetSharedData("PLAYER_IN_CASINO", false);
                            }
                        }
                    }
                    catch { }
                }, 1600);
            }
        }
        public static void EnterCasino(Player player)
        {
            if (!MoneySystem.Wallet.Change(player, -_priceForAdmission))
            {
                Notify.Error(player, "У вас недостаточно средств");
                return;
            }
            Trigger.PlayerEvent(player, "cameracasinoenter", true);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        Trigger.PlayerEvent(player, "screenFadeOut", 2000);
                    }
                }
                catch { }
            }, 5000);
            NAPI.Task.Run(() => {
                try
                {
                    if (player != null)
                    {
                        Trigger.PlayerEvent(player, "cameracasinoexit", true);
                        Trigger.PlayerEvent(player, "screenFadeIn", 0);
                        NAPI.Entity.SetEntityPosition(player, _exitPosition);
                        Trigger.PlayerEvent(player, "enterCasinoWall");
                        NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -27.5));
                    }
                }
                catch { }
            }, 8000);
        }
    }
}

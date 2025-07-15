using System;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Casino
{
    class CasinoManager : Script
    {
        private static nLog Log = new nLog("CasinoManager");
        public static Vector3 entrancePosition = new Vector3(924.0211, 46.933903, 81.20635);
        private static Vector3 _exitPosition = new Vector3(1089.695, 206.015, -49);
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var colShapeEnter = NAPI.ColShape.CreateCylinderColShape(entrancePosition, 1f, 2, 0);
                var colShapeExit = NAPI.ColShape.CreateCylinderColShape(_exitPosition, 1f, 2, 0);

                NAPI.Marker.CreateMarker(1, entrancePosition - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), 0.5f, new Color(67, 140, 239, 200), false, 0);
                NAPI.Marker.CreateMarker(1, _exitPosition - new Vector3(0, 0, 1.12), new Vector3(), new Vector3(), 0.5f, new Color(67, 140, 239, 200), false, 0);

                NAPI.Blip.CreateBlip(679, entrancePosition, 1f, 67, "Diamond Casino", 255, 0, true, 0, 0);

                colShapeEnter.OnEntityEnterColShape += (s, e) =>
                {
                    try
                    {
                        if (!e.IsInVehicle)
                        {
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 805);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "ENTER");
                            Trigger.PlayerEvent(e, "client::showhintHUD", true, "Войти в казино");
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
                            NAPI.Data.SetEntityData(e, "INTERACTIONCHECK", 805);
                            NAPI.Data.SetEntityData(e, "CASINO_MAIN_SHAPE", "EXIT");
                            Trigger.PlayerEvent(e, "client::showhintHUD", true, "Выйти из казино");
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
            Trigger.PlayerEvent(player, "client::showhintHUD", false, null);
        }
        public static void CallBackShape(Player player)
        {
            if (!player.HasData("CASINO_MAIN_SHAPE")) return;
            string data = player.GetData<string>("CASINO_MAIN_SHAPE");
            if (data == "ENTER")
            {
                NAPI.Entity.SetEntityPosition(player, entrancePosition);
                NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -6));
                NAPI.Entity.SetEntityDimension(player, (uint)player.Value + 2000);
                Trigger.PlayerEvent(player, "client::cameracasinoenter");
                BattlePass.AddProgressToQuest(player, 3, 1);
                return;
            }
            if (data == "EXIT")
            {
                Trigger.PlayerEvent(player, "client::fadescreen", 800);
                NAPI.Task.Run(() =>
                {
                    if (player != null)
                    {
                        NAPI.Entity.SetEntityPosition(player, entrancePosition);
                        NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, 63));
                        player.SetSharedData("PLAYER_IN_CASINO", false);
                    }
                }, 1000);
            }
        }
        [RemoteEvent("server::setposCasino")]
        public static void setposCasino(Player player)
        {
            NAPI.Entity.SetEntityDimension(player, 0);
            NAPI.Entity.SetEntityPosition(player, _exitPosition);
            NAPI.Entity.SetEntityRotation(player, new Vector3(0, 0, -120));
            player.SetSharedData("PLAYER_IN_CASINO", true);
            Trigger.PlayerEvent(player, "cameracasinoexit");
        }
    }
}

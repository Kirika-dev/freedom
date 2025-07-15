using GTANetworkAPI;
using NeptuneEVO.SDK;
using System;
using NeptuneEVO;
using NeptuneEVO.Core;

namespace NeptuneEVO.Core
{
    class Drone : Script
    {
        private static nLog Log = new nLog("DroneScript");
        [RemoteEvent("startSpecMission")]
        public void startSpecMission(Player client)
        {
            if (!Main.Players.ContainsKey(client)) return;

            if (!Group.CanUseCmd(client, "drone")) return;

            try
            {
                var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey("rcbandito"), client.Position, client.Rotation.Z, 0, 0, "DRONE", 0, true, true, client.Dimension);
                veh.SetData("ACCESS", "ADMIN");
                veh.SetData("BY", client.Name);

                client.SetIntoVehicle(veh, 0);

                long drone_model;
                switch (client.GetData<int>("drone_type"))
                {
                    default:
                        drone_model = Convert.ToInt64(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.LSPDDrone));
                        break;
                    case 1:
                        drone_model = NAPI.Util.GetHashKey("xs_prop_arena_drone_02"); // -388213579
                        break;
                    case 2:
                        drone_model = NAPI.Util.GetHashKey("ba_prop_battle_cameradrone");
                        break;
                    case 3:
                        drone_model = NAPI.Util.GetHashKey("ba_prop_battle_drone_hornet");
                        break;
                }

                NAPI.Task.Run(() =>
                {
                    veh.Transparency = 0;
                    BasicSync.AttachObjectToPlayer(client, (uint)drone_model, 0, new Vector3(0, 0, 0), new Vector3(0, 0, 0));

                    veh.SetSharedData("markAsDrone", true);
                    Trigger.PlayerEventInRange(veh.Position, 300, "vSync:Sound", veh.Id);
                }, 250);
            }
            catch (Exception e) { Log.Write($"startSpecMission/: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        [RemoteEvent("startSpecMissionLspd")]
        public void startSpecMissionLspd(Player client)
        {
            if (!Main.Players.ContainsKey(client)) return;

            if (Main.Players[client].FractionID != 7 && Main.Players[client].FractionID != 9)
            {
                Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник!", 3000);
                return;
            }
            if (!NAPI.Data.GetEntityData(client, "ON_DUTY"))
            {
                Notify.Send(client, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                return;
            }

            try
            {
                Trigger.PlayerEvent(client, "client:drone:status", true);
                var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey("rcbandito"), client.Position, client.Rotation.Z, 0, 0, "DRONE", 0, true, true, client.Dimension);
                veh.SetData("BY", client.Name);

                client.SetIntoVehicle(veh, 0);

                NAPI.Task.Run(() =>
                {
                    veh.Transparency = 0;
                    BasicSync.AttachObjectToPlayer(client, (uint)Convert.ToInt64(nInventory.InventoryItems.Find(x => x.ItemType == ItemType.LSPDDrone)), 0, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    NAPI.Data.SetEntityData(client, "positionDrone", client.Position);
                    veh.SetSharedData("markAsDrone", true);
                    veh.SetSharedData("fraction_id", Main.Players[client].FractionID);
                    veh.SetSharedData("dispatchMarked", $"X-{client.Id}");
                    Trigger.PlayerEventInRange(veh.Position, 300, "vSync:Sound", veh.Id);
                }, 250);
            }
            catch (Exception e) { Log.Write($"startSpecMissionLspd/: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        [RemoteEvent("stopSpecMission")]
        public void stopSpecMission(Player client)
        {
            try
            {
                if (!client.IsInVehicle) return;
                Vehicle veh = client.Vehicle;
                if (veh.HasData("ACCESS") && veh.GetData<string>("ACCESS") == "ADMIN") veh.Delete();

                Trigger.PlayerEvent(client, "client:drone:status", false);

                BasicSync.DetachObject(client);
            }
            catch (Exception e) { Log.Write($"stopSpecMission/: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        [RemoteEvent("stopSpecMissionLspd")]
        public void stopSpecMissionLspd(Player client)
        {
            try
            {
                if (!client.IsInVehicle) return;
                Vehicle veh = client.Vehicle;
                
                veh.Delete();
                NAPI.Entity.SetEntityPosition(client, NAPI.Data.GetEntityData(client, "positionDrone"));
                Trigger.PlayerEvent(client, "client:drone:status", false);

                BasicSync.DetachObject(client);
            }
            catch (Exception e) { Log.Write($"stopSpecMissionLspd/: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }

        public void showKeykodesNotify(Player client)
        {
            try
            {
                Notify.Send(client, NotifyType.Info, NotifyPosition.BottomCenter, "Управление положением: WASD, Управление высотой: Z/Q, Режимы камеры: Tab, Выход - ESC", 3000);
            }
            catch (Exception e) { Log.Write($"startSpecMission/: {e.ToString()}\n{e.StackTrace}", nLog.Type.Error); }
        }
    }
}

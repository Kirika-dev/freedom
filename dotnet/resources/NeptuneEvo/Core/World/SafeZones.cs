using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class SafeZones : Script
    {
        private static nLog Log = new nLog("SafeZones");
        public static void CreateSafeZone(Vector3 position, int range, int height, uint dim = 0, int max = 540, bool col = true, string name = "Green")
        {
            var colShape = NAPI.ColShape.CreateCylinderColShape(position, range, height, dim);
            colShape.OnEntityEnterColShape += (shape, entity) =>
            {
                try
                {
                    Trigger.PlayerEvent(entity, "safeZone", true, max, name);
                    if (col)
                        entity.SetSharedData("InSafeZone", true);
                }
                catch (Exception e) { Log.Write($"SafeZoneEnter: {e.Message}", nLog.Type.Error); }

            };
            colShape.OnEntityExitColShape += (shape, entity) =>
            {
                try
                {
                    Trigger.PlayerEvent(entity, "safeZone", false, max, name);
                    if (col)
                        entity.SetSharedData("InSafeZone", false);
                }
                catch (Exception e) { Log.Write($"SafeZoneExit: {e.Message}", nLog.Type.Error); }
            };
        }

        [ServerEvent(Event.ResourceStart)]
        public void Event_onResourceStart()
        {
            CreateSafeZone(new Vector3(-538.7153, -214.66, 36.52974), 120, 40); // мерия
            CreateSafeZone(new Vector3(-804.4073, -224.9438, 36.10337), 20, 20); // салон
            CreateSafeZone(new Vector3(224.5672, -610.4773, 8.761386), 150, 40); // емс
            CreateSafeZone(new Vector3(906.8802, -2919.172, 7.482737), 90, 20); // контейнеры
            CreateSafeZone(new Vector3(924.0211, 46.933903, 81.20635), 60, 20); // Казино
            CreateSafeZone(new Vector3(1105.7426, 210.61623, -49.44009), 120, 15, name: "Casino"); // Внутри казино
            CreateSafeZone(new Vector3(1105.7426, 210.61623, -49.44009), 120, 15, 1); // Внутри казино 1 дименшн
            CreateSafeZone(new Vector3(-1626.5548, -870.18665, 9.495345), 140, 30); // Б/У рынок
            CreateSafeZone(new Vector3(14.078935, -1070.0706, 38.11777), 100, 30); // Парковка
            CreateSafeZone(new Vector3(-911.61945, -2041.6262, 8.28443), 100, 30); // Лицензирование
        }
    }
}

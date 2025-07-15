using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using GTANetworkAPI;
using NeptuneEVO.Core;
using Newtonsoft.Json;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Fractions
{
    class Configs : Script
    {
        private static nLog Log = new nLog("FractionConfigs");
        // fractionid - vehicle number - vehiclemodel, position, rotation, min rank, color1, color2
        public static Dictionary<int, Dictionary<string, Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>>> FractionVehicles = new Dictionary<int, Dictionary<string, Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>>>();
        public static Dictionary<int, string> FractionTypes = new Dictionary<int, string>()
        {
            { 1, "FAMILY" },
            { 2, "BALLAS" },
            { 3, "VAGOS" },
            { 4, "MARABUNTA" },
            { 5, "BLOOD" },
            { 6, "CITY" },
            { 7, "POLICE" },
            { 8, "EMS" },
            { 9, "FIB" },
            { 10, "LCN" },
            { 11, "RUSSIAN" },
            { 12, "YAKUZA" },
            { 13, "ARMENIAN" },
            { 14, "ARMY" },
            { 15, "LSNEWS" },
            { 16, "THELOST" },
            { 17, "PMC" },
			{ 18, "NACBEZ" },
			{ 19, "ФЕРМА" },
        };
        // fractionid - ranknumber - rankname, rankclothes
        public static Dictionary<int, Dictionary<int, Tuple<string, string, string, int>>> FractionRanks = new Dictionary<int, Dictionary<int, Tuple<string, string, string, int>>>();
        // fractionid - commandname, minrank
        public static Dictionary<int, Dictionary<string, int>> FractionCommands = new Dictionary<int, Dictionary<string, int>>();
        // fractionid - commandname, minrank
        public static Dictionary<int, Dictionary<string, int>> FractionWeapons = new Dictionary<int, Dictionary<string, int>>();

        public static Dictionary<string, string> FractionVehiclesNames = new Dictionary<string, string>();
        public static void LoadFractionConfigs()
        {
            for (int i = 1; i <= 19; i++)
                FractionVehicles.Add(i, new Dictionary<string, Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>>());
            for (int i = 1; i <= 19; i++)
                FractionRanks.Add(i, new Dictionary<int, Tuple<string, string, string, int>>());
            for (int i = 1; i <= 19; i++)
                FractionCommands.Add(i, new Dictionary<string, int>());
            for (int i = 1; i <= 19; i++)
                FractionWeapons.Add(i, new Dictionary<string, int>());

            // loading fraction vehicle configs and spawn
            DataTable result = MySQL.QueryRead("SELECT * FROM `fractionvehicles`");
            if (result == null || result.Rows.Count == 0) return;
            foreach (DataRow Row in result.Rows)
            {
                var fraction = Convert.ToInt32(Row["fraction"]);
                var number = Row["number"].ToString();
                var model = (VehicleHash)NAPI.Util.GetHashKey(Row["model"].ToString());
                var modelName = Convert.ToString(Row["model"]);
                var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                var minrank = Convert.ToInt32(Row["rank"]);
                var color1 = Convert.ToInt32(Row["colorprim"]);
                var color2 = Convert.ToInt32(Row["colorsec"]);
                VehicleManager.VehicleCustomization components = JsonConvert.DeserializeObject<VehicleManager.VehicleCustomization>(Row["components"].ToString());
                FractionVehiclesNames.Add(number, modelName);
                FractionVehicles[fraction].Add(number, new Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>(model, position, rotation, minrank, color1, color2, components));
            }

			
				
             foreach (var fraction in FractionVehicles.Keys)
             SpawnFractionCars(fraction);

            // load fraction ranks configs
            result = MySQL.QueryRead("SELECT * FROM `fractionranks`");
            if (result == null || result.Rows.Count == 0) return;
            foreach (DataRow Row in result.Rows)
            {
                var fraction = Convert.ToInt32(Row["fraction"]);
                var rank = Convert.ToInt32(Row["rank"]);
                var payday = Convert.ToInt32(Row["payday"]);
                var name = Row["name"].ToString();
                var clothesm = Row["clothesm"].ToString();
                var clothesf = Row["clothesf"].ToString();

                FractionRanks[fraction].Add(rank, new Tuple<string, string, string, int>(name, clothesm, clothesf, payday));
            }

            result = MySQL.QueryRead("SELECT * FROM `fractionaccess`");
            if (result == null || result.Rows.Count == 0) return;
            foreach (DataRow Row in result.Rows)
            {
                var fraction = Convert.ToInt32(Row["fraction"]);
                var dictionaryCmd = JsonConvert.DeserializeObject<Dictionary<string, int>>(Row["commands"].ToString());
                var dictionaryWeap = JsonConvert.DeserializeObject<Dictionary<string, int>>(Row["weapons"].ToString());

                FractionCommands[fraction] = dictionaryCmd;
                FractionWeapons[fraction] = dictionaryWeap;
            }

            Manager.onResourceStart();
        }

        public static void SpawnFractionCars(int fraction)
        {
            foreach (var vehicle in FractionVehicles[fraction])
            {
                var model = vehicle.Value.Item1;
				if (model != VehicleHash.Barracks) continue;
                var canmats = (model == VehicleHash.Barracks || model == VehicleHash.Youga || model == VehicleHash.Burrito3 || model == VehicleHash.Rumpo3); // "CANMATS"
                var candrugs = (model == VehicleHash.Youga || model == VehicleHash.Burrito3); // "CANDRUGS"
                var canmeds = (model == (VehicleHash)NAPI.Util.GetHashKey("emsnspeedo")); // "CANMEDKITS"
                var veh = NAPI.Vehicle.CreateVehicle(model, vehicle.Value.Item2, vehicle.Value.Item3, vehicle.Value.Item5, vehicle.Value.Item6);

                NAPI.Data.SetEntityData(veh, "ACCESS", "FRACTION");
                NAPI.Data.SetEntityData(veh, "FRACTION", fraction);
                NAPI.Data.SetEntityData(veh, "MINRANK", vehicle.Value.Item4);
                NAPI.Data.SetEntityData(veh, "TYPE", FractionTypes[fraction]);
                if (canmats)
                    NAPI.Data.SetEntityData(veh, "CANMATS", true);
                if (candrugs)
                    NAPI.Data.SetEntityData(veh, "CANDRUGS", true);
                if (canmeds)
                    NAPI.Data.SetEntityData(veh, "CANMEDKITS", true);
                NAPI.Vehicle.SetVehicleNumberPlate(veh, vehicle.Key);
                Core.VehicleStreaming.SetEngineState(veh, false);
                VehicleManager.FracApplyCustomization(veh, fraction);
                if (model == VehicleHash.Submersible || model == VehicleHash.Thruster) veh.SetSharedData("PETROL", 0);
            }
        }
        public static void RespawnFractionCar(Vehicle vehicle)
        {
            try
            {
                var canmats = vehicle.HasData("CANMATS");
                var candrugs = vehicle.HasData("CANDRUGS");
                var canmeds = vehicle.HasData("CANMEDKITS");
                string number = vehicle.NumberPlate;
                int fraction = vehicle.GetData<int>("FRACTION");

                NAPI.Entity.SetEntityPosition(vehicle, FractionVehicles[fraction][number].Item2);
                NAPI.Entity.SetEntityRotation(vehicle, FractionVehicles[fraction][number].Item3);
                VehicleManager.RepairCar(vehicle);
                NAPI.Data.SetEntityData(vehicle, "ACCESS", "FRACTION");
                NAPI.Data.SetEntityData(vehicle, "FRACTION", fraction);
                NAPI.Data.SetEntityData(vehicle, "MINRANK", FractionVehicles[fraction][number].Item4);
                if (canmats)
                    NAPI.Data.SetEntityData(vehicle, "CANMATS", true);
                if (candrugs)
                    NAPI.Data.SetEntityData(vehicle, "CANDRUGS", true);
                if (canmeds)
                    NAPI.Data.SetEntityData(vehicle, "CANMEDKITS", true);
                NAPI.Vehicle.SetVehicleNumberPlate(vehicle, number);
                Core.VehicleStreaming.SetEngineState(vehicle, false);
                VehicleManager.FracApplyCustomization(vehicle, fraction);
            }
            catch (Exception e) { Log.Write("RespawnFractionCar: " + e.Message, nLog.Type.Error); }
        }
    }
}

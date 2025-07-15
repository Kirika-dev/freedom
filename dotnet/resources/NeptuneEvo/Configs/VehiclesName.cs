using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;

namespace NeptuneEVO.Utilis
{
    public static class VehiclesName
    {
        public static Dictionary<string, string> ModelList = new Dictionary<string, string>()
        {
            { "name", "Test Car" },
        };

        public static string GetRealVehicleName(string model)
        {
            if (ModelList.ContainsKey(model))
            {
                return ModelList[model];
            }
            else
            {
                return model;
            }
        }

        public static string GetVehicleModelName(string name)
        {
            if (ModelList.ContainsValue(name))
            {
                return ModelList.FirstOrDefault(x => x.Value == name).Key;
            }
            else
            {
                return name;
            }
        }
        public static string GetRealVehicleNameHash(VehicleHash model)
        {
            if (ModelList2.ContainsKey(model))
            {
                return ModelList2[model];
            }
            else
            {
                return "null";
            }
        }
        public static Dictionary<VehicleHash, string> ModelList2 = new Dictionary<VehicleHash, string>()
        {
              { (VehicleHash)NAPI.Util.GetHashKey("a6"), "Audi A6" },
        };
    }
}

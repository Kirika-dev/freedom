using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEvo;
using Newtonsoft.Json;
using System.Data;
using NeptuneEVO;
using NeptuneEVO.Core;

namespace NeptuneEVO.Utils.Promocodes
{
    class Promocodes : Script
    {
        private static nLog Log = new nLog("Promocodes");
        public static List<Promocode> PromoList = new List<Promocode>();
        public static List<string> PromoListNames = new List<string>();
        public static List<string> MediaPromo = new List<string>();
        public static Dictionary<string, string> CarPromo = new Dictionary<string, string>();
        public static Dictionary<string, DateTime> PromoListLimits = new Dictionary<string, DateTime>();

        [ServerEvent(Event.ResourceStart)]
        public static void ResourceStart()
        {
            var result = MySQL.QueryRead($"SELECT * FROM `promo`");
            foreach (DataRow Row in result.Rows)
            {
                string promoName = Convert.ToString(Row["name"]);
                string type = Convert.ToString(Row["type"]);
                int prize = Convert.ToInt32(Row["prize"]);
                int used = Convert.ToInt32(Row["used"]);
                int usedandget = Convert.ToInt32(Row["usedandget"]);
                int time = Convert.ToInt32(Row["time"]);
                DateTime limit = (DateTime)Row["limitdate"];
                Promocode data = new Promocode(promoName, type, prize, used, usedandget, time, limit);
                if (type == "yt")
                {
                    MediaPromo.Add(promoName);
                }
                if(type == "car")
                {
                    CarPromo.Add(promoName, type); //type = car name
                }
                PromoList.Add(data);
                PromoListNames.Add(promoName);
                PromoListLimits.Add(promoName, limit);
            }
            if (PromoList.Count != 0)
                Log.Write($"Load promocodes: {PromoList.Count}");
        }

        [RemoteEvent("Promo_Logic")]
        public static void Promo_Logic(Player player, params object[] args)
        {
            switch (Convert.ToString(args[0]))
            {
                case "open":
                    List<List<object>> allPromo = new List<List<object>>();
                    for (int i = 0; i < PromoList.Count; i++)
                    {
                        if (!Main.Players[player].ActivePromocodes.ContainsKey(PromoList[i].Name) && !Main.Players[player].UsedPromocodes.ContainsKey(PromoList[i].Name)) continue;
                        string time = "";
                        int active = 0;
                        if (Main.Players[player].ActivePromocodes.ContainsKey(PromoList[i].Name))
                        {
                            active = 1;
                            if (Main.Players[player].ActivePromocodes[PromoList[i].Name] < 60) time = $"00:{Main.Players[player].ActivePromocodes[PromoList[i].Name]}";
                            if (Main.Players[player].ActivePromocodes[PromoList[i].Name] < 10) time = $"00:0{Main.Players[player].ActivePromocodes[PromoList[i].Name]}";
                            else time = $"{Main.Players[player].ActivePromocodes[PromoList[i].Name] / 60}:{Main.Players[player].ActivePromocodes[PromoList[i].Name] % 60}";
                        }
                        else active = 0;
                        int complited = 0;
                        if (Main.Players[player].UsedPromocodes.ContainsKey(PromoList[i].Name)) complited = Main.Players[player].UsedPromocodes[PromoList[i].Name] ? 1 : 0;
                        else complited = 0;
                        string time2 = $"";
                        if (PromoList[i].Time < 60) time2 = $"00:{PromoList[i].Time} минут";
                        if (PromoList[i].Time < 10) time2 = $"00:0{PromoList[i].Time} минут";
                        else time2 = $"{PromoList[i].Time / 60}:{PromoList[i].Time % 60} часов";
                        List<object> obj = new List<object>
                        {
                            PromoList[i].Name,
                            active,
                            complited,
                            PromoList[i].Prize,
                            time,
                            time2,
                        };
                        allPromo.Add(obj);
                    }
                    NAPI.ClientEvent.TriggerClientEvent(player, "SetPromoHistory", JsonConvert.SerializeObject(allPromo));
                    break;
                case "use":
                    string promo = Convert.ToString(args[1]);
                    if (!PromoListNames.Contains(promo))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Промокода [{promo}] не существует.", 4000);
                        return;
                    }
                    if (MediaPromo.Contains(promo))
                    {
                        for (int i = 0; i < PromoList.Count; i++)
                        {
                            if (!MediaPromo.Contains(PromoList[i].Name)) continue;
                            if (Main.Players[player].UsedPromocodes.ContainsKey(PromoList[i].Name))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже использовали промокод медиа-партнёра сервера.", 4000);
                                return;
                            }
                            else if (Main.Players[player].ActivePromocodes.ContainsKey(PromoList[i].Name))
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже использовали промокод медиа-партнёра сервера.", 4000);
                                return;
                            }
                        }
                    }
                    if (PromoListLimits[promo] <= DateTime.Now)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Срок действия промокода истёк.", 4000);
                        return;
                    }
                    if (Main.Players[player].ActivePromocodes.ContainsKey(promo))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже использовали этот промокод.", 4000);
                        return;
                    }
                    if (Main.Players[player].UsedPromocodes.ContainsKey(promo))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы уже использовали этот промокод.", 4000);
                        return;
                    }
                    if (CarPromo.ContainsKey(promo))
                    {
                        for (int i = 0; i < PromoList.Count; i++)
                        {
                            if (PromoList[i].Limit <= DateTime.Now) continue;
                            if (promo != PromoList[i].Name) continue;
                            Main.Players[player].ActivePromocodes.Add(PromoList[i].Name, PromoList[i].Time);
                            PromoList[i].Used++;
                            MySQL.Query($"UPDATE `promo` SET `used`='{PromoList[i].Used}' WHERE `name`='{PromoList[i].Name}'");
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно применили промокод!", 4000);
                        }
                        Promo_Logic(player, "open");
                        return;
                    }
                    for (int i = 0; i < PromoList.Count; i++)
                    {
                        if (PromoList[i].Limit <= DateTime.Now) continue;
                        if (promo != PromoList[i].Name) continue;
                        Main.Players[player].ActivePromocodes.Add(PromoList[i].Name, PromoList[i].Time);
                        PromoList[i].Used++;
                        MySQL.Query($"UPDATE `promo` SET `used`='{PromoList[i].Used}' WHERE `name`='{PromoList[i].Name}'");
                        PlayerConnected(player);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы успешно применили промокод!", 4000);
                    }
                    Promo_Logic(player, "open");
                    break;
            }
        }

        public static void PlayerConnected(Player player)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].ActivePromocodes.Count == 0) return;
                Timer(player);
            }
            catch { }
        }

        public static void Timer(Player player)
        {
            try
            {
                NAPI.Task.Run(() =>
                {
                    if (!Main.Players.ContainsKey(player)) return;
                    var acc = Main.Players[player];
                    for (int i = 0; i < PromoList.Count; i++)
                    {
                        if (!Main.Players.ContainsKey(player)) return;
                        if (!acc.ActivePromocodes.ContainsKey(PromoList[i].Name)) continue;
                        acc.ActivePromocodes[PromoList[i].Name]--;
                        if (acc.ActivePromocodes[PromoList[i].Name] <= 0)
                        {
                            PromoList[i].UsedAndGet++;
                            MySQL.Query($"UPDATE `promo` SET `usedandget`='{PromoList[i].UsedAndGet}' WHERE `name`='{PromoList[i].Name}'");
                            if (PromoList[i].Name == "ОБНОВА")
                            {
                                string vname = "r8v10";
                                var house = Houses.HouseManager.GetHouse(player, true);
                                if (house == null)
                                {
                                    var vNumber = VehicleManager.Create(player.Name, vname, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выйграли {Utilis.VehiclesName.GetRealVehicleName(vname)} с номером {vNumber}", 5000);
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени она будет доставлена на стоянку", 5000);
                                }
                                else
                                {
                                    var garage = Houses.GarageManager.Garages[house.GarageID];
                                    var vNumber = VehicleManager.Create(player.Name, vname, new Color(0, 0, 0), new Color(0, 0, 0), new Color(0, 0, 0));
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы выйграли {Utilis.VehiclesName.GetRealVehicleName(vname)} с номером {vNumber}", 5000);
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"В скором времени она будет доставлена в Ваш гараж", 5000);
                                    garage.SpawnCar(vNumber);
                                }
                            }
                            else
                            {
                                MoneySystem.Wallet.Change(player, PromoList[i].Prize);
                            }
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили награду {PromoList[i].Prize}$ за использование промокода [{PromoList[i].Name}].", 4000);
                            acc.ActivePromocodes.Remove(PromoList[i].Name);
                            acc.UsedPromocodes.Add(PromoList[i].Name, true);
                        }
                    }
                    Promo_Logic(player, "open");
                });
            }
            catch { }
        }
    }
}

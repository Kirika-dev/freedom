﻿using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using System.Linq;
using System.Data;

namespace NeptuneEVO.Houses
{

    public class Apartments : Script
    {

        static nLog Log = new nLog("Apartaments");

        public static Dictionary<int, ApartmentParent> ApartmentList = new Dictionary<int, ApartmentParent>();

        [ServerEvent(Event.ResourceStart)]

        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM aparts");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB rod return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    Vector3 pos = JsonConvert.DeserializeObject<Vector3>(Row["pos"].ToString());
                    Vector3 garpos = JsonConvert.DeserializeObject<Vector3>(Row["garpos"].ToString());
                    List<int> houses = JsonConvert.DeserializeObject<List<int>>(Row["houses"].ToString());

                    ApartmentList.Add(Convert.ToInt32(Row["id"].ToString()), new ApartmentParent(Row["name"].ToString(), Convert.ToInt32(Row["id"].ToString()), houses, pos, garpos, Convert.ToInt32(Row["heading"].ToString())));
                }
                GarageManager.onResourceStart();
            }
            catch (Exception e)
            {
                Log.Write("onResourceStart: " + e.ToString(), nLog.Type.Error);
            }
        }


        [RemoteEvent("server::interact")]
        public static void RM_interact(Player player, int index)
        {
            try
            {

                if (!Main.Players.ContainsKey(player)) return;
                if (!player.HasData("APARTMENT")) return;

                ApartmentParent parent = player.GetData<ApartmentParent>("APARTMENT");

                House house = HouseManager.Houses.FirstOrDefault(h => h.ID == index);
                if (house == null) return;

                if (house.Owner == "")
                {
                    Trigger.PlayerEvent(player, "openDialog", "BUY_APART", $"Вы хотите купить квартиру под №{index} за {house.Price}$ ?");
                    player.SetData("APART_HOUSE", house);
                }
                else
                {
                    if (house.Locked)
                    {
                        var playerHouse = HouseManager.GetApart(player);
                        if (playerHouse != null && playerHouse.ID == house.ID)
                            house.SendPlayer(player);
                        else if (player.HasData("InvitedHouse_ID") && player.GetData<int>("InvitedHouse_ID") == house.ID)
                            house.SendPlayer(player);
                        else
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет доступа", 3000);
                    }
                    else
                        house.SendPlayer(player);
                }

                player.ResetData("APARTMENT");


            }
            catch (Exception e) { Log.Write("interact: " + e.ToString()); }
        }

        [Command("createapart")]
        static void CMD_createapart(Player player, int id, string name)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;
                if (ApartmentList.ContainsKey(id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Уже существует такой айди апартаментов!", 3000);
                    return;
                }

                ApartmentList.Add(id, new ApartmentParent(name, id, new List<int>(), player.Position - new Vector3(0,0,1.12), new Vector3(), (int)player.Heading ));

                MySQL.Query($"INSERT INTO aparts (id, name, pos) " + $"VALUES ({id},'{name}','{JsonConvert.SerializeObject( player.Position - new Vector3(0, 0, 1.12) )}')");

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Успешно созданы апартаменты < {name} ({id}) >", 3000);
            }
            catch (Exception e) { Log.Write("createapart: " + e.ToString()); }
        }

        [Command("garageapart")]
        static void CMD_garageapart(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;
                if (!ApartmentList.ContainsKey(id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Не существует такой айди апартаментов!", 3000);
                    return;
                }

                ApartmentList[id].GaragePos = player.Position - new Vector3(0, 0, 1.12);
                ApartmentList[id].Heading = (int)player.Heading;

                ApartmentList[id].Save();

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы изменили точку гаража", 3000);

            }
            catch (Exception e) { Log.Write("garageapart: " + e.ToString()); }
        }


        [Command("addhouseapart")]
        static void CMD_addhouseapart(Player player, int id, int houseid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;
                if (!ApartmentList.ContainsKey(id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Не существует такой айди апартаментов!", 3000);
                    return;
                }

                ApartmentParent parent = ApartmentList[id];

                if (parent.Houses.Contains(houseid))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В списке квартир уже имеется данный дом", 3000);
                    return;
                }

                House house = HouseManager.Houses.FirstOrDefault(h => h.ID == houseid);
                if (house == null) return;

                house.Apart = id;
                MySQL.Query($"UPDATE `houses` SET `apart`='{id}' WHERE `id`='{houseid}'");

                parent.Houses.Add(houseid);
                parent.Save();

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Добавлен новый дом под №{houseid}", 3000);

            }
            catch(Exception e) { Log.Write("addhouseapart: " + e.ToString()); }
        }

        [Command("delhouseapart")]
        static void CMD_delhouseapart(Player player, int id, int houseid)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;
                if (!ApartmentList.ContainsKey(id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Не существует такой айди апартаментов!", 3000);
                    return;
                }

                ApartmentParent parent = ApartmentList[id];

                if (!parent.Houses.Contains(houseid))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В списке квартир не имеется данный дом", 3000);
                    return;
                }

                House house = HouseManager.Houses.FirstOrDefault(h => h.ID == houseid);
                if (house == null) return;

                house.Apart = -1;
                MySQL.Query($"UPDATE `houses` SET `apart`='{-1}' WHERE `id`='{houseid}'");

                parent.Houses.Remove(houseid);
                parent.Save();

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Убран дом под №{houseid}", 3000);

            }
            catch (Exception e) { Log.Write("delhouseapart: " + e.ToString()); }
        }

        [Command("deleteapart")]
        static void CMD_deleteapart(Player player, int id)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                if (Main.Players[player].AdminLVL < 9) return;
                if (!ApartmentList.ContainsKey(id))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Не существует такой айди апартаментов!", 3000);
                    return;
                }
                ApartmentList[id].Destroy();
                ApartmentList.Remove(id);

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы убрали апартаменты под №{id}", 3000);
            }
            catch (Exception e) { Log.Write("deleteapart: " + e.ToString()); }
        }
    }

    public class ApartmentParent
    {
        public string Name;
        public int ID;
        public List<int> Houses;
        public Vector3 Pos;
        public Vector3 GaragePos;
        public int Heading;
        public Blip Blip;

        public ApartmentParent(string name, int id, List<int> houses, Vector3 posi, Vector3 garpos, int rot)
        {

            Name = name; ID = id; Houses = houses; Pos = posi; GaragePos = garpos; Heading = rot;

            Blip = NAPI.Blip.CreateBlip(475, Pos, 0.8f, Convert.ToByte(53), Main.StringToU16("Апартаменты"), 255, 0, true, 0, 0);

            ColShape shape = NAPI.ColShape.CreateCylinderColShape(Pos, 1.5f, 2f);
            shape.SetData("APARTMENT", this);
            shape.OnEntityEnterColShape += (s, entity) =>
            {
                try
                {
                    entity.SetData("INTERACTIONCHECK", 525);
                    entity.SetData("APARTMENT", s.GetData<ApartmentParent>("APARTMENT"));
                    Trigger.PlayerEvent(entity, "client::showhintHUD", true, $"Апартаменты {Name}");
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };
            shape.OnEntityExitColShape += (s, entity) =>
            {
                try
                {
                    entity.SetData("INTERACTIONCHECK", 0);
                    entity.ResetData("APARTAMENT");
                    Trigger.PlayerEvent(entity, "client::showhintHUD", false, null);
                }
                catch (Exception e) { Console.WriteLine("shape.OnEntityEnterColshape: " + e.Message); }
            };

            NAPI.Marker.CreateMarker(1, Pos, new Vector3(), new Vector3(), 0.5f, new Color(67, 140, 239, 200), false, 0);
        }

        public void Save()
        {
            try
            {
                MySQL.Query($"UPDATE aparts SET garpos='{JsonConvert.SerializeObject(GaragePos)}',houses='{JsonConvert.SerializeObject(Houses)}', heading={Heading} WHERE id={ID}");
            }
            catch { }
        }

        public void Destroy()
        {
            NAPI.Task.Run(() => {
                try
                {
                   // foreach (TextLabel obj in Texts) obj.Delete();
                   // foreach (Marker obj in Markers) obj.Delete();
                   // foreach (ColShape obj in ColShapes) obj.Delete();
                    Blip.Delete();
                }
                catch { }
            });
        }

        public void Interact(Player player, int interact)
        {
            try
            {
                switch (interact)
                {
                    case 525: // on open apartment list
                        List<object> HousesList = new List<object>();
                        foreach (int id in Houses)
                        {
                            House housefind = HouseManager.Houses.FirstOrDefault(h => h.ID == id);
                            if (housefind == null) continue;

                            List<object> Housef = new List<object>
                            {
                                id, housefind.Owner, housefind.Price + "$", GarageManager.GarageTypes[GarageManager.Garages[housefind.GarageID].Type].MaxCars, housefind.Roommates.Count + " / " + HouseManager.MaxRoommates[housefind.Type]
                            };
                            HousesList.Add(Housef);
                        }

                        NAPI.ClientEvent.TriggerClientEvent(player, "client::openapart", JsonConvert.SerializeObject(HousesList), JsonConvert.SerializeObject(player.GetData<ApartmentParent>("APARTMENT").Name), JsonConvert.SerializeObject(player.GetData<ApartmentParent>("APARTMENT").ID));
                        return;
                    default:
                        return;
                }
            }
            catch { }
        }

    }


}

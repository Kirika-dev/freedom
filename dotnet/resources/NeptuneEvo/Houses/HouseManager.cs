using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;
using System.Linq;
using System.Data;
using NeptuneEVO.GUI;
using NeptuneEVO.Core.nAccount;
using NeptuneEVO.Businesses;

namespace NeptuneEVO.Houses
{
    #region HouseType Class
    public class HouseType
    {
        public string Name { get; }
        public Vector3 Position { get; }
        public string IPL { get; set; }
        public Vector3 PetPosition { get; }
        public float PetRotation { get; }

        public HouseType(string name, Vector3 position, Vector3 petpos, float rotation, string ipl = "")
        {
            Name = name;
            Position = position;
            IPL = ipl;
            PetPosition = petpos;
            PetRotation = rotation;
        }

        public void Create()
        {
            if (IPL != "") NAPI.World.RequestIpl(IPL);
        }
    }
    #endregion

    #region House Class
    class House
    {
        public int ID { get; }
        public string Owner { get; private set; }
        public int Type { get; private set; }
        public Vector3 Position { get; }
        public int Price { get; set; }
        public bool Locked { get; private set; }
        public int GarageID { get; set; }
        public int BankID { get; set; }
        public List<string> Roommates { get; set; } = new List<string>();
        public int Apart { get; set; }
        public List<PostalItem> Postal { get; set; } = new List<PostalItem>();
        [JsonIgnore] public int Dimension { get; set; }

        [JsonIgnore]
        public Blip blip;
        [JsonIgnore]
        public string PetName;
        [JsonIgnore]
        private TextLabel label;
        [JsonIgnore]
        private ColShape shape;

        [JsonIgnore]
        private ColShape intshape;
        [JsonIgnore]
        private Marker intmarker;
        [JsonIgnore]
        public Marker marker;

        [JsonIgnore]
        private List<GTANetworkAPI.Object> Objects = new List<GTANetworkAPI.Object>();

        [JsonIgnore]
        private List<Entity> PlayersInside = new List<Entity>();

        public House(int id, string owner, int type, Vector3 position, int price, bool locked, int garageID, int bank, List<string> roommates, int apart, List<PostalItem> postal)
        {
            ID = id;
            Owner = owner;
            Type = type;
            Position = position;
            Price = price;
            Locked = locked;
            GarageID = garageID;
            BankID = bank;
            Roommates = roommates;
            Apart = apart;
            Postal = postal;

            if (Apart != -1)
            {

                Position = Apartments.ApartmentList[Apart].Pos;
                GarageManager.Garages[GarageID].SetPos(Apartments.ApartmentList[Apart].GaragePos, new Vector3(0, 0, Apartments.ApartmentList[Apart].Heading));
                return;
            }

            #region Creating Blip
                marker = NAPI.Marker.CreateMarker(1, Position - new Vector3(0,0,0.25), new Vector3(), new Vector3(), 0.7f, new Color(0, 0, 0, 255), false, 0);
                blip = NAPI.Blip.CreateBlip(Position);
                blip.Scale = 0.5f;
                blip.ShortRange = true;
                blip.Dimension = 0;
                UpdateBlip();
                #endregion

            #region Creating Marker & Colshape
            shape = NAPI.ColShape.CreateCylinderColShape(position, 1, 2, 0);
                shape.OnEntityEnterColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "HOUSEID", id);
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 6);
                        Trigger.PlayerEvent(ent, "client::showhintHUD", true, $"Дом #{ID}");
                        Jobs.Gopostal.GoPostal_onEntityEnterColShape(s, ent);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityEnterColShape: " + ex.Message); }
                };
                shape.OnEntityExitColShape += (s, ent) =>
                {
                    try
                    {
                        NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                        NAPI.Data.ResetEntityData(ent, "HOUSEID");
                        Trigger.PlayerEvent(ent, "client::showhintHUD", false, null);
                    }
                    catch (Exception ex) { Console.WriteLine("shape.OnEntityExitColShape: " + ex.Message); }
                };
                #endregion

                label = NAPI.TextLabel.CreateTextLabel(Main.StringToU16($"House {id}"), position + new Vector3(0, 0, 1.5), 5f, 0.4f, 4, new Color(255, 255, 255), false, 0);
                UpdateLabel();
        }
        public void UpdateLabel()
        {
            try
            {
                if (Apart == -1)
                label.Text = $"Дом: #{ID}";

            }
            catch (Exception e)
            {
                blip.Color = 48;
                Console.WriteLine(ID.ToString() + e.ToString());
            }
        }
        public void CreateAllFurnitures()
        {
            if (FurnitureManager.HouseFurnitures.ContainsKey(ID))
            {
                if (FurnitureManager.HouseFurnitures[ID].Count >= 1)
                {
                    foreach (var f in FurnitureManager.HouseFurnitures[ID].Values) if (f.IsSet) CreateFurniture(f);
                }
            }
        }
        public void CreateFurniture(HouseFurniture f)
        {
            try
            {
                var obj = f.Create((uint)Dimension);
                NAPI.Data.SetEntityData(obj, "HOUSE", ID);
                NAPI.Data.SetEntityData(obj, "ID", f.ID);
                NAPI.Entity.SetEntityDimension(obj, (uint)Dimension);
                if (f.Name == "Оружейный сейф") NAPI.Data.SetEntitySharedData(obj, "TYPE", "WeaponSafe");
                else if (f.Name == "Шкаф с одеждой") NAPI.Data.SetEntitySharedData(obj, "TYPE", "ClothesSafe");
                else if (f.Name == "Шкаф с предметами") NAPI.Data.SetEntitySharedData(obj, "TYPE", "SubjectSafe");
                Objects.Add(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR FURNITURE: " + e.ToString());
            }
        }
        public void DestroyFurnitures()
        {
            try
            {
                foreach (var obj in Objects) NAPI.Entity.DeleteEntity(obj);
                Objects = new List<GTANetworkAPI.Object>();
            }
            catch { }
        }
        public void DestroyFurniture(int id)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    foreach (var obj in Objects)
                    {
                        if (obj.HasData("ID") && obj.GetData<int>("ID") == id)
                        {
                            NAPI.Entity.DeleteEntity(obj);
                            //Log.Debug("HOUSEFURNITURE: deleted " + id);
                            break;
                        }
                    }
                }
                catch { }
            });
        }
        public void UpdateBlip()
        {
            if (string.IsNullOrEmpty(Owner))
            {
                blip.Sprite = 40;
                blip.Color = 11;
                blip.Name = "Дом";
                blip.Transparency = 255;
                marker.Color = new Color(92, 228, 125, 150);
            }
            else
            {
                blip.Sprite = 40;
                blip.Color = 49;
                blip.Name = "Дом";
                blip.Transparency = 0;
                marker.Color = new Color(255, 77, 77, 150);
            }
        }

        public void Create()
        {
            MySQL.Query($"INSERT INTO `houses`(`id`,`owner`,`type`,`position`,`price`,`locked`,`garage`,`bank`,`roommates`,`postal`) " +
                $"VALUES ('{ID}','{Owner}',{Type},'{JsonConvert.SerializeObject(Position)}',{Price},{Locked},{GarageID},{BankID},'{JsonConvert.SerializeObject(Roommates)}','{JsonConvert.SerializeObject(Postal)}')");
        }
        public void Save()
        {
            //MoneySystem.Bank.Save(BankID);
            MySQL.Query($"UPDATE `houses` SET `owner`='{Owner}'," +
                $"`locked`={Locked},`garage`={GarageID},`roommates`='{JsonConvert.SerializeObject(Roommates)}',`postal`='{JsonConvert.SerializeObject(Postal)}' WHERE `id`='{ID}'");
        }
        public void TimerPostal()
        {
            if (Postal.Count != 0)
            {
                foreach (PostalItem item in Postal)
                {
                    if (item.Time != 0)
                    {
                        item.Time--;
                        if (item.Time == 0)
                        {
                            foreach (Player p in API.Shared.GetAllPlayers())
                            {
                                if (Main.Players.ContainsKey(p))
                                if (Owner == p.Name)
                                {
                                    Trigger.PlayerEvent(p, "client::house::postal:notify");
                                }
                            }
                        }
                        Save();
                    }
                }
            }
        }

        public int getCountPostal()
        {
            int i = 0;
            foreach (PostalItem item in Postal)
            {
                if (item.Time == 0)
                {
                    i++;
                }
            }
            return i;
        }
        public void Destroy()
        {
            RemoveAllPlayers();
            blip.Delete();
            NAPI.ColShape.DeleteColShape(shape);
            NAPI.ColShape.DeleteColShape(intshape);
            label.Delete();
            intmarker.Delete();
            marker.Delete();
            DestroyFurnitures();
        }
        public void SetLock(bool locked)
        {
            Locked = locked;

            UpdateLabel();
            Save();
        }
        public void SetOwner(Player player)
        {
            GarageManager.Garages[GarageID].DestroyCars();
            Owner = (player == null) ? string.Empty : player.Name;
            NAPI.Task.Run(() => { try { 
            if (blip != null)
                blip.Delete();
                }
                catch { }
            });
            UpdateLabel();
            if (player != null)
            {
                Trigger.PlayerEvent(player, "changeBlipColor", blip, 73);
                if (Apart == -1)
                    Trigger.PlayerEvent(player, "createHouseBlip", Position);
                else
                    Trigger.PlayerEvent(player, "client::blip:shortrange", Apartments.ApartmentList[Apart].Blip, false);

                /*var vehicles = VehicleManager.getAllPlayerVehicles(Owner);
                if (GarageManager.Garages[GarageID].Type != -1)
                    NAPI.Task.Run(() => { try { GarageManager.Garages[GarageID].SpawnCars(vehicles); } catch { } });*/
            }

            foreach (var r in Roommates)
            {
                var roommate = NAPI.Player.GetPlayerFromName(r);
                if (roommate != null)
                {
                    Notify.Send(roommate, NotifyType.Warning, NotifyPosition.BottomCenter, "Вы были выселены из дома", 3000);
					roommate.TriggerEvent("deleteHouseBlip");
                    roommate.TriggerEvent("deleteCheckpoint", 333);
                    roommate.TriggerEvent("deleteGarageBlip");
                    if (Apart != -1)
                    {
                        Trigger.PlayerEvent(player, "client::blip:shortrange", Apartments.ApartmentList[Apart].Blip, true);
                    }
                }
            }
            if (Apart == -1)
                marker.Color = new Color(255, 77, 77, 150);
            Roommates = new List<string>();
            Save();
        }
        public string GaragePlayerExit(Player player)
        {
            var players = NAPI.Pools.GetAllPlayers();
            var online = players.FindAll(p => Roommates.Contains(p.Name) && p.Name != player.Name);

            var owner = NAPI.Player.GetPlayerFromName(Owner);
            if (Roommates.Contains(player.Name) && owner != null && Main.Players.ContainsKey(owner))
                online.Add(owner);

            var garage = GarageManager.Garages[GarageID];
            var number = garage.SendVehiclesInsteadNearest(online, player);

            return number;
        }
        public void SendPlayer(Player player)
        {
            Trigger.PlayerEvent(player, "client::screen:transition", 400, 1500, 400, null);
            NAPI.Task.Run(() => {
                if (player != null)
                {
                    NAPI.Entity.SetEntityPosition(player, HouseManager.HouseTypeList[Type].Position + new Vector3(0, 0, 1.12));
                    NAPI.Entity.SetEntityDimension(player, Convert.ToUInt32(Dimension));
                    Main.Players[player].InsideHouseID = ID;
                    if (HouseManager.HouseTypeList[Type].PetPosition != null)
                    {
                        if (PetName != null && PetName != "null") Trigger.PlayerEvent(player, "petinhouse", PetName, HouseManager.HouseTypeList[Type].PetPosition.X, HouseManager.HouseTypeList[Type].PetPosition.Y, HouseManager.HouseTypeList[Type].PetPosition.Z, HouseManager.HouseTypeList[Type].PetRotation, Dimension);
                    }
                    DestroyFurnitures();
                    CreateAllFurnitures();
                    if (!PlayersInside.Contains(player)) PlayersInside.Add(player);
                }
            }, 600);
        }
        public void RemovePlayer(Player player, bool exit = true)
        {
            Trigger.PlayerEvent(player, "client::screen:transition", 400, 1500, 400, null);
            NAPI.Task.Run(() =>
            {
                if (player != null)
                {
                    if (exit)
                    {
                        NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0, 0, 1.12));
                        NAPI.Entity.SetEntityDimension(player, 0);
                    }
                    player.ResetData("InvitedHouse_ID");
                    Main.Players[player].InsideHouseID = -1;

                    if (PlayersInside.Contains(player)) PlayersInside.Remove(player);
                }
            }, 600);
        }
        public void RemoveFromList(Player player)
        {
            if (PlayersInside.Contains(player)) PlayersInside.Remove(player);
        }
        public void RemoveAllPlayers(Player requster = null)
        {
            for (int i = PlayersInside.Count - 1; i >= 0; i--)
            {
                Player player = NAPI.Entity.GetEntityFromHandle<Player>(PlayersInside[i]);
                if (requster != null && player == requster) continue;

                if (player != null)
                {
                    NAPI.Entity.SetEntityPosition(player, Position + new Vector3(0, 0, 1.12));
                    NAPI.Entity.SetEntityDimension(player, 0);

                    player.ResetData("InvitedHouse_ID");
                    Main.Players[player].InsideHouseID = -1;
                }

                PlayersInside.RemoveAt(i);
            }
        }
        public void CreateInterior()
        {
            #region Creating Interior ColShape & Marker
            intmarker = NAPI.Marker.CreateMarker(1, HouseManager.HouseTypeList[Type].Position, new Vector3(), new Vector3(), 0.7f, new Color(67, 140, 239, 200), false, (uint)Dimension);

            intshape = NAPI.ColShape.CreateCylinderColShape(HouseManager.HouseTypeList[Type].Position - new Vector3(0.0, 0.0, 1.0), 2f, 4f, (uint)Dimension);
            intshape.OnEntityEnterColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 7);
                }
                catch (Exception ex) { Console.WriteLine("intshape.OnEntityEnterColShape: " + ex.Message); }
            };

            intshape.OnEntityExitColShape += (s, ent) =>
            {
                try
                {
                    NAPI.Data.SetEntityData(ent, "INTERACTIONCHECK", 0);
                }
                catch (Exception ex) { Console.WriteLine("intshape.OnEntityExitColShape: " + ex.Message); }
            };
            #endregion
        }

        public void changeOwner(string newName)
        {
            Owner = newName;
            this.UpdateLabel();
            this.Save();
        }
    }
    #endregion

    class HouseManager : Script
    {
        public static nLog Log = new nLog("HouseManager");

        public static List<House> Houses = new List<House>();
        public static List<HouseType> HouseTypeList = new List<HouseType>
        {
            // name, position
            new HouseType("Трейлер", new Vector3(1973.124, 3816.065, 32.30873), new Vector3(), 0.0f, "trevorstrailer"),
            new HouseType("Эконом", new Vector3(151.2052, -1008.007, -100.12), new Vector3(), 0.0f, "hei_hw1_blimp_interior_v_motel_mp_milo_"),
            new HouseType("Эконом+", new Vector3(265.9691, -1007.078, -102.0758), new Vector3(), 0.0f, "hei_hw1_blimp_interior_v_studio_lo_milo_"),
            new HouseType("Комфорт", new Vector3(346.6991, -1013.023, -100.3162), new Vector3(349.5223, -994.5601, -99.7562), 264.0f, "hei_hw1_blimp_interior_v_apart_midspaz_milo_"),
            new HouseType("Комфорт+", new Vector3(-31.35483, -594.9686, 78.9109),  new Vector3(-25.42115, -581.4933, 79.12776), 159.84f, "hei_hw1_blimp_interior_32_dlc_apart_high2_new_milo_"),
            new HouseType("Премиум", new Vector3(-18.41382, -591.3663, 88.99482), new Vector3(-38.84652, -578.466, 88.58952), 50.8f, "hei_hw1_blimp_interior_10_dlc_apart_high_new_milo_"),
            new HouseType("Премиум+", new Vector3(-174.1553, 497.3088, 136.5341), new Vector3(-164.9799, 480.7568, 137.1526), 40.0f, "apa_ch2_05e_interior_0_v_mp_stilts_b_milo_"),
            new HouseType("Элитный", new Vector3(-174.1553, 497.3088, 136.5341), new Vector3(-164.9799, 480.7568, 137.1526), 40.0f, "apa_ch2_05e_interior_0_v_mp_stilts_b_milo_"),
        };
        public static List<int> MaxRoommates = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

        private static int GetUID()
        {
            int newUID = 1;
            while (Houses.FirstOrDefault(h => h.ID == newUID) != null) newUID++;
            return newUID;
        }

        public static int DimensionID = 10000;

        #region Events

        public static void onResourceStart()
        {
            try
            {
                foreach (HouseType house_type in HouseTypeList) house_type.Create();

                var result = MySQL.QueryRead($"SELECT * FROM `houses`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result.", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    try
                    {
                        var id = Convert.ToInt32(Row["id"].ToString());
                        var owner = Convert.ToString(Row["owner"]);
                        var type = Convert.ToInt32(Row["type"]);
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var price = Convert.ToInt32(Row["price"]);
                        var locked = Convert.ToBoolean(Row["locked"]);
                        var garage = Convert.ToInt32(Row["garage"]);
                        var bank = Convert.ToInt32(Row["bank"]);
                        var roommates = JsonConvert.DeserializeObject<List<string>>(Row["roommates"].ToString());
                        var apart = Convert.ToInt32(Row["apart"]);
                        var postal = JsonConvert.DeserializeObject<List<PostalItem>>(Row["postal"].ToString());

                        House house = new House(id, owner, type, position, price, locked, garage, bank, roommates, apart, postal);
                        house.Dimension = DimensionID;
                        house.CreateInterior();
                        FurnitureManager.Create(id);
                        house.CreateAllFurnitures();

                        Houses.Add(house);
                        DimensionID++;

                    }
                    catch (Exception e)
                    {
                        Log.Write(Row["id"].ToString() + e.ToString(), nLog.Type.Error);
                    }

                }

                NAPI.Object.CreateObject(0x07e08443, new Vector3(-825.2067, -524.3351, -98.62196), new Vector3(0, 0, -109.999962), 255, NAPI.GlobalDimension);
                for (int i = 0; i < Jobs.DeliveryClub.MaxOrders; i++)
                {
                    Jobs.DeliveryClub.AddOrder();
                }
                Log.Write($"Loaded {Houses.Count} houses.", nLog.Type.Success);
            }
            catch (Exception e) { Log.Write("ResourceStart: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_OnPlayerDeath(Player player, Player entityKiller, uint weapon)
        {
            try
            {
                NAPI.Entity.SetEntityDimension(player, 0);
                RemovePlayerFromHouseList(player);
            }
            catch (Exception e) { Log.Write("PlayerDeath: " + e.Message, nLog.Type.Error); }
        }

        public static void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            try
            {
                RemovePlayerFromHouseList(player);
            }
            catch (Exception e) { Log.Write("PlayerDisconnected: " + e.Message, nLog.Type.Error); }
        }

        public static void SavingHouses()
        {
            foreach (var h in Houses) h.Save();
            Log.Write("Saved!", nLog.Type.Save);
        }

        [ServerEvent(Event.ResourceStop)]
        public void Event_OnResourceStop()
        {
            try
            {
                SavingHouses();
            }
            catch (Exception e) { Log.Write("ResourceStop: " + e.Message, nLog.Type.Error); }
        }
        #endregion

        #region Methods
        public static House GetHouse(Player player, bool checkOwner = false)
        {
            House house = null;
            foreach (House hou in Houses)
                if (hou.Apart == -1 && hou.Owner == player.Name)
                {
                    house = hou;
                    break;
                }
            if (house != null && house.Apart == -1)
                return house;
            else if (!checkOwner)
            {
                house = null;
                foreach (House hou in Houses)
                    if (hou.Apart == -1 && hou.Roommates.Contains(player.Name))
                    {
                        house = hou;
                        break;
                    }
                return house;
            }
            else
                return null;
        }

        public static House GetApart(Player player, bool checkOwner = false)
        {
            House house = null;
            foreach (House hou in Houses)
                if (hou.Apart != -1 && hou.Owner == player.Name)
                {
                    house = hou;
                    break;
                }
            if (house != null && house.Apart != -1)
                return house;
            else if (!checkOwner )
            {
                house = null;
                foreach (House hou in Houses)
                    if (hou.Apart != -1 && hou.Roommates.Contains(player.Name))
                    {
                        house = hou;
                        break;
                    }
                return house;
            }
            else
                return null;
        }

        public static House GetHouse(string name, bool checkOwner = false)
        {
            House house = Houses.FirstOrDefault(h => h.Owner == name);
            if (house != null && house.Apart == -1)
                return house;
            else if (!checkOwner && house.Apart == -1)
            {
                house = Houses.FirstOrDefault(h => h.Roommates.Contains(name));
                return house;
            }
            else
                return null;
        }

        public static void RemovePlayerFromHouseList(Player player)
        {
            if (Main.Players[player].InsideHouseID != -1)
            {
                House house = Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                if (house == null) return;
                house.RemoveFromList(player);
            }
        }

        public static void CheckAndKick(Player player)
        {
            var house = GetHouse(player);
            if (house == null) return;
            if (house.Roommates.Contains(player.Name)) house.Roommates.Remove(player.Name);
        }

        public static void ChangeOwner(string oldName, string newName)
        {
            lock (Houses)
            {
                foreach (House h in Houses)
                {
                    if (h.Owner != oldName) continue;
                    Log.Write($"The house was found! [{h.ID}]");
                    h.changeOwner(newName);
                    h.Save();
                }
            }
        }
        #endregion

        public static void interactPressed(Player player, int id)
        {
            switch (id)
            {
                case 6:
                    {
                        if (player.IsInVehicle) return;
                        if (!player.HasData("HOUSEID")) return;

                        House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
                        if (house == null) return;
                        if (player.HasData("DELIVERYCLUB_ORDER") && player.GetData<Jobs.DeliveryClub.Order>("DELIVERYCLUB_ORDER") != null)
                        {
                            if ((player.HasSharedData("DELIVERY_CLUB_FOODINHANDS") && player.GetSharedData<bool>("DELIVERY_CLUB_FOODINHANDS") != true) || !player.HasSharedData("DELIVERY_CLUB_FOODINHANDS"))
                            {
                                Notify.Error(player, "Вы не взяли еду со скутера");
                                return;
                            }
                            MoneySystem.Wallet.Change(player, player.GetData<Jobs.DeliveryClub.Order>("DELIVERYCLUB_ORDER").Price);
                            Jobs.DeliveryClub.Orders.Remove(player.GetData<Jobs.DeliveryClub.Order>("DELIVERYCLUB_ORDER"));
                            Notify.Succ(player, $"Вы сдали заказ Delivery Club #{player.GetData<Jobs.DeliveryClub.Order>("DELIVERYCLUB_ORDER").ID}");
                            player.SetData<Jobs.DeliveryClub.Order>("DELIVERYCLUB_ORDER", null);
                            Trigger.PlayerEvent(player, "deleteWorkBlip");
                            player.SetSharedData("DELIVERYCLUB_ORDER_TAKEN", false);
                            player.SetSharedData("DELIVERY_CLUB_FOODINHANDS", false);
                            BasicSync.DetachObject(player);
                            Main.StopSyncAnimation(player);

                            BattlePass.AddProgressToQuest(player, 13, 1);
                            
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                            return;
                        }
                        OpenHouseManager(player, house);
                        return;
                    }
                case 7:
                    {
                        if (Main.Players[player].InsideHouseID == -1) return;

                        House house = Houses.FirstOrDefault(h => h.ID == Main.Players[player].InsideHouseID);
                        if (house == null) return;

                        if (player.HasData("IS_EDITING"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны закончить редактирование", 3000);
                            MenuManager.Close(player);
                            return;
                        }
                        house.RemovePlayer(player);
                        return;
                    }
            }
        }
                   
        [RemoteEvent("server::house::postal:take")]
        public static void TakePostalItem(Player player, int id)
        {
            House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;
            if (house.Postal[id] == null)
            {
                Notify.Error(player, "Предмет не найден");
                return;
            }
            PostalItem item = house.Postal[id];
            int tryAdd = nInventory.TryAdd(player, item.Item);
            if (tryAdd == -1 || tryAdd > 0)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                return;
            }
            nInventory.Add(player, item.Item);
            house.Postal.Remove(item);
            Notify.Succ(player, "Вы забрали с посылку");
            Trigger.PlayerEvent(player, "client::house::postal:update", JsonConvert.SerializeObject(house.Postal));
        }

        public static void OpenHouseManager(Player player, House house)
        {
            bool owner = house.Owner == player.Name ? true : false;
            var houseowner = string.IsNullOrEmpty(house.Owner) ? "Государство" : house.Owner;
            Trigger.PlayerEvent(player, "client::house:open", house.ID, JsonConvert.SerializeObject(houseowner), house.Price, GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars, MaxRoommates[house.Type], house.Type, house.Locked, owner);
        }

        [RemoteEvent("server::house:interaction")]
        public static void PlayerEvent_CallBack(Player player, int id)
        {
            try
            {
                House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
                if (house == null) return;
                switch (id)
                {
                    case 0:
                        if (!string.IsNullOrEmpty(house.Owner))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В этом доме уже имеется хозяин", 3000);
                            return;
                        }

                        house.SendPlayer(player);
                        break;
                    case 1:
                        if (!string.IsNullOrEmpty(house.Owner))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В этом доме уже имеется хозяин", 3000);
                            return;
                        }
                        if (nInventory.Find(Main.Players[player].UUID, ItemType.IDCard) == null)
                        {
                            Notify.Error(player, "У вас нет ID-Карты. Получите ее в мэрии");
                            return;
                        }
                        if (house.Price > Main.Players[player].Money)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не хватает средств для покупки дома", 3000);
                            return;
                        }

                        if (GetHouse(player) != null)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не можете купить больше одного дома", 3000);
                            return;
                        }
                        var vehicles = VehicleManager.getAllPlayerVehicles(player.Name).Count;
                        var maxcars = GarageManager.GarageTypes[GarageManager.Garages[house.GarageID].Type].MaxCars;
                        if (vehicles > maxcars)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Дом, который Вы покупаете, имеет {maxcars} гаражных места, продайте лишние машины", 3000);
                            OpenCarsSellMenu(player);
                            return;
                        }
                        if (HouseTypeList[house.Type].PetPosition != null) house.PetName = Main.Players[player].PetName;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили этот дом, не забудьте внести налог за него в банкомате", 3000);
                        Notify.Send(player, NotifyType.Success, NotifyPosition.Center, $"Не забудьте внести налог за него в банкомате.", 8000);
                        CheckAndKick(player);
                        house.SetLock(true);
                        house.SetOwner(player);
                        house.SendPlayer(player);
                        MoneySystem.Bank.Accounts[house.BankID].Balance = Convert.ToInt32(house.Price / 100 * 0.005) * 2;
                        MoneySystem.Bank.Save(house.BankID);


                        MoneySystem.Wallet.Change(player, -house.Price);

                        var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());
                        var vehicle = "";
                        foreach (var num in targetVehicles)
                        {
                            vehicle = num;
                            break;
                        }


                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "PERSONAL" && NAPI.Vehicle.GetVehicleNumberPlate(v) == vehicle)
                            {
                                var veh = v;

                                foreach (var ve in NAPI.Pools.GetAllVehicles())
                                    if (ve.Model == NAPI.Util.GetHashKey("flatbed"))
                                        if (ve.HasSharedData("fbAttachVehicle") && ve.GetSharedData<int>("fbAttachVehicle") == v.Id)
                                            return;

                                if (veh == null) return;
                                VehicleManager.Vehicles[vehicle].Fuel = (!veh.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[veh.Class] : veh.GetSharedData<int>("PETROL");
                                NAPI.Entity.DeleteEntity(veh);

                                MoneySystem.Wallet.Change(player, -200);
                                GameLog.Money($"player({Main.Players[player].UUID})", $"server", 200, $"carEvac");
                                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была отогнана в гараж", 3000);
                                break;
                            }
                        }

                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", house.Price, $"houseBuy({house.ID})");
                        break;
                    case 2:
                        if (house.Locked)
                        {
                            var playerHouse = GetHouse(player);
                            if (playerHouse != null && playerHouse.ID == house.ID)
                                house.SendPlayer(player);
                            else if (player.HasData("InvitedHouse_ID") && player.GetData<int>("InvitedHouse_ID") == house.ID)
                                house.SendPlayer(player);
                            else
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет доступа", 3000);
                        }
                        else
                            house.SendPlayer(player);
                        break;
                    case 3:
                        if (house.getCountPostal() == 0)
                        {
                            Notify.Error(player, "В почтовом ящике нет посылок");
                            return;
                        }
                        Trigger.PlayerEvent(player, "client::house::postal:open", JsonConvert.SerializeObject(house.Postal));
                        break;
                }
            }
            catch (Exception e) { Log.Write("Housecallback: " + e.ToString(), nLog.Type.Error); }
        }

        [RemoteEvent("server::phone:switchhouse")]
        public static void ClientEvent_SwitchHouseTab(Player player, int id)
        {
            House house = GetHouse(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            switch (id)
            {
                case 0:
                    house.RemoveAllPlayers(player);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выгнали всех из дома", 3000);
                    return;
                case 1:
                    house.SetLock(!house.Locked);
                    if (house.Locked) Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли дом", 3000);
                    else Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли дом", 3000);
                    return;
                case 2:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", house.Position.X, house.Position.Y, house.Position.Z);
                    return;
                case 3:
                    acceptHouseSellToGov(player, false);
                    return;
            }
        }  
        [RemoteEvent("server::phone:switchapart")]
        public static void ClientEvent_SwitchApartamentTab(Player player, int id)
        {
            House house = GetApart(player, true);
            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                MenuManager.Close(player);
                return;
            }
            switch (id)
            {
                case 0:
                    house.RemoveAllPlayers(player);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы выгнали всех из дома", 3000);
                    return;
                case 1:
                    house.SetLock(!house.Locked);
                    if (house.Locked) Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы закрыли дом", 3000);
                    else Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы открыли дом", 3000);
                    return;
                case 2:
                    Trigger.PlayerEvent(player, "client::navigator:setPoint", house.Position.X, house.Position.Y, house.Position.Z);
                    return;
                case 3:
                    acceptHouseSellToGov(player, true);
                    return;
            }
        }
        public static House GetNearestHouse(Player player)
        {
            House house = null;
            House apart = HouseManager.GetApart(player, true);
            House hous = HouseManager.GetHouse(player, true);
            if (apart != null && house != null)
            {
                if (player.Position.DistanceTo(apart.Position) < player.Position.DistanceTo(hous.Position))
                {
                    house = apart;
                }
                else
                {
                    house = hous;
                }
            }
            else
            {
                if (apart != null)
                {
                    house = apart;
                }
                if (hous != null)
                {
                    house = hous;
                }
            }
            return house;
        }
        public static int GetMaxCarsInHouse(Player player)
        {
            int max = 1;
            House apart = HouseManager.GetApart(player, true);
            House house = HouseManager.GetHouse(player, true);
            if (apart != null)
            {
                var garage = GarageManager.Garages[apart.GarageID];
                if (garage != null)
                    max += GarageManager.GarageTypes[garage.Type].MaxCars;
            }
            if (house != null)
            {
                var garage = GarageManager.Garages[house.GarageID];
                if (garage != null)
                    max += GarageManager.GarageTypes[garage.Type].MaxCars;
            }
            return max;
        }
        [RemoteEvent("server::phone:switchvehicle")]
        public static void ClientEvent_SwitchVehicleTab(Player player, int id, string number)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (VehicleManager.Vehicles[number] == null)
            {
                Notify.Error(player, "Машины не существует");
                Commands.SendToAdmins(1, $"Игрок {player.Name} #{Main.Players[player].UUID} ({player.Value}) пытается взаимодействовать в телефоне с несуществующей машиной");
                return;
            }
            House house = GetNearestHouse(player);
            switch (id)
            {
                case 0:
                    var vData = VehicleManager.Vehicles[number];
                    if (vData.Health > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Машина не нуждается в восстановлении", 3000);
                        return;
                    }

                    var vClass = NAPI.Vehicle.GetVehicleClass(NAPI.Util.VehicleNameToModel(vData.Model));
                    if (!MoneySystem.Wallet.Change(player, -VehicleManager.VehicleRepairPrice[vClass]))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно средств", 3000);
                        return;
                    }
                    vData.Items = new List<nItem>();
                    GameLog.Money($"player({Main.Players[player].UUID})", $"server", VehicleManager.VehicleRepairPrice[vClass], $"carRepair({vData.Model})");
                    vData.Health = 1000;
                    var text = number.Contains("TRANSIT") ? "" : "(" + number + ")";
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы восстановили {Utilis.VehiclesName.GetRealVehicleName(vData.Model)} {text}", 3000);
                    return;
                case 1:
                    var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.CarKey));
                    if (tryAdd == -1 || tryAdd > 0)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                        return;
                    }

                    nInventory.Add(player, new nItem(ItemType.CarKey, 1, $"{number}_{VehicleManager.Vehicles[number].KeyNum}"));
                    text = number.Contains("TRANSIT") ? Utilis.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[number].Model) + " с транзитными номерами" : Utilis.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[number].Model) + " c номером " + number;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы получили ключ от машины {text}", 3000);
                    return;
                case 2:
                    if (!MoneySystem.Wallet.Change(player, -100))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Смена замков стоит $100", 3000);
                        return;
                    }

                    VehicleManager.Vehicles[number].KeyNum++;
                    text = number.Contains("TRANSIT") ? Utilis.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[number].Model) + " с транзитными номерами" : Utilis.VehiclesName.GetRealVehicleName(VehicleManager.Vehicles[number].Model) + " c номером " + number;
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы сменили замки на машине {text}", 3000);
                    return;
                case 3:
                    if (house == null)
                    {
                        if (Main.Players[player].Money < 100)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                            return;
                        }
                        var targetVehicles = VehicleManager.getAllPlayerVehicles(player.Name.ToString());
                        var vehicle = "";
                        foreach (var num in targetVehicles)
                        {
                            vehicle = num;
                            break;
                        }
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v != null && v.HasData("ACCESS") && (v.GetData<string>("ACCESS") == "PERSONAL" || v.GetData<string>("ACCESS") == "INPARK") && v.NumberPlate == vehicle)
                            {
                                NAPI.Task.Run(() =>
                                {
                                    try
                                    {
                                        var veh = v;
                                        if (veh == null) return;
                                        VehicleManager.Vehicles[number].Fuel = (!veh.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[veh.Class] : veh.GetSharedData<int>("PETROL");
                                        veh.Delete();

                                        MoneySystem.Wallet.Change(player, -100);
                                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 15, $"carEvac");
                                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была отогнана на стоянку", 3000);
                                    }
                                    catch { }
                                });
                                break;
                            }
                        }
                    }
                    else
                    {
                        var garage = GarageManager.Garages[house.GarageID];
                        var check = garage.CheckCar(false, number);

                        if (!check)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта машина стоит в гараже", 3000);
                            return;
                        }
                        if (Main.Players[player].Money < 100)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                            return;
                        }

                        var veh = garage.GetOutsideCar(number);

                        if (veh == null) return;

                        VehicleManager.Vehicles[number].Fuel = (!veh.HasSharedData("PETROL")) ? VehicleManager.VehicleTank[veh.Class] : veh.GetSharedData<int>("PETROL");
                        if (garage.vehiclesOut.ContainsKey(number))
                            garage.vehiclesOut.Remove(number);
                        NAPI.Entity.DeleteEntity(veh);

                        MoneySystem.Wallet.Change(player, -15);
                        GameLog.Money($"player({Main.Players[player].UUID})", $"server", 15, $"carEvac");
                        Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Ваша машина была отогнана в гараж", 3000);
                    }
                    return;
                case 4:
                    vData = VehicleManager.Vehicles[number];
                    if (vData.Holder != player.Name)
                    {
                        Commands.SendToAdmins(1, $"Игрок {player.Name} #{Main.Players[player].UUID} ({player.Value}) пытается продать чужую машину");
                        return;
                    }

                    int price = BCore.GetVipCost(player, BCore.CostForCar(vData.Model));

                    foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                        if (veh != null && veh.NumberPlate == number)
                        {
                            break;
                        }

                    text = number.Contains("TRANSIT") ? "" : "(" + number + ")";
                    if (AutoShopI.ProductsList[4].ContainsKey(vData.Model))
                    {
                        Main.Accounts[player].RedBucks += price * 6;
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {Utilis.VehiclesName.GetRealVehicleName(vData.Model)} {text} за {String.Format("{0:n0}", price * 6)} MC", 3000);
                    }
                    else
                    {
                        Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали {Utilis.VehiclesName.GetRealVehicleName(vData.Model)} {text} за {String.Format("{0:n0}", price * 6)}$", 3000);
                        MoneySystem.Wallet.Change(player, price * 6);
                    }

                    foreach (Vehicle veh in NAPI.Pools.GetAllVehicles())
                    {
                        if (veh.NumberPlate == number)
                        {
                            NAPI.Task.Run(() => { veh.Delete(); });
                            break;
                        }
                    }

                    GameLog.Money($"server", $"player({Main.Players[player].UUID})", price * 6, $"carSell({vData.Model})");
                    VehicleManager.Remove(number, player);
                    Trigger.PlayerEvent(player, "client::phone:updateVeh", JsonConvert.SerializeObject(NeptuneEvo.Core.Phone.Manager.GetVehicleInfo(player)));
                    return;
                case 5:
                    var result = MySQL.QueryRead($"SELECT position, rotation FROM vehicles WHERE number='{number}'");
                    foreach (DataRow Row in result.Rows)
                    {
                        var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                        var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                        if (position == null)
                        {
                            position = new Vector3(-50.669792, -1116.777, 27.506832);
                        }
                        if (rotation == null)
                        {
                            rotation = new Vector3(0.08978348, 0.012273829, 3.0145645);
                        }
                        foreach (var v in NAPI.Pools.GetAllVehicles())
                        {
                            if (v.HasData("ACCESS") && v.GetData<string>("ACCESS") == "PERSONAL" && NAPI.Vehicle.GetVehicleNumberPlate(v) == number)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, "Машина уже взвана, метка установлена в GPS", 3000);
                                Trigger.PlayerEvent(player, "createWaypoint", position.X, position.Y);
                                return;
                            }
                        }
                        if (house != null)
                        {
                            var garage = GarageManager.Garages[house.GarageID];
                            var check = garage.CheckCar(false, number);
                            if (!check)
                            {
                                if (player.Position.DistanceTo(position) > 15)
                                {

                                    if (house != null)
                                    {
                                        if (player.Position.DistanceTo(garage.Position) < 15)
                                        {
                                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы вызвали вашу машину", 3000);
                                            garage.SpawnCarAtPosition(player, number, garage.Position + new Vector3(0, 0, 1), rotation);
                                            return;
                                        }
                                    }
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы слишком далеко от машины, метка установлена в GPS", 3000);
                                    Trigger.PlayerEvent(player, "createWaypoint", position.X, position.Y);
                                    return;
                                }
                                if (number != null)
                                {
                                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы вызвали вашу машину", 3000);
                                    garage.SpawnCarAtPosition(player, number, position, rotation);
                                }
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomLeft, "Машина уже взвана, метка установлена в GPS", 3000);
                                Trigger.PlayerEvent(player, "createWaypoint", position.X, position.Y);
                                return;
                            }
                        }
                        else
                        {
                            if (player.Position.DistanceTo(position) < 15)
                            {
                                var vehdata = VehicleManager.Vehicles[number];
                                var veh = NAPI.Vehicle.CreateVehicle((VehicleHash)NAPI.Util.GetHashKey(vehdata.Model), position, rotation, 0, 0);
                                NAPI.Entity.SetEntityRotation(veh, rotation);

                                VehicleStreaming.SetEngineState(veh, false);
                                vehdata.Holder = player.Name;
                                veh.SetData("ACCESS", "PERSONAL");
                                veh.SetData("ITEMS", vehdata.Items);
                                veh.SetData("OWNER", player);
                                veh.SetSharedData("PETROL", vehdata.Fuel);
                                NAPI.Vehicle.SetVehicleNumberPlate(veh, number);
                                VehicleManager.ApplyCustomization(veh);
                                NAPI.Vehicle.SetVehicleNumberPlate(veh, number);
                                VehicleManager.ApplyCustomization(veh);
                                VehicleStreaming.SetLockStatus(veh, false);

                                Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы вызвали вашу машину", 3000);
                            }
                            else
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы слишком далеко от машины, метка установлена в GPS", 3000);
                                Trigger.PlayerEvent(player, "createWaypoint", position.X, position.Y);
                            }
                        }
                    }
                    return;
             }
        }
        #region Menus
        public static void OpenCarsSellMenu(Player player)
        {

        }
        public static void acceptHouseSellToGov(Player player, bool apart)
        {
            House house;
            if (!apart)
                house = HouseManager.GetHouse(player, true);
            else
                house = HouseManager.GetApart(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                return;
            }

            if (Main.Players[player].InsideGarageID != -1)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны выйти из гаража", 3000);
                return;
            }

            house.RemoveAllPlayers();
            house.SetOwner(null);
            house.PetName = "null";
            if (house.Apart == -1)
            {
                Trigger.PlayerEvent(player, "changeBlipColor", house.blip, 2);
                Trigger.PlayerEvent(player, "deleteCheckpoint", 333);
                Trigger.PlayerEvent(player, "deleteGarageBlip");
            }
            if (house.Apart != -1)
                Trigger.PlayerEvent(player, "deleteGarageBlip");
            int price = BCore.GetVipCost(player, house.Price);

            MoneySystem.Wallet.Change(player, price);
            GameLog.Money($"server", $"player({Main.Players[player].UUID})", Convert.ToInt32(house.Price * 0.6), $"houseSell({house.ID})");
            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы продали свой дом государству за {price}$", 3000);
            Trigger.PlayerEvent(player, "client::phone:updateHouseApart", JsonConvert.SerializeObject(NeptuneEvo.Core.Phone.Manager.GetApartInfo(player)), JsonConvert.SerializeObject(NeptuneEvo.Core.Phone.Manager.GetHouseInfo(player)));
        }

        #endregion

        #region Commands
        public static void InviteToRoom(Player player, Player guest)
        {
            var house = HouseManager.GetHouse(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас в доме проживает максимальное кол-во людей", 3000);
                return;
            }

            if (GetHouse(guest) != null || GetApart(guest) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Человек уже живет в доме", 3000);
                return;
            }

            guest.SetData("ROOM_INVITER", player);
            guest.TriggerEvent("openDialog", "ROOM_INVITE", $"Гражданин ({player.Value}) предложил Вам подселиться к нему");

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину ({guest.Value}) подселиться к Вам", 3000);
        }

        public static void InviteToRoomApart(Player player, Player guest)
        {
            var house = HouseManager.GetApart(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет квартиры", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас в квартире проживает максимальное кол-во людей", 3000);
                return;
            }

            if (GetHouse(guest) != null || GetApart(guest) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Человек уже живет в доме", 3000);
                return;
            }

            guest.SetData("ROOM_INVITER", player);
            guest.TriggerEvent("openDialog", "ROOM_INVITE_APART", $"Гражданин ({player.Value}) предложил Вам подселиться к нему");

            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину ({guest.Value}) подселиться к Вам", 3000);
        }

        public static void acceptRoomInviteApart(Player player)
        {
            Player owner = player.GetData<Player>("ROOM_INVITER");
            if (owner == null || !Main.Players.ContainsKey(owner)) return;

            var house = HouseManager.GetApart(owner, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет квартиры", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В доме проживает максимальное кол-во людей", 3000);
                return;
            }

            house.Roommates.Add(player.Name);
            //Trigger.PlayerEvent(player, "createCheckpoint", 333, 27, GarageManager.Garages[house.GarageID].Position - new Vector3(0, 0, 0.2f), 1, NAPI.GlobalDimension, 0, 86, 214);
            //Trigger.PlayerEvent(player, "createGarageBlip", GarageManager.Garages[house.GarageID].Position);
            house.Save();

            Notify.Send(owner, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) подселился к Вам", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы подселились к Гражданину ({owner.Value})", 3000);
        }

        public static void acceptRoomInvite(Player player)
        {
            Player owner = player.GetData<Player>("ROOM_INVITER");
            if (owner == null || !Main.Players.ContainsKey(owner)) return;

            var house = HouseManager.GetHouse(owner, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                return;
            }

            if (house.Roommates.Count >= MaxRoommates[house.Type])
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В доме проживает максимальное кол-во людей", 3000);
                return;
            }

            house.Roommates.Add(player.Name);
            //Trigger.PlayerEvent(player, "createGarageBlip", GarageManager.Garages[house.GarageID].Position);
            house.Save();

            Notify.Send(owner, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) подселился к Вам", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы подселились к Гражданину ({owner.Value})", 3000);
        }

        [Command("cleargarages")]
        public static void CMD_CreateHouse(Player player)
        {
            if (!Group.CanUseCmd(player, "save")) return;

            var list = new List<int>();
            lock (GarageManager.Garages)
            {
                foreach (var g in GarageManager.Garages)
                {
                    var house = Houses.FirstOrDefault(h => h.GarageID == g.Key);
                    if (house == null) list.Add(g.Key);
                }
            }

            foreach (var id in list)
            {
                GarageManager.Garages.Remove(id);
                MySQL.Query($"DELETE FROM `garages` WHERE `id`={id}");
            }
        }

        [Command("createhouse")]
        public static void CMD_CreateHouse(Player player, int type, int price)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if(Main.Players[player].AdminLVL < 100)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"А всё уже, нету доступа", 3000);
                return;
            }
            if (type < 0 || type >= HouseTypeList.Count)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Неправильный тип", 3000);
                return;
            }

            var bankId = MoneySystem.Bank.Create(string.Empty, 2, 0);
            House new_house = new House(GetUID(), string.Empty, type, player.Position - new Vector3(0, 0, 1.12), price, false, 0, bankId, new List<string>(), -1, new List<PostalItem>());
            DimensionID++;
            new_house.Dimension = DimensionID;
            new_house.Create();
            FurnitureManager.Create(new_house.ID);
            new_house.CreateInterior();

            Houses.Add(new_house);
        }

        [Command("removehouse")]
        public static void CMD_RemoveHouse(Player player, int id)
        {
            if (!Group.CanUseCmd(player, "save")) return;

            House house = Houses.FirstOrDefault(h => h.ID == id);
            if (house == null) return;

            house.Destroy();
            Houses.Remove(house);
            MySQL.Query($"DELETE FROM `houses` WHERE `id`='{house.ID}'");
        }
        [Command("houseis")]
        public static void CMD_HouseIs(Player player)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }
            House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            NAPI.Chat.SendChatMessageToPlayer(player, $"{player.GetData<int>("HOUSEID")}");
        }
        [Command("housechange")]
        public static void CMD_HouseOwner(Player player, string newOwner)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }
            House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;

            house.changeOwner(newOwner);
            SavingHouses();
        }

        [Command("housenewprice")]
        public static void CMD_setHouseNewPrice(Player player, int price)
        {
            if (!Group.CanUseCmd(player, "save")) return;
            if (!player.HasData("HOUSEID"))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться на маркере дома", 3000);
                return;
            }

            House house = Houses.FirstOrDefault(h => h.ID == player.GetData<int>("HOUSEID"));
            if (house == null) return;
            house.Price = price;
            house.UpdateLabel();
            house.Save();
        }

        [Command("myguest")]
        public static void CMD_InvitePlayerToHouse(Player player, int id)
        {
            var guest = Main.GetPlayerByID(id);
            if (guest == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не найден", 3000);
                return;
            }
            if (player.Position.DistanceTo(guest.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко", 3000);
                return;
            }
            InvitePlayerToHouse(player, guest);
        }

        public static void InvitePlayerToHouse(Player player, Player guest)
        {
            var house = HouseManager.GetHouse(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет дома", 3000);
                return;
            }

            guest.SetData("InvitedHouse_ID", house.ID);
            Notify.Send(guest, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) пригласил Вас в свой дом", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы пригласили Гражданина ({guest.Value}) в свой дом", 3000);
        }

        public static void InvitePlayerToApart(Player player, Player guest)
        {
            var house = HouseManager.GetApart(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вас нет квартиры", 3000);
                return;
            }

            guest.SetData("InvitedHouse_ID", house.ID);
            Notify.Send(guest, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) пригласил Вас в свой дом", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы пригласили Гражданина ({guest.Value}) в свой дом", 3000);
        }

        [Command("sellhouse")]
        public static void CMD_sellHouse(Player player, int id, int price)
        {
            var target = Main.GetPlayerByID(id);
            if (target == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Гражданин не найден", 3000);
                return;
            }
            OfferHouseSell(player, target, price);
        }

        public static void OfferHouseSell(Player player, Player target, int price)
        {
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от покупателя", 3000);
                return;
            }
            House house = GetHouse(player, true);

            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                return;
            }
            if (GetHouse(target, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина уже есть дом", 3000);
                return;
            }
            if (price > 1000000000 || price < house.Price / 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Слишком большая/маленькая цена", 3000);
                return;
            }
            if (player.Position.DistanceTo(house.Position) > 30)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от дома", 3000);
                return;
            }

            target.SetData("HOUSE_SELLER", player);
            target.SetData("HOUSE_PRICE", price);
            Trigger.PlayerEvent(target, "openDialog", "HOUSE_SELL", $"Гражданин ({player.Value}) предложил Вам купить свой дом за ${price}");
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину ({target.Value}) купить Ваш дом за {price}$", 3000);
        }

        public static void OfferApartSell(Player player, Player target, int price)
        {
            if (player.Position.DistanceTo(target.Position) > 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от покупателя", 3000);
                return;
            }
            House house = GetApart(player, true);


            if (house == null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас нет дома", 3000);
                return;
            }
            if (GetApart(target, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Гражданина уже есть квартира", 3000);
                return;
            }
            if (price > 1000000000 || price < house.Price / 2)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Слишком большая/маленькая цена", 3000);
                return;
            }
            if (player.Position.DistanceTo(house.Position) > 30)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы находитесь слишком далеко от дома", 3000);
                return;
            }

            target.SetData("HOUSE_SELLER", player);
            target.SetData("HOUSE_PRICE", price);
            Trigger.PlayerEvent(target, "openDialog", "APART_SELL", $"Гражданин ({player.Value}) предложил Вам купить свою квартиру за ${price}");
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы предложили Гражданину ({target.Value}) купить Вашу квартиру за {price}$", 3000);
        }

        public static void acceptApartSell(Player player)
        {
            if (!player.HasData("HOUSE_SELLER") || !Main.Players.ContainsKey(player.GetData<Player>("HOUSE_SELLER"))) return;
            Player seller = player.GetData<Player>("HOUSE_SELLER");

            if (GetApart(player, true) != null)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть квартира", 3000);
                return;
            }

            House house = GetApart(seller, true);

            var price = player.GetData<int>("HOUSE_PRICE");
            if (house == null || house.Owner != seller.Name) return;
            if (!MoneySystem.Wallet.Change(player, -price))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                return;
            }
            CheckAndKick(player);
            MoneySystem.Wallet.Change(seller, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", price, $"houseSell({house.ID})");
            seller.TriggerEvent("deleteGarageBlip");
            house.SetOwner(player);
            house.PetName = Main.Players[player].PetName;
            house.Save();

            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) купил у Вас квартиру", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили квартиру у Гражданина ({seller.Value})", 3000);
        }

        public static void acceptHouseSell(Player player)
        {
            if (!player.HasData("HOUSE_SELLER") || !Main.Players.ContainsKey(player.GetData<Player>("HOUSE_SELLER"))) return;
            Player seller = player.GetData<Player>("HOUSE_SELLER");

            if (GetHouse(player, true) != null )
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас уже есть дом", 3000);
                return;
            }

            House house = GetHouse(seller, true);

            var price = player.GetData<int>("HOUSE_PRICE");
            if (house == null || house.Owner != seller.Name) return;
            if (!MoneySystem.Wallet.Change(player, -price))
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно средств", 3000);
                return;
            }
            CheckAndKick(player);
            MoneySystem.Wallet.Change(seller, price);
            GameLog.Money($"player({Main.Players[player].UUID})", $"player({Main.Players[seller].UUID})", price, $"houseSell({house.ID})");
            seller.TriggerEvent("changeBlipColor", house.blip, 2);
            seller.TriggerEvent("deleteCheckpoint", 333);
            seller.TriggerEvent("deleteGarageBlip");
            house.SetOwner(player);
            house.PetName = Main.Players[player].PetName;
            house.Save();

            Notify.Send(seller, NotifyType.Info, NotifyPosition.BottomCenter, $"Гражданин ({player.Value}) купил у Вас дом", 3000);
            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы купили дом у Гражданина ({seller.Value})", 3000);
        }
        #endregion
    }
}

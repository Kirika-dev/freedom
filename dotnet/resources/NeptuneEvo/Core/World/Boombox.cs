using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using NeptuneEVO;

class XMR : Script
{
    [ServerEvent(Event.ResourceStart)]
    public void onResourceStart()
    {
        try
        {
            XMR.XMRs();
        }
        catch { }
    }

    public static int MAX_RADIOS = 5000;
    private static nLog Log = new nLog("Boombox");

    public class XmrEnum : IEquatable<XmrEnum>
    {
        public int id { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public string stream { get; set; }
        public int carid { get; set; }

        [JsonIgnore]
        public int sql_id { get; set; }

        [JsonIgnore]
        public Entity objeto { get; set; }

        [JsonIgnore]
        public TextLabel label { get; set; }
        [JsonIgnore]
        public Marker marker { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            XmrEnum objAsPart = obj as XmrEnum;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public override int GetHashCode()
        {
            return id;
        }
        public bool Equals(XmrEnum other)
        {
            if (other == null) return false;
            return (this.id.Equals(other.id));
        }
    }

    public static List<XmrEnum> xmr_radios = new List<XmrEnum>();
    public static List<dynamic> list_of_radios = new List<dynamic>();


    public static void XMRs()
    {
        for (int i = 0; i < MAX_RADIOS; i++)
        {
            xmr_radios.Add(new XmrEnum { id = i, x = 0, y = 0, z = 0, stream = "null", carid = 0, sql_id = -1 });
        }
    }

    public static void LoadRadiosXMR(Player Client)
    {
        Client.TriggerEvent("LoadXMR", API.Shared.ToJson(xmr_radios));
    }

    public static void OnPlayerDisconnect(Player Client)
    {
        for (int i = 0; i < MAX_RADIOS; i++)
        {
            if (Client != null)
            {
                if (xmr_radios[i].sql_id == Client.Value && xmr_radios[i].carid == 0)
                {
                    xmr_radios[i].sql_id = -1;
                    xmr_radios[i].carid = 0;
                    xmr_radios[i].x = 0;
                    xmr_radios[i].y = 0;
                    xmr_radios[i].z = 0;
                    xmr_radios[i].stream = "null";
                    xmr_radios[i].objeto.Delete();
                    xmr_radios[i].label.Delete();
                    xmr_radios[i].marker.Delete();
                    Client.TriggerEvent("RemoveRadio", i);
                }
            }
        }
    }

    public static void remove_boom_box(Player client)
    {
        for (int i = 0; i < MAX_RADIOS; i++)
        {
            if (xmr_radios[i].sql_id == client.Value && xmr_radios[i].carid == 0)
            {
                xmr_radios[i].sql_id = -1;
                xmr_radios[i].carid = 0;
                foreach (var pl in NAPI.Player.GetPlayersInRadiusOfPosition(50, new Vector3(xmr_radios[i].x, xmr_radios[i].y, xmr_radios[i].z)))
                {
                    pl.TriggerEvent("RemoveRadio", i);
                }
                client.SetSharedData("BOOMBOXON", false);
                xmr_radios[i].x = 0;
                xmr_radios[i].y = 0;
                xmr_radios[i].z = 0;
                xmr_radios[i].stream = "null";
                xmr_radios[i].objeto.Delete();
                xmr_radios[i].label.Delete();
                xmr_radios[i].marker.Delete();
                NeptuneEVO.Core.nInventory.Add(client, new nItem(ItemType.Boombox, 1));
            }
        }
    }

    public static void CMD_setstation(Player Client)
    {
        List<dynamic> menu_item_list = new List<dynamic>();

        foreach (var radio in list_of_radios)
        {
            menu_item_list.Add(new { Type = 1, Name = radio.name, Description = "" });
        }

        if (Client.IsInVehicle && Client.Vehicle.HasSharedData("vehicle_radio_id"))
        {

        }
        else
        {
            int radio_id = -1;
            for (int i = 0; i < MAX_RADIOS; i++)
            {
                if (xmr_radios[i].sql_id == Client.Value && xmr_radios[i].carid == 0)
                {
                    radio_id = i;
                    break;
                }
            }

            if (radio_id == -1)
            {
                Notify.Error(Client, "Вы где-то уже поставили бумбокс");
                return;
            }

            NeptuneEVO.Trigger.PlayerEvent(Client, "client::openboombox");
        }

    }
    [ServerEvent(Event.PlayerDisconnected)]
    public void Event_OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
    {
        try
        {
            if (player.HasSharedData("BOOMBOXON") && player.GetSharedData<bool>("BOOMBOXON"))
            {
                OnPlayerDisconnect(player);
            }
        }
        catch (Exception e) { Log.Write($"PlayerDisconnected (value: {player.Value}): " + e.Message, nLog.Type.Error); }
    }
    [RemoteEvent("server::addmusiconboombox")]
    public static void SelectMenuResponse(Player Client, String objectName)
    {
        if (Client.IsInVehicle)
        {
            return;
        }
        else
        {

            int radio_id = -1;

            for (int i = 0; i < MAX_RADIOS; i++)
            {
                if (xmr_radios[i].sql_id == Client.Value && xmr_radios[i].carid == 0)
                {
                    radio_id = i;
                    break;
                }
            }

            if (radio_id == -1)
            {
                return;
            }

            if (objectName == "")
            {
                for (int i = 0; i < MAX_RADIOS; i++)
                {
                    if (xmr_radios[i].sql_id == Client.Value && xmr_radios[i].carid == 0)
                    {
                        xmr_radios[i].sql_id = -1;
                        xmr_radios[i].carid = 0;
                        xmr_radios[i].x = 0;
                        xmr_radios[i].y = 0;
                        xmr_radios[i].z = 0;
                        xmr_radios[i].stream = "null";
                        xmr_radios[i].objeto.Delete();
                        xmr_radios[i].label.Delete();
                        xmr_radios[i].marker.Delete();

                        foreach (var target in API.Shared.GetAllPlayers())
                        {
                            target.TriggerEvent("RemoveRadio", i);
                        }
                        return;
                    }
                }
            }
            SetBoombox(Client, objectName);
        }
    }


    public static void SetStationVehicle(Player Client, string stream)
    {
        if (Client.IsInVehicle)
        {
            int radio_id = -1;
            for (int i = 0; i < MAX_RADIOS; i++)
            {
                if (xmr_radios[i].carid == Client.Vehicle.GetSharedData<dynamic>("vehicle_radio_id") && xmr_radios[i].sql_id == -1)
                {
                    radio_id = i;

                    break;
                }
            }

            if (radio_id == -1)
            {
                for (int i = 0; i < MAX_RADIOS; i++)
                {
                    if (xmr_radios[i].carid == 0 && xmr_radios[i].sql_id == -1)
                    {
                        radio_id = i;
                        break;
                    }
                }
            }

            if (radio_id == -1)
            {
                return;
            }

            if (stream == "Desligar")
            {
                int i = radio_id;
                xmr_radios[i].sql_id = -1;
                xmr_radios[i].carid = 0;
                xmr_radios[i].x = 0;
                xmr_radios[i].y = 0;
                xmr_radios[i].z = 0;
                xmr_radios[i].stream = "null";

                foreach (var target in API.Shared.GetAllPlayers())
                {
                    target.TriggerEvent("RemoveRadio", i);
                }

                return;
            }


            if (xmr_radios[radio_id].stream == "null")
            {
                xmr_radios[radio_id].carid = Client.Vehicle.GetSharedData<dynamic>("vehicle_radio_id");
                xmr_radios[radio_id].x = Client.Position.X;
                xmr_radios[radio_id].y = Client.Position.Y;
                xmr_radios[radio_id].z = Client.Position.Z;
                xmr_radios[radio_id].stream = stream;

                foreach (var target in API.Shared.GetAllPlayers())
                {
                    target.TriggerEvent("AddRadio", radio_id, Client.Position.X, Client.Position.Y, Client.Position.Z, xmr_radios[radio_id].stream, xmr_radios[radio_id].carid);
                }

            }
            else
            {

                xmr_radios[radio_id].carid = Client.Vehicle.GetSharedData<dynamic>("vehicle_radio_id");
                xmr_radios[radio_id].x = Client.Position.X;
                xmr_radios[radio_id].y = Client.Position.Y;
                xmr_radios[radio_id].z = Client.Position.Z;
                xmr_radios[radio_id].stream = stream;

                foreach (var target in API.Shared.GetAllPlayers())
                {
                    target.TriggerEvent("EditRadio", radio_id, stream);
                }

            }

        }
    }

    public static void CMD_Boombox(Player Client)
    {
        int radio_id = -1;
        for (int i = 0; i < MAX_RADIOS; i++)
        {
            if (xmr_radios[i].sql_id == Client.Value && xmr_radios[i].carid == 0)
            {
                radio_id = i;
                break;
            }
        }
        if (radio_id == -1)
        {
            for (int i = 0; i < MAX_RADIOS; i++)
            {
                if (xmr_radios[i].sql_id == -1 && xmr_radios[i].carid == 0)
                {
                    radio_id = i;
                    break;
                }
            }
        }
        if (radio_id == -1)
        {
            Notify.Error(Client, "Слишком много бумбоксов расположено в штате");
            return;
        }

        if (xmr_radios[radio_id].stream == "null")
        {

            xmr_radios[radio_id].carid = 0;
            xmr_radios[radio_id].sql_id = Client.Value;
            xmr_radios[radio_id].x = Client.Position.X;
            xmr_radios[radio_id].y = Client.Position.Y;
            xmr_radios[radio_id].z = Client.Position.Z;
            xmr_radios[radio_id].stream = "";

            foreach (var target in API.Shared.GetAllPlayers())
            {
                target.TriggerEvent("AddRadio", radio_id, Client.Position.X, Client.Position.Y, Client.Position.Z, xmr_radios[radio_id].stream, xmr_radios[radio_id].carid);
            }

            Client.SetSharedData("BOOMBOXON", true);
            Client.SetData("BOOMBOXONPOSITION", Client.Position);
            NeptuneEVO.Core.nInventory.Remove(Client, new nItem(ItemType.Boombox, 1));
            xmr_radios[radio_id].objeto = API.Shared.CreateObject(NAPI.Util.GetHashKey("prop_tapeplayer_01"), Client.Position - new Vector3(0, 0, 0.8f), new Vector3(0, 0, 0), 255);
            xmr_radios[radio_id].marker = API.Shared.CreateMarker(0, xmr_radios[radio_id].objeto.Position + new Vector3(0, 0, 0.6f), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 0.4f, new Color(126, 116, 255, 120));
            xmr_radios[radio_id].label = API.Shared.CreateTextLabel($"Бумбокс \n {Client.Name.Replace('_',' ')} ({Client.Value})", xmr_radios[radio_id].objeto.Position + new Vector3(0, 0, 0.75f), 5f, 1f, 4, new Color(255, 255, 255));
        }
        else
        {
            List<dynamic> menu_item_list = new List<dynamic>();
            foreach (var radio in list_of_radios)
            {
                menu_item_list.Add(new { Type = 1, Name = radio.name, Description = "" });
            }
            Notify.Error(Client, "У вас уже есть бумбокс");
        }
    }

    public static void Create3DSound(Player player)
    {

    }

    public static void SetBoombox(Player Client, string stream)
    {

        int radio_id = -1;

        for (int i = 0; i < MAX_RADIOS; i++)
        {
            if (xmr_radios[i].sql_id == Client.Value && xmr_radios[i].carid == 0)
            {
                radio_id = i;
                break;
            }
        }
        if (radio_id == -1)
        {
            return;
        }
        if (xmr_radios[radio_id].stream != "null")
        {
            xmr_radios[radio_id].carid = 0;
            xmr_radios[radio_id].sql_id = Client.Value;
            xmr_radios[radio_id].x = Client.Position.X;
            xmr_radios[radio_id].y = Client.Position.Y;
            xmr_radios[radio_id].z = Client.Position.Z;
            xmr_radios[radio_id].stream = stream;

            foreach (var target in API.Shared.GetAllPlayers())
            {
                target.TriggerEvent("EditRadio", radio_id, stream);
            }

            List<Player> proxPlayers = NAPI.Player.GetPlayersInRadiusOfPlayer(45, Client);
            foreach (Player target in proxPlayers)
            {
                if (target.GetData<dynamic>("status") == true)
                {
                    //target.TriggerEvent("Send_ToChat", "", "<font color ='#C2A2DA'>* " + Main.Players[Client].FirstName + "_" + Main.Players[Client].LastName + " changed the boombox radio station. (" + radio_id + ")");
                }
            }
        }
    }
}




 

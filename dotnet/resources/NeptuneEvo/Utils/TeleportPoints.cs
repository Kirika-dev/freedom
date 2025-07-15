using System;
using System.Collections.Generic;
using System.Xml.Schema;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Core
{
    class TeleportPoints : Script
    {

        static nLog Log = new nLog("TeleportPoint");

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                new TeleportPoint("Аэропорт", "Аэропорт", new Vector3(-1058.5121, -2538.0662, 13.94454), new Vector3(4494.155, -4525.5806, 4.4123641), 5000, "Вы уверены что хотите полететь на самолете", 307, 4, 0.65f, false);
                new TeleportPoint("Los Santos CarMeet", "", new Vector3(-2220.0928, 1156.5823, -23.379158), new Vector3(783.0346, -1867.8917, 29.325284), 0, "", 777, 4, 3f, true);
            }
            catch (Exception e) { Log.Write("TeleportPoints on start: " + e.ToString(), nLog.Type.Error); }
        }
        public class TeleportPoint
        {
            public string Name { get; set; }
            public string Name2 { get; set; }
            public Vector3 To { get; set; }
            public Vector3 From { get; set; }
            public int Price { get; set; }
            public string Text { get; set; }
            public uint Sprite { get; set; }
            public byte Color { get; set; }
            public bool Veh { get; set; }
            [JsonIgnore]
            public ColShape ShapeFrom { get; set; }   
            [JsonIgnore]
            public ColShape ShapeTo { get; set; }
            [JsonIgnore]
            public Blip BlipFrom { get; set; }
            [JsonIgnore]
            public Blip BlipTo { get; set; }
            [JsonIgnore]
            public Marker MarkerFrom { get; set; }
            [JsonIgnore]
            public Marker MarkerTo { get; set; }
            public TeleportPoint(string name, string name2, Vector3 to, Vector3 from, int price, string text, uint sprite, byte color, float range = 0.65f, bool veh = false)
            {
                Name = name; Name2 = name2; To = to; From = from; Price = price; Text = text; Sprite = sprite; Color = color; Veh = veh;
                BlipFrom = NAPI.Blip.CreateBlip(Sprite, From, 0.9f, Color, Name, 255, 0, true, 0, 0);
                if (!Veh)
                    BlipTo = NAPI.Blip.CreateBlip(Sprite, To, 0.9f, Color, Name2, 255, 0, true, 0, 0);

                MarkerFrom = NAPI.Marker.CreateMarker(1, From - new Vector3(0, 0, 1), new Vector3(), new Vector3(), range, new Color(67, 140, 239, 200), false, 0);
                MarkerTo = NAPI.Marker.CreateMarker(1, To - new Vector3(0, 0, 1), new Vector3(), new Vector3(), range, new Color(67, 140, 239, 200), false, 0);

                ShapeFrom = NAPI.ColShape.CreateCylinderColShape(From, range + 1.4f, 2, 0);
                ShapeFrom.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 523);
                        Trigger.PlayerEvent(entity, "client::showhintHUD", true, $"{Name}");
                        entity.SetData("TeleportPoint", this);
                        entity.SetData("TeleportPoint_ShapeData", "FROM");
                    }
                    catch (Exception e) { Console.WriteLine("TeleportTo_OnEntityEnterColshape: " + e.Message); }
                };
                ShapeFrom.OnEntityExitColShape += OnEntitiyExitColShape;

                ShapeTo = NAPI.ColShape.CreateCylinderColShape(To, 2, 2, 0);
                ShapeTo.OnEntityEnterColShape += (s, entity) =>
                {
                    try
                    {
                        entity.SetData("INTERACTIONCHECK", 523);
                        Trigger.PlayerEvent(entity, "client::showhintHUD", true, $"{Name}");
                        entity.SetData("TeleportPoint", this);
                        entity.SetData("TeleportPoint_ShapeData", "TO");
                    }
                    catch (Exception e) { Console.WriteLine("TeleportTo_OnEntityEnterColshape: " + e.Message); }
                };
                ShapeTo.OnEntityExitColShape += OnEntitiyExitColShape;
            }
            public void OnEntitiyExitColShape(ColShape s, Player entity)
            {
                Trigger.PlayerEvent(entity, "client::showhintHUD", false, "");
                entity.SetData("INTERACTIONCHECK", -1);
                entity.ResetData("TeleportPoint");
                entity.ResetData("TeleportPoint_ShapeData");
            }
            public void Teleport(Player player)
            {
                if (player.IsInVehicle && !Veh) return;
                if (player.HasData("TeleportPoint") && player.HasData("TeleportPoint_ShapeData"))
                {
                    string state = player.GetData<string>("TeleportPoint_ShapeData");
                    if (state == "FROM")
                    {
                        if (Price != 0)
                        {
                            if (Main.Players[player].Money < Price)
                            {
                                Notify.Error(player, "Недостаточно средств");
                                return;
                            } 
                            MoneySystem.Wallet.Change(player, -Price);
                        }
                        if (Name == "Аэропорт")
                        {
                            BattlePass.AddProgressToQuest(player, 12, 1);
                        }
                        Trigger.PlayerEvent(player, "client::screen:transition", 400, 2000, 400, null);
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null)
                                {
                                    if (!Veh)
                                    {
                                        NAPI.Entity.SetEntityPosition(player.Handle, To);
                                        Trigger.PlayerEvent(player, "freeze", true);
                                    }
                                    if (Veh)
                                    {
                                        if (player.IsInVehicle)
                                        {
                                            NAPI.Entity.SetEntityPosition(player.Vehicle, To);
                                            player.SetIntoVehicle(player.Vehicle, 0);
                                        }
                                        else
                                        {
                                            NAPI.Entity.SetEntityPosition(player.Handle, To);
                                            Trigger.PlayerEvent(player, "freeze", true);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }, 500);
                    }
                    if (state == "TO")
                    {
                        if (Price != 0)
                        {
                            if (Main.Players[player].Money < Price)
                            {
                                Notify.Error(player, "Недостаточно средств");
                                return;
                            }
                            MoneySystem.Wallet.Change(player, -Price);
                        }
                        Trigger.PlayerEvent(player, "client::screen:transition", 400, 2000, 400, null);
                        NAPI.Task.Run(() =>
                        {
                            try
                            {
                                if (player != null)
                                {
                                    if (!Veh)
                                    {
                                        NAPI.Entity.SetEntityPosition(player.Handle, From);
                                        Trigger.PlayerEvent(player, "freeze", true);
                                    }
                                    if (Veh)
                                    {
                                        if (player.IsInVehicle)
                                        {
                                            NAPI.Entity.SetEntityPosition(player.Vehicle, From);
                                            player.SetIntoVehicle(player.Vehicle, 0);
                                        }
                                        else
                                        {
                                            NAPI.Entity.SetEntityPosition(player.Handle, From);
                                            Trigger.PlayerEvent(player, "freeze", true);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }, 500);
                    }
                }
            }
        }

    }
}

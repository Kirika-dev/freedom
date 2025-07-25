﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;

using NeptuneEVO;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;
using Newtonsoft.Json;

namespace NeptuneEVO.Voice
{
    public class Voice : Script
    {
        private static nLog Log = new nLog("Voice");
        public Voice()
        {
            RoomController.getInstance().CreateRoom("VoiceRoom");
        }

        public Player GetPlayerById(int id)
        {
            Player target = null;
            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (player.Value == id)
                {
                    target = player;
                    break;
                }
            }
            return target;
        }
        
        public static void PlayerJoin(Player player)
        {
            try
            {
                VoiceMetaData DefaultVoiceMeta = new VoiceMetaData
                {
                    IsEnabledMicrophone = false,
                    RadioRoom = "",
                    StateConnection = "closed",
                    MicrophoneKey = 78 // N
                };

                VoicePhoneMetaData DefaultVoicePhoneMeta = new VoicePhoneMetaData
                {
                    CallingState = "nothing",
                    Target = null
                };

                player.SetData("Voip", DefaultVoiceMeta);
                player.SetData("PhoneVoip", DefaultVoicePhoneMeta);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        
        public static void PlayerQuit(Player player, string reson)
        {
            try
            {
                RoomController controller = RoomController.getInstance();
                VoiceMetaData voiceMeta = player.GetData<VoiceMetaData>("Voip");

                if (controller.HasRoom(voiceMeta.RadioRoom))
                {
                    controller.OnQuit(voiceMeta.RadioRoom, player);
                }

                VoicePhoneMetaData playerPhoneMeta = player.GetData<VoicePhoneMetaData>("PhoneVoip");

                if (playerPhoneMeta.Target != null)
                {
                    Player target = playerPhoneMeta.Target;
                    VoicePhoneMetaData targetPhoneMeta = target.GetData<VoicePhoneMetaData>("PhoneVoip");

                    var pSim = Main.Players[player].Sim;
                    var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();

                    Notify.Send(target, NotifyType.Alert, NotifyPosition.BottomCenter, $"{playerName} завершил вызов", 3000);
                    targetPhoneMeta.Target = null;
                    targetPhoneMeta.CallingState = "nothing";

                    target.ResetData("AntiAnimDown");
                    if (!target.IsInVehicle) target.StopAnimation();
                    else target.SetData("ToResetAnimPhone", true);

                    NeptuneEVO.Core.BasicSync.DetachObject(target);

                    NeptuneEVO.Trigger.PlayerEvent(target, "voice.phoneStop");

                    target.SetData("PhoneVoip", targetPhoneMeta);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        [Command("v_reload")]
        public void voiceDebugReload(Player player)
        {
            player.SendChatMessage("Вы успешно перезагрузили голосовой чат для себя (v1).");
            NeptuneEVO.Trigger.PlayerEvent(player, "v_reload");
        }

        [Command("v_reload2")]
        public void voiceDebug2Reload(Player player)
        {
            player.SendChatMessage("Вы успешно перезагрузили голосовой чат для себя (v2).");
            NeptuneEVO.Trigger.PlayerEvent(player, "v_reload2");
        }

        [Command("v_reload3")]
        public void voiceDebug3Reload(Player player)
        {
            player.SendChatMessage("Вы успешно перезагрузили голосовой чат для себя (v3).");
            NeptuneEVO.Trigger.PlayerEvent(player, "v_reload3");
        }

        [RemoteEvent("setmutelist")]
        public void add_voice_listener(Player player, string list)
        {
            try
            {
                player.SetData("MUTELIST", JsonConvert.DeserializeObject<List<string>>(list));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [RemoteEvent("add_voice_listener")]
        public void add_voice_listener(Player player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                Player target = (Player)arguments[0];
                if (!Main.Players.ContainsKey(target)) return;

                if (target.HasData("MUTELIST"))
                    if (target.GetData<List<string>>("MUTELIST").Contains(player.Value.ToString()))
                        return;
                player.EnableVoiceTo(target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [RemoteEvent("remove_voice_listener")]
        public void remove_voice_listener(Player player, params object[] arguments)
        {
            try
            {
                if (player == null) return;
                if (!Main.Players.ContainsKey(player)) return;
                Player target = (Player)arguments[0];
                if (target == null) return;
                if (!Main.Players.ContainsKey(target)) return;
                
                player.DisableVoiceTo(target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // METHODS //
        
        public static void PhoneCallCommand(Player player, Player target)
        {
            try
            {
                if (target != null && Main.Players.ContainsKey(target))
                {
                    VoicePhoneMetaData targetPhoneMeta = target.GetData<VoicePhoneMetaData>("PhoneVoip");
                    VoicePhoneMetaData playerPhoneMeta = player.GetData<VoicePhoneMetaData>("PhoneVoip");

                    if (playerPhoneMeta.Target != null)
                    {
                        Trigger.PlayerEvent(player, "client::phone:calloff", 2);
                        return;
                    }

                    var tSim = Main.Players[target].Sim;
                    var pSim = Main.Players[player].Sim;

                    var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();
                    var targetName = (Main.Players[player].Contacts.ContainsKey(tSim)) ? Main.Players[player].Contacts[tSim] : tSim.ToString();

                    if (targetPhoneMeta.Target != null)
                    {
                        Trigger.PlayerEvent(player, "client::phone:calloff", 4);
                        NAPI.Chat.SendChatMessageToPlayer(player, $"Номер {pSim} пытался Вам дозвониться");
                        return;
                    }

                    targetPhoneMeta.Target = player;
                    targetPhoneMeta.CallingState = "callMe";

                    playerPhoneMeta.Target = target;
                    playerPhoneMeta.CallingState = "callTo";

                    Main.OnAntiAnim(player);
                    player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                    NeptuneEVO.Core.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));

                    player.SetData("PhoneVoip", playerPhoneMeta);
                    target.SetData("PhoneVoip", targetPhoneMeta);

                    NAPI.Task.Run(() => {
                        try
                        {
                            if (!Main.Players.ContainsKey(player) || !Main.Players.ContainsKey(target)) return;

                            VoicePhoneMetaData tPhoneMeta = target.GetData<VoicePhoneMetaData>("PhoneVoip");
                            VoicePhoneMetaData pPhoneMeta = player.GetData<VoicePhoneMetaData>("PhoneVoip");

                            if (pPhoneMeta.Target == null || pPhoneMeta.Target != target || pPhoneMeta.CallingState == "talk") return;

                            pPhoneMeta.Target = null;
                            tPhoneMeta.Target = null;

                            pPhoneMeta.CallingState = "nothing";
                            tPhoneMeta.CallingState = "nothing";

                            if (!player.IsInVehicle)
                                player.StopAnimation();
                            else
                                player.SetData("ToResetAnimPhone", true);
                            NeptuneEVO.Core.BasicSync.DetachObject(player);

                            player.SetData("PhoneVoip", pPhoneMeta);
                            target.SetData("PhoneVoip", tPhoneMeta);

                            player.ResetData("AntiAnimDown");

                            Trigger.PlayerEvent(player, "client::phone:calloff", 2);
                            Trigger.PlayerEvent(target, "client::phone:calloff", 2);
                        }
                        catch { }
                        
                    }, 20000);

                    Trigger.PlayerEvent(target, "client::phone:callopen", pSim, 3);
                    Trigger.PlayerEvent(player, "client::phone:callopen", tSim, 0);
                }
                else
                {
                    Trigger.PlayerEvent(player, "client::phone:calloff", 2);
                }

            }
            catch (Exception e)
            {
                Log.Write($"PhoneCall: {e.Message}", nLog.Type.Error);
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_PlayerExitVehicle(Player player, Vehicle veh)
        {
            try
            {
                if (player.HasData("ToResetAnimPhone"))
                {
                    player.StopAnimation();
                    player.ResetData("ToResetAnimPhone");
                }
            }
            catch { }
        }

        [RemoteEvent("server::phone:callaccept")]
        public static void PhoneCallAcceptCommand(Player player)
        {
            try
            {
                VoicePhoneMetaData playerPhoneMeta = player.GetData<VoicePhoneMetaData>("PhoneVoip");

                if (playerPhoneMeta.Target == null || playerPhoneMeta.CallingState == "callTo" || !Main.Players.ContainsKey(playerPhoneMeta.Target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент Вам никто не звонит", 3000);
                    return;
                }

                Player target = playerPhoneMeta.Target;

                VoicePhoneMetaData targetPhoneMeta = target.GetData<VoicePhoneMetaData>("PhoneVoip");

                playerPhoneMeta.CallingState = "talk";
                targetPhoneMeta.CallingState = "talk";

                var tSim = Main.Players[target].Sim;
                var pSim = Main.Players[player].Sim;

                var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();
                var targetName = (Main.Players[player].Contacts.ContainsKey(tSim)) ? Main.Players[player].Contacts[tSim] : tSim.ToString();

                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"{playerName} принял Ваш вызов", 3000);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы приняли вызов от {targetName}", 3000);

                Main.OnAntiAnim(player);
                player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                NeptuneEVO.Core.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));

                Trigger.PlayerEvent(target, "client::phone:callopen", pSim, 1);
                Trigger.PlayerEvent(player, "client::phone:callopen", tSim, 1);

                player.ResetData("ToResetAnimPhone");
                target.ResetData("ToResetAnimPhone");

                player.SetData("PhoneVoip", playerPhoneMeta);
                target.SetData("PhoneVoip", targetPhoneMeta);
            }
            catch (Exception e)
            {
                Log.Write($"PhoneCallAccept: {e.Message}", nLog.Type.Error);
            }
        }

        [Command("server::phone:callend")]
        public static void PhoneHCommand(Player player)
        {
            try
            {
                VoicePhoneMetaData playerPhoneMeta = player.GetData<VoicePhoneMetaData>("PhoneVoip");

                if (playerPhoneMeta.Target == null || !Main.Players.ContainsKey(playerPhoneMeta.Target))
                {
                    if (!player.HasData("IS_DYING") && !player.GetData<bool>("CUFFED")) Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент Вы не говорите по телефону", 3000);
                    return;
                }

                Player target = playerPhoneMeta.Target;
                VoicePhoneMetaData targetPhoneMeta = target.GetData<VoicePhoneMetaData>("PhoneVoip");

                var tSim = Main.Players[target].Sim;
                var pSim = Main.Players[player].Sim;

                var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();
                var targetName = (Main.Players[player].Contacts.ContainsKey(tSim)) ? Main.Players[player].Contacts[tSim] : tSim.ToString();

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Звонок завершен", 3000);
                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"{playerName} завершил звонок", 3000);

                playerPhoneMeta.Target = null;
                targetPhoneMeta.Target = null;

                playerPhoneMeta.CallingState = "nothing";
                targetPhoneMeta.CallingState = "nothing";

                if (!player.IsInVehicle) player.StopAnimation();
                if (!target.IsInVehicle) target.StopAnimation();

                player.ResetData("AntiAnimDown");
                target.ResetData("AntiAnimDown");
                if (player.IsInVehicle) player.SetData("ToResetAnimPhone", true);
                if (player.IsInVehicle) target.SetData("ToResetAnimPhone", true);

                NeptuneEVO.Core.BasicSync.DetachObject(player);
                NeptuneEVO.Core.BasicSync.DetachObject(target);

                NeptuneEVO.Trigger.PlayerEvent(player, "voice.phoneStop");
                NeptuneEVO.Trigger.PlayerEvent(target, "voice.phoneStop");

                player.SetData("PhoneVoip", playerPhoneMeta);
                target.SetData("PhoneVoip", targetPhoneMeta);
            }
            catch (Exception e)
            {
                Log.Write($"PhoneCallCancel: {e.Message}", nLog.Type.Error);
            }
        }

        //[Command("changeroom")]
        public void ChangeRoomCommand(Player player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();
                    VoiceMetaData voiceMeta = player.GetData<VoiceMetaData>("Voip");

                    if (controller.HasRoom(name))
                    {
                        if (name.Equals(voiceMeta.RadioRoom))
                        {
                            player.SendChatMessage("You are already on this room");
                            return;
                        }

                        controller.OnQuit(name, player);
                        controller.OnJoin(name, player);
                    }
                    else
                    {
                        player.SendChatMessage("This room doesn't exist");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //[Command("createroom")]
        public void CreateRoomCommand(Player player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();

                    if (!controller.HasRoom(name))
                    {
                        controller.CreateRoom(name);

                        player.SendChatMessage("You create room - " + name);
                    }
                    else
                    {
                        player.SendChatMessage("Room already created");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //[Command("removeroom")]
        public void RemoveRoomCommand(Player player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();

                    if (controller.HasRoom(name))
                    {
                        controller.RemoveRoom(name);

                        player.SendChatMessage("You has removed room - " + name);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //[Command("leaveroom")]
        public void LeaveRoomCommand(Player player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();

                    if (controller.HasRoom(name))
                    {
                        VoiceMetaData voiceMeta = player.GetData<VoiceMetaData>("Voip");

                        if (name.Equals(voiceMeta.RadioRoom))
                        {
                            controller.OnQuit(name, player);
                        }

                        player.SendChatMessage("You leave from room - " + name);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SetVoiceDistance(Player player, float distance)
        {
            player.SetSharedData("voice.distance", distance);
        }

        public float GetVoiceDistance(Player player)
        {
            return player.GetSharedData<float>("voice.distance");
        }

        public bool IsMicrophoneEnabled(Player player)
        {
            VoiceMetaData voiceMeta = player.GetData<VoiceMetaData>("Voip");

            return voiceMeta.IsEnabledMicrophone;
        }

        public void SetVoiceMuted(Player player, bool isMuted)
        {
            player.SetSharedData("voice.muted", isMuted);
        }

        public bool GetVoiceMuted(Player player)
        {
            return player.GetSharedData<bool>("voice.muted");
        }

        public void SetMicrophoneKey(Player player, int microphoneKey)
        {
            try
            {
                VoiceMetaData voiceMeta = player.GetData<VoiceMetaData>("Voip");
                voiceMeta.MicrophoneKey = microphoneKey;

                NeptuneEVO.Trigger.PlayerEvent(player, "voice.changeMicrophoneActivationKey", microphoneKey);
                player.SetData("Voip", voiceMeta);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public int GetMicrophoneKey(Player player)
        {
            VoiceMetaData voiceMeta = player.GetData<VoiceMetaData>("Voip");
            return voiceMeta.MicrophoneKey;
        }
    }
}
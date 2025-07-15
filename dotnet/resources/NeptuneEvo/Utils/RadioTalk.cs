using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using NeptuneEVO.Core;
using NeptuneEVO.SDK;

namespace NeptuneEVO.Utilis
{
    class Walkie : Script
    {
        private static nLog Log = new nLog("walkie");
        private static List<Player> Listeners = new List<Player>();
        private static Random rnd = new Random();
        private static Dictionary<int, int> FractionsFrequency = new Dictionary<int, int>()
        {
            {6, 0 },
            {7, 0 },
            {8, 0 },
            {9, 0 },
            {14, 0 }
        };

        #region ServerEvents
        public static void ResourceStart()
        {
            int randNum = rnd.Next(1, 180);
            int gg = randNum;
            for (int i = 6; i < 14; i++)
            {
                gg += 4;
                FractionsFrequency[i] = gg;
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public static void PlayerDisc(Player player, DisconnectionType type, string reason)
        {
            if (player.GetData<bool>("LISTENER_WALKIE") == true)
            {
                Listeners.Remove(player);
            }
        }
        [ServerEvent(Event.PlayerDeath)]
        public static void PlayerDic(Player player, Player entityKiller, uint weapon)
        {
            if (player.GetData<bool>("LISTENER_WALKIE") == true)
            {
                Listeners.Remove(player);
            }
        }
        #endregion

        #region RemoteEvents
        public static void OpenWalkie(Player player)
        {
            player.ResetData("Frequency");
            switch (Main.Players[player].FractionID)
            {
                case 6:
                    player.SetData<int>("Frequency", FractionsFrequency[6]);
                    player.SetData<bool>("LISTENER_WALKIE", true);
                    player.SetSharedData("LISTENER_RADIO", true);
                    Listeners.Add(player);
                    NAPI.ClientEvent.TriggerClientEvent(player, "walkie.open", FractionsFrequency[6]);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Для того чтобы говорить в рацию, зажмите стрелочку вверх.", 5000);
                    break;
                case 7:
                    player.SetData<int>("Frequency", FractionsFrequency[7]);
                    player.SetData<bool>("LISTENER_WALKIE", true);
                    player.SetSharedData("LISTENER_RADIO", true);
                    Listeners.Add(player);
                    NAPI.ClientEvent.TriggerClientEvent(player, "walkie.open", FractionsFrequency[7]);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Для того чтобы говорить в рацию, зажмите стрелочку вверх.", 5000);
                    break;
                case 8:
                    player.SetData<bool>("LISTENER_WALKIE", true);
                    player.SetSharedData("LISTENER_RADIO", true);
                    Listeners.Add(player);
                    player.SetData<int>("Frequency", FractionsFrequency[8]);
                    NAPI.ClientEvent.TriggerClientEvent(player, "walkie.open", FractionsFrequency[8]);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Для того чтобы говорить в рацию, зажмите стрелочку вверх.", 5000);
                    break;
                case 9:
                    player.SetData<int>("Frequency", FractionsFrequency[9]);
                    player.SetData<bool>("LISTENER_WALKIE", true);
                    player.SetSharedData("LISTENER_RADIO", true);
                    Listeners.Add(player);
                    NAPI.ClientEvent.TriggerClientEvent(player, "walkie.open", FractionsFrequency[9]);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Для того чтобы говорить в рацию, зажмите стрелочку вверх.", 5000);
                    break;
                case 14:
                    player.SetData<int>("Frequency", FractionsFrequency[14]);
                    player.SetData<bool>("LISTENER_WALKIE", true);
                    player.SetSharedData("LISTENER_RADIO", true);
                    Listeners.Add(player);
                    NAPI.ClientEvent.TriggerClientEvent(player, "walkie.open", FractionsFrequency[14]);
                    Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, "Для того чтобы говорить в рацию, зажмите стрелочку вверх.", 5000);
                    break;
            }
        }

        [RemoteEvent("ChangeFrequency")]
        public static void ChangeFrequency(Player player, int Frequency)
        {
            player.ResetData("Frequency");
            player.SetData<int>("Frequency", Frequency);
        }

        [RemoteEvent("closeWalkie")]
        public static void CloseWalkie(Player player)
        {
            player.SetData<bool>("LISTENER_WALKIE", false);
            player.SetSharedData("LISTENER_RADIO", false);
            Listeners.Remove(player);
            NAPI.ClientEvent.TriggerClientEvent(player, "walkie.close");
        }

        [RemoteEvent("talkingInWalkie")]
        public static void TalkingWalkie(Player player)
        {
            try
            {
                if (!player.IsInVehicle)
                {
                    BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_cs_walkie_talkie"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));
                    player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                }
                player.SetData<bool>("TalkingInWalkie", true);
                foreach (Player p in Listeners)
                {
                    if (p == player) continue;
                    if (player.GetData<int>("Frequency") != p.GetData<int>("Frequency")) continue;
                    NAPI.ClientEvent.TriggerClientEvent(p, "walkie.talking", player);
                    NAPI.ClientEvent.TriggerClientEvent(player, "walkie.talking", p);
                    if (!p.IsInVehicle)
                    {
                        BasicSync.AttachObjectToPlayer(p, NAPI.Util.GetHashKey("prop_cs_walkie_talkie"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));
                        p.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                    }
                    player.EnableVoiceTo(p);
                }
            }
            catch (Exception e)
            {
                Log.Write($"TalkingWalkie: {e.Message}", nLog.Type.Error);
            }
        }

        [RemoteEvent("DisableTalkingWalkie")]
        public static void DisableTalkingWalkie(Player player)
        {
            try
            {
                if (!player.IsInVehicle)
                {
                    player.StopAnimation();
                }
                BasicSync.DetachObject(player);
                NAPI.ClientEvent.TriggerClientEvent(player, "walkie.playSound");
                foreach (Player p in Listeners)
                {
                    if (p == player) continue;
                    if (player.GetData<int>("Frequency") != p.GetData<int>("Frequency")) continue;
                    if (p.GetData<bool>("TalkingInWalkie") == true) continue;
                    p.ResetData("TalkingInWalkie");
                    NAPI.ClientEvent.TriggerClientEvent(p, "walkie.disableTalking", player);
                    player.DisableVoiceTo(p);
                    if (!p.IsInVehicle)
                    {
                        p.StopAnimation();
                    }
                    BasicSync.DetachObject(p);
                    NAPI.ClientEvent.TriggerClientEvent(p, "walkie.playSound");
                }
            }
            catch (Exception e)
            {
                Log.Write($"DisableTalkingWalkie: {e.Message}", nLog.Type.Error);
            }
        }
        #endregion
    }
}
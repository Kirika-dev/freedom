using System;
using System.Collections.Generic;
using System.Xml.Schema;
using GTANetworkAPI;
using Newtonsoft.Json;
using NeptuneEVO.SDK;
using System.Data;

namespace NeptuneEVO.Core
{
    class CreatorBlips : Script
    {
        private static nLog Log = new nLog("CreateBlips");
        private static int LastID = 1;
        public static List<CustomBlip> Blips = new List<CustomBlip>();

        private static int AdminLVLToOpenMenu = 10;

        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                var result = MySQL.QueryRead($"SELECT * FROM merumond_blips");

                foreach (DataRow Row in result.Rows)
                {
                    int id = Convert.ToInt32(Row["id"].ToString());
                    BlipsSettings settings = JsonConvert.DeserializeObject<BlipsSettings>(Row["settings"].ToString());
                    new CustomBlip(id, settings);
                }
                Loading();
            }
            catch (Exception e) { Log.Write("Ошибка запуска CreatorBlips: " + e.ToString(), nLog.Type.Error); }
        }
        #region RemoteEvents
        [RemoteEvent("server::creatorblips:open")]
        public static void ClientEvent_OpenMenu(Player player)
        {
            if (Main.Players[player].AdminLVL < AdminLVLToOpenMenu) return;
            NAPI.ClientEvent.TriggerClientEvent(player, "client::creatorblips:open", JsonConvert.SerializeObject(Blips));
        }

        [RemoteEvent("server::creatorblips:tptoblip")]
        public static void ClientEvent_TeleportToBlip(Player player, int id)
        {
            CustomBlip selectblip = Blips.Find(x => x.ID == id);
            if (selectblip == null) { Notify.Error(player, "Блип не найден!"); return; };
            NAPI.Entity.SetEntityPosition(player, selectblip.BlipSettings.Position + new Vector3(0,0,1.2));
            Notify.Succ(player, $"Вы телепортировались на блип #{id}");
        }

        [RemoteEvent("server::creatorblips:changeposition")]
        public static void ClientEvent_ChangePosition(Player player, int id)
        {
            CustomBlip selectblip = Blips.Find(x => x.ID == id);
            if (selectblip == null) { Notify.Error(player, "Блип не найден!"); return; };
            selectblip.BlipSettings.Position = player.Position;
            Notify.Succ(player, "Позиция была изменена");
            UpdateBlips();
        }

        [RemoteEvent("server::creatorblips:create")]
        public static void ClientEvent_Create(Player player)
        {
            CustomBlip blp = new CustomBlip(LastID, new BlipsSettings(1, player.Position, 1f, 1, "Made with BlipCreator", 255, true, 0, player.Dimension));
            MySQL.Query($"INSERT INTO `merumond_blips` (`id`, `settings`) VALUES({blp.ID},'{JsonConvert.SerializeObject(blp.BlipSettings)}')");
            Notify.Succ(player, $"Вы создали блип номер {LastID - 1}!");
            UpdateListForClient(player, false);
        }

        [RemoteEvent("server::creatorblips:remove")]
        public static void ClientEvent_Remove(Player player, int id)
        {
            CustomBlip selectblip = Blips.Find(x => x.ID == id);
            if (selectblip == null) { Notify.Error(player, "Блип не найден!"); return; };
            selectblip.Blip.Delete();
            Blips.Remove(selectblip);
            MySQL.Query($"DELETE FROM `merumond_blips` WHERE `id`='{id}'");
            UpdateListForClient(player);
            UpdateBlips();
        }

        [RemoteEvent("server::creatorblips:setsettings")]
        public static void ClientEvent_SetNewSettings(Player player, int id, string settings)
        {
            BlipsSettings ns = JsonConvert.DeserializeObject<BlipsSettings>(settings);
            CustomBlip selectblip = Blips.Find(x => x.ID == id);
            if (selectblip == null) { Notify.Error(player, "Блип не найден!"); return; };
            selectblip.BlipSettings = ns;
            MySQL.Query($"UPDATE `merumond_blips` SET `settings`='{JsonConvert.SerializeObject(selectblip.BlipSettings)}' WHERE id={id}");
            UpdateListForClient(player);
            UpdateBlips();
        }
        #endregion

        #region Обновления данных
        public static void UpdateBlips()
        {
            foreach(CustomBlip blip in Blips)
                blip.Reload();

            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (!Main.Players.ContainsKey(player)) continue;
                if (Main.Players[player].AdminLVL >= 1)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "!{#ffa800}[CREATOR BLIPS] !{#ffffff}Конфиг блипов обновлен!");
                }
            }
        }

        public static void UpdateListForClient(Player player, bool update = true)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "client::creatorblips:update", JsonConvert.SerializeObject(Blips));
            if (update)
                Notify.Succ(player, "Конфиг блипов обновлен!");
        }
        #endregion

        #region Модуль Блипа
        public class CustomBlip
        {
            public int ID { get; set; } = 0;
            public BlipsSettings BlipSettings { get; set; }
            [JsonIgnore]
            public GTANetworkAPI.Blip Blip { get; set; }

            public CustomBlip(int id, BlipsSettings settings)
            {
                ID = id; BlipSettings = settings;
                Blip = NAPI.Blip.CreateBlip(BlipSettings.Sprite, BlipSettings.Position, BlipSettings.Scale, BlipSettings.Color, BlipSettings.Name, BlipSettings.Alpha, 0, BlipSettings.ShortRange, BlipSettings.Rotation, BlipSettings.Dimension);
                LastID = id + 1;

                Blips.Add(this);
            }
            public void Reload()
            {
                Blip.Position = BlipSettings.Position;
                Blip.Sprite = Convert.ToUInt32(BlipSettings.Sprite);
                Blip.Scale = BlipSettings.Scale;
                Blip.Color = BlipSettings.Color;
                Blip.Name = BlipSettings.Name;
                Blip.Transparency = BlipSettings.Alpha;
                Blip.ShortRange = BlipSettings.ShortRange;
                Blip.Dimension = BlipSettings.Dimension;
            }
        }
        #endregion

        #region Модуль настройки блипа
        public class BlipsSettings
        {
            public int Sprite { get; set; } = 1;
            public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
            public float Scale { get; set; } = 1f;
            public byte Color { get; set; } = 1;
            public string Name { get; set; } = "Made with CreatorBlips";
            public byte Alpha { get; set; } = 255;
            public bool ShortRange { get; set; } = true;
            public short Rotation { get; set; } = 0;
            public uint Dimension { get; set; } = 0;
            public BlipsSettings(int sprite, Vector3 pos, float scale, byte color, string name, byte alpha, bool shortrange, short rotation, uint dim)
            {
                Sprite = sprite; Position = pos; Scale = scale; Color = color; Name = name; Alpha = alpha; ShortRange = shortrange; Rotation = rotation; Dimension = dim;
            }
        }
        #endregion

        #region by RAGEMP.PRO (Все для RAGE:MP. Портал о мультиплеере).
        public static void Loading()
        {
            Console.WriteLine("========================================================================");
            Console.WriteLine("");
            Console.WriteLine("██████╗░░█████╗░░██████╗░███████╗███╗░░░███╗██████╗░░░░██████╗░██████╗░░█████╗░");
            Console.WriteLine("██╔══██╗██╔══██╗██╔════╝░██╔════╝████╗░████║██╔══██╗░░░██╔══██╗██╔══██╗██╔══██╗");
            Console.WriteLine("██████╔╝███████║██║░░██╗░█████╗░░██╔████╔██║██████╔╝░░░██████╔╝██████╔╝██║░░██║");
            Console.WriteLine("██╔══██╗██╔══██║██║░░╚██╗██╔══╝░░██║╚██╔╝██║██╔═══╝░░░░██╔═══╝░██╔══██╗██║░░██║");
            Console.WriteLine("██║░░██║██║░░██║╚██████╔╝███████╗██║░╚═╝░██║██║░░░░░██╗██║░░░░░██║░░██║╚█████╔╝");
            Console.WriteLine("╚═╝░░╚═╝╚═╝░░╚═╝░╚═════╝░╚══════╝╚═╝░░░░░╚═╝╚═╝░░░░░╚═╝╚═╝░░░░░╚═╝░░╚═╝░╚════╝░");
            Console.WriteLine("");
            Console.WriteLine("█▀▀ █▀█ █▀▀ █▀▀ █▀▄ █▀█ █▀▄▀█   █▀▀ ▄▀█ █▀▄▀█ █▀▀ █▀▄▀█ █▀█ █▀▄ █▀▀");
            Console.WriteLine("█▀░ █▀▄ ██▄ ██▄ █▄▀ █▄█ █░▀░█   █▄█ █▀█ █░▀░█ ██▄ █░▀░█ █▄█ █▄▀ ██▄");
            Console.WriteLine("");
            Console.WriteLine("========================================================================");

            Log.Write("RAGEMP.PRO");
        }
        #endregion
    }
}

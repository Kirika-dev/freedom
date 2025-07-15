using GTANetworkAPI;
using NeptuneEVO.SDK;
using System.Collections.Generic;

namespace NeptuneEVO.Core
{
    class AdminSP : Script
    {   
        public static void Spectate(Player player, int id)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (id >= 0 && id < NAPI.Server.GetMaxPlayers())
                {
                    Player target = Main.GetPlayerByID(id);
                    if (target != null)
                    {
                        if (target != player)
                        {
                            NAPI.Task.Run(() => {
                                if (Main.Players.ContainsKey(target))
                                {
                                    if (target.GetData<bool>("spmode") == false)
                                    {
                                    
                                        if (player.GetData<bool>("spmode") == false)
                                        { // Не сохраняем новые данные о позиции, если мы уже в режиме слежки
                                            player.SetData("sppos", player.Position);
                                            player.SetData("spdim", player.Dimension);
                                        }
                                        else NAPI.ClientEvent.TriggerClientEvent(player, "spmode", null, false, null); // Если уже за кем-то SPшит и потом на другюго, то сначала deattach
                                
                                        player.SetSharedData("INVISIBLE", true); // Ваша переменная с Вашей системы инвизов, чтобы игроки не видели ника над головой
                                        player.SetData("spmode", true);
                                        player.SetData("spPlayer", target.Value);
                                        player.Transparency = 0; // Сначала устанавливаем игроку полную прозрачность, а только потом телепортируем к игроку
                                        player.Dimension = target.Dimension;
                                        player.Position = new Vector3(target.Position.X, target.Position.Y, (target.Position.Z + 4.5)); // Сначала телепортируем к игроку, чтобы он загрузился
                                        List<object> playerInfo = new List<object>
                                        {
                                             target.Name.Replace("_"," "),
                                             target.Value,
                                             target.GetSharedData<int>("UID"),
                                             Main.Players[target].LVL,
                                             $"{Main.Players[target].EXP}/{3 + Main.Players[target].LVL * 3}",
                                             target.Ping,
                                             Main.Players[target].CreateDate,
                                             Main.Players[target].Warns,
                                             Main.Players[target]. FractionID > 0 ? Fractions.Manager.FractionNames[Main.Players[target].FractionID] : "Нет",
                                             Main.Players[target].FractionID > 0 ? Fractions.Manager.getNickname(Main.Players[target].FractionID, Main.Players[target].FractionLVL) : "Нет",
                                             (Main.Players[target].WorkID > 0) ? Jobs.WorkManager.JobStats[Main.Players[target].WorkID - 1] : "Безработный"
                                        };
                                        NAPI.ClientEvent.TriggerClientEvent(player, "spmode", target, true, Newtonsoft.Json.JsonConvert.SerializeObject(playerInfo)); // И только потом аттачим админа к игроку
                                        player.SendChatMessage("Вы наблюдаете за " + target.Name + " [ID: " + target.Value + "].");
                                    }
                                }
                                else player.SendChatMessage("Игрок под данным ID еще не авторизовался.");
                            });
                        }
                    }
                    else player.SendChatMessage("Игрок под ID " + id + " отсутствует.");
                }
                else player.SendChatMessage("ID игрока недействительно (меньше 0 или больше количества слотов).");
            }
        }

        [RemoteEvent("UnSpectate")]
        public static void RemoteUnSpectate(Player player)
        {
            if (!Main.Players.ContainsKey(player)) return;
            if (!Group.CanUseCmd(player, "sp")) return;
            UnSpectate(player);
        }
        
        public static void UnSpectate(Player player)
        {
            if (Main.Players.ContainsKey(player))
            {
                if (player.GetData<bool>("spmode") == true)
                {
                    NAPI.ClientEvent.TriggerClientEvent(player, "spmode", null, false, null);
                    player.SetData("spPlayer", -1);
                    Timers.StartOnce(400, () => {
                        NAPI.Task.Run(() => { 
                            player.Dimension = player.GetData<uint>("spdim");
                            player.Position = player.GetData<Vector3>("sppos"); // Сначала возвращаем игрока на исходное местоположение, а только потом восстанавливаем прозрачность
                            player.Transparency = 255;
                            player.SetSharedData("INVISIBLE", false); // Включаем видимость ника и отключаем отображение хп всех игроков рядом
                            player.SetData("spmode", false);
                            player.SendChatMessage("Вы вышли из режима наблюдателя.");
                        });
                    });
                }
                else player.SendChatMessage("Вы не находитесь в режиме наблюдателя.");
            }
        }
    }
}

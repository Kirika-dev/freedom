﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using GTANetworkAPI;
using NeptuneEVO.SDK;
using NeptuneEVO.GUI;
using MySqlConnector;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace NeptuneEVO.Core.nAccount
{
    public class Account : AccountData
    {
        private static nLog Log = new nLog("Account");

        public async Task<RegisterEvent> Register(Player client, string login_, string pass_, string email_)
        {
            try
            {
                if (Main.Accounts.ContainsKey(client)) Main.Accounts.Remove(client);

                if (Main.SocialClubs.Contains(client.SocialClubName) || Main.SocialClubs.Contains(client.GetData<string>("RealSocialClub"))) return RegisterEvent.SocialReg;

                if (login_.Length < 1 || pass_.Length < 1 || email_.Length < 1) return RegisterEvent.DataError;
                if (Main.Usernames.Contains(login_)) return RegisterEvent.UserReg;

                if (Main.Emails.ContainsKey(email_)) return RegisterEvent.EmailReg;
                if (!email_.Contains('@')) return RegisterEvent.EmailRegEr;

                Password = GetSha256(pass_);
                Login = login_;
                Email = email_;
                VipLvl = 0;
                PromoCodes = new List<string>();

                Characters = new List<int>() { -1, -1, -2 }; // -1 - empty slot, -2 - non-purchased slot

                HWID = client.Serial;
                if (client.Address.Equals("80.235.53.64")) IP = "31.13.190.88";
                else IP = client.Address;
                SocialClub = client.SocialClubName;
                await MySQL.QueryAsync($"INSERT INTO `accounts` (`login`,`email`,`password`,`hwid`,`ip`,`socialclub`,`redbucks`,`viplvl`,`vipdate`,`promocodes`,`character1`,`character2`,`character3`) " +
                    $"VALUES ('{Login}','{Email}','{Password}','{HWID}','{IP}','{SocialClub}',0,{VipLvl},'{MySQL.ConvertTime(VipDate)}','{JsonConvert.SerializeObject(PromoCodes)}',-1,-1,-2)");

                Main.SocialClubs.Add(SocialClub);
                Main.Usernames.Add(Login);
                Main.Emails.Add(Email, Login);
                Main.Accounts.Add(client, this);

                MoneySystem.Donations.newNames.Enqueue(Login);
                	                Trigger.PlayerEvent(client, "CharacterCreatorName");
                Trigger.PlayerEvent(client, "AuthClose");
                if (!Main.LoggedIn.ContainsKey(login_)) Main.LoggedIn.Add(login_, client);
                return RegisterEvent.Registered;
            }
            catch (Exception ex)
            {
                await Log.WriteAsync(ex.ToString(), nLog.Type.Error);
                return RegisterEvent.Error;
            }
        }
        public async Task<LoginEvent> LoginIn(Player client, string login_, string pass_)
        {
            try
            {
                //if (!Main.Usernames.Contains(login_)) return LoginEvent.Refused;
                // На всякий, ибо говнокод
                login_ = login_.ToLower();

                pass_ = GetSha256(pass_);
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = "SELECT * FROM `accounts` WHERE `login`=@login AND password=@password";
                cmd.Parameters.AddWithValue("@login", login_);
                cmd.Parameters.AddWithValue("@password", pass_);
                DataTable result = MySQL.QueryRead(cmd);
                //Если база не вернула таблицу, то отправляем сброс
                if (result == null || result.Rows.Count == 0) return LoginEvent.Refused;
                //Иначе, парсим строку
                DataRow row = result.Rows[0];
                //Далее делаем разбор и оперируем данными
                Login = Convert.ToString(row["login"]);
                Email = Convert.ToString(row["email"]);
                Password = pass_;
                //Служебные данные
                NAPI.Task.Run(() =>
                {
                    HWID = client.Serial;
                });
                NAPI.Task.Run(() =>
                {
                    if (client.Address.Equals("80.235.53.64")) IP = "31.13.190.88";
                    else IP = client.Address;
                });
                SocialClub = row["socialclub"].ToString();
                if (Main.SCCheck)
                {
                    if (SocialClub != client.GetData<string>("RealSocialClub")) return LoginEvent.SclubError;
                }

                RedBucks = Convert.ToInt32(row["redbucks"]);
                VipLvl = Convert.ToInt32(row["viplvl"]);
                VipDate = (DateTime)row["vipdate"];

                PromoCodes = JsonConvert.DeserializeObject<List<string>>(row["promocodes"].ToString());
                var char1 = Convert.ToInt32(row["character1"]);
                var char2 = Convert.ToInt32(row["character2"]);
                var char3 = Convert.ToInt32(row["character3"]);
                Characters = new List<int>() { char1, char2, char3 };

                PresentGet = Convert.ToBoolean(row["present"]);

                CustomPromo = row["custompromo"].ToString();

                bool inaccount = false;

                foreach(AccountData account in Main.Accounts.Values)
                    if (account.Login == login_)
                    {
                        inaccount = true;
                        break;
                    }


                if (inaccount)
                    return LoginEvent.Already;
                else if (Main.LoggedIn.ContainsKey(login_))
                    Main.LoggedIn.Remove(login_);
                Main.LoggedIn.Add(login_, client);

                //if(Main.AdminSlots.ContainsKey(SocialClub)) Main.AdminSlots[SocialClub].Logged = true;

                Main.Accounts.Add(client, this);

                return LoginEvent.Authorized;
            }
            catch (Exception ex)
            {
                await Log.WriteAsync(ex.ToString(), nLog.Type.Error);
                return LoginEvent.Error;
            }
        }
        public async Task<bool> Save(Player player)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = "UPDATE `accounts` SET `password`=@pass,`email`=@email,`socialclub`=@sc,`redbucks`=@red,`viplvl`=@vipl,`hwid`=@hwid,`ip`=@ip," +
                    "`vipdate`=@vipd,`character1`=@charf,`character2`=@charn,`character3`=@charm,`present`=@pres, `custompromo`=@cust WHERE `login`=@login";
                cmd.Parameters.AddWithValue("@pass", Password);
                cmd.Parameters.AddWithValue("@email", Email);
                cmd.Parameters.AddWithValue("@sc", SocialClub);
                cmd.Parameters.AddWithValue("@red", RedBucks);
                cmd.Parameters.AddWithValue("@vipl", VipLvl);
                cmd.Parameters.AddWithValue("@hwid", HWID);
                cmd.Parameters.AddWithValue("@ip", IP);
                cmd.Parameters.AddWithValue("@vipd", MySQL.ConvertTime(VipDate));
                cmd.Parameters.AddWithValue("@charf", Characters[0]);
                cmd.Parameters.AddWithValue("@charn", Characters[1]);
                cmd.Parameters.AddWithValue("@charm", Characters[2]);
                cmd.Parameters.AddWithValue("@pres", PresentGet);
                cmd.Parameters.AddWithValue("@login", Login);
                cmd.Parameters.AddWithValue("@cust", CustomPromo);
                await MySQL.QueryAsync(cmd);

                return true;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"Save\":\n" + e.ToString(), nLog.Type.Error);
                return false;
            }
        }
        public void LoadSlots(Player player)
        {
            try
            {
                List<object> data = new List<object>();
                foreach (int uuid in Characters)
                {
                    if (uuid > -1)
                    {
                        List<object> subData = new List<object>();

                        var ban = Ban.Get2(uuid);
                        if (ban != null && ban.CheckDate())
                        {
                            subData.Add("ban");
                            subData.Add(ban.Reason);
                            subData.Add(ban.ByAdmin);
                            subData.Add($"{ban.Time.ToShortDateString()} {ban.Time.ToShortTimeString()}");
                            subData.Add($"{ban.Until.ToShortDateString()} {ban.Until.ToShortTimeString()}");
                        }
                        else
                        {
                            if (Main.PlayerNames.ContainsKey(uuid) && Main.PlayerSlotsInfo.ContainsKey(uuid))
                            {
                                string name = Main.PlayerNames[uuid];
                                string[] split = name.Split('_');
                                Tuple<int, int, int, long> tuple = Main.PlayerSlotsInfo[uuid];

                                subData.Add(split[0]);
                                subData.Add(split[1]);
                                subData.Add(tuple.Item1);
                                subData.Add(tuple.Item2);
                                subData.Add(Fractions.Manager.FractionNames[tuple.Item3]);
                                subData.Add(tuple.Item4);
                                if (Main.PlayerBankAccs.ContainsKey(name)) subData.Add(MoneySystem.Bank.Get(Main.PlayerBankAccs[name]).Balance);
                                else subData.Add("ERROR");
                            }
                            else
                            {
                                if (Main.LoggedIn.ContainsKey(Login)) Main.LoggedIn.Remove(Login);
                                Notify.Send(player, NotifyType.Error, NotifyPosition.Center, $"К сожалению, невозможно получить данные о персонаже с номером паспорта {uuid}, обратитесь в тех.раздел на форуме.", 5000);
                                return;
                            }
                        }
                        data.Add(subData);
                    }
                    else data.Add(uuid);
                }
                data.Add(RedBucks);
                data.Add(Login);
                Trigger.PlayerEvent(player, "toslots", JsonConvert.SerializeObject(data));
            }
            catch (Exception e)
            {
                if (Main.LoggedIn.ContainsKey(Login)) Main.LoggedIn.Remove(Login);
                Notify.Send(player, NotifyType.Error, NotifyPosition.Center, "К сожалению, невозможно получить данные о персонажах аккаунта, сообщите в тех.раздел на форуме.", 5000);
                Log.Write("EXCEPTION AT \"LoadSlots\":\n" + e.ToString(), nLog.Type.Error);
                return;
            }
        }
        public async Task CreateCharacter(Player player, int slot, string firstName, string lastName)
        {
            if (Characters[slot - 1] != -1) return;
            var character = new Character.Character();
            var result = await character.Create(player, firstName, lastName);
            if (result == -1) return;

            Characters[slot - 1] = result;
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = $"UPDATE `accounts` SET `character{slot}`={result} WHERE `login`=@login";
            cmd.Parameters.AddWithValue("@login", Login);
            MySQL.Query(cmd);

            Main.Players[player].Spawn(player);
        }
        public async Task DeleteCharacter(Player player, int slot, string firstName_, string lastName_, string password_)
        {
            if (Characters[slot - 1] == -1 || Characters[slot - 1] == -2) return;

            var result = await MySQL.QueryReadAsync($"SELECT `firstname`,`lastname`,`biz`,`sim`,`bank` FROM `characters` WHERE uuid={Characters[slot - 1]}");
            if (result == null || result.Rows.Count == 0) return;
            Ban ban = Ban.Get2(Characters[slot - 1]);
            if (ban != null && ban.CheckDate())
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно удалить персонажа, который находится в бане.", 3000);
                return;
            }
            var row = result.Rows[0];
            var firstName = row["firstname"].ToString();
            var lastName = row["lastname"].ToString();
            var biz = JsonConvert.DeserializeObject<List<int>>(row["biz"].ToString());
            var sim = Convert.ToInt32(row["sim"]);
            var bank = Convert.ToInt32(row["bank"]);
            var uuid = Characters[slot - 1];

            if (firstName != firstName_ || lastName != lastName_)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Имя и фамилия не соответствуют персонажу на выбранном слоте", 3000);
                return;
            }

            password_ = GetSha256(password_);
            if (Password != password_)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Неправильный пароль от аккаунта", 3000);
                return;
            }

            foreach (var b in biz)
                Businesses.BCore.changeOwner($"{firstName}_{lastName}", "Государство");

            await MySQL.QueryAsync("DELETE FROM `customization` WHERE uuid=" + uuid);

            nInventory.Items.Remove(uuid);
            await MySQL.QueryAsync("DELETE FROM `inventory` WHERE uuid=" + uuid);

            MoneySystem.Bank.Remove(bank, $"{firstName}_{lastName}");

            var vehicles = VehicleManager.getAllPlayerVehicles($"{firstName}_{lastName}");
            foreach (var v in vehicles)
                VehicleManager.Remove(v);

            await MySQL.QueryAsync("DELETE FROM `characters` WHERE uuid=" + uuid);

            Main.UUIDs.Remove(uuid);
            Main.PlayerNames.Remove(uuid);
            Main.PlayerUUIDs.Remove($"{firstName}_{lastName}");
            Main.PlayerBankAccs.Remove($"{firstName}_{lastName}");
            Main.SimCards.Remove(sim);
            Main.PlayerSlotsInfo.Remove(uuid);
            Customization.CustomPlayerData.Remove(uuid);

            Characters[slot - 1] = -1;
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = $"UPDATE accounts SET character{slot}=-1 WHERE login='@login";
            cmd.Parameters.AddWithValue("@login", Login);
            MySQL.Query(cmd);

            GameLog.CharacterDelete($"{firstName}_{lastName}", uuid, Login);

            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Персонаж {firstName} {lastName} успешно удален", 3000);
            NAPI.Task.Run(() => Trigger.PlayerEvent(player, "delCharSuccess", slot));
        }
        public void changePassword(string newPass)
        {
            Password = GetSha256(newPass);
            //TODO: Logging ths action
        }
        public static string GetSha256(string strData)
        {
            var message = Encoding.ASCII.GetBytes(strData);
            var hashString = new SHA256Managed();
            var hex = "";

            var hashValue = hashString.ComputeHash(message);
            foreach (var x in hashValue)
                hex += string.Format("{0:x2}", x);
            return hex;
        }
    }

    public enum LoginEvent
    {
        Already,
        Authorized,
        Refused,
        SclubError,
        Error
    }
    public enum RegisterEvent
    {
        Registered,
        SocialReg,
        UserReg,
        EmailReg,
        DataError,
        Error,
        EmailRegEr
    }
}
using GTANetworkAPI;
using NeptuneEVO.Fractions.Activity;
using NeptuneEVO.SDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeptuneEVO.Core
{
    class Items : Script
    {
        private static nLog Log = new nLog("Items");

        public static List<int> ItemsDropped = new List<int>();
        public static List<int> InProcessering = new List<int>();
        [ServerEvent(Event.EntityDeleted)]
        public void Event_OnEntityDeleted(Entity entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) == EntityType.Object && NAPI.Data.HasEntityData(entity, "DELETETIMER"))
                {
                    Timers.Stop(NAPI.Data.GetEntityData(entity, "DELETETIMER"));
                    ItemsDropped.Remove(NAPI.Data.GetEntityData(entity, "ID"));
                    InProcessering.Remove(NAPI.Data.GetEntityData(entity, "ID"));
                }
                else if (NAPI.Entity.GetEntityType(entity) == EntityType.Vehicle && NAPI.Data.HasEntityData(entity, "TRUNK"))
                {
                    Vehicle veh = NAPI.Entity.GetEntityFromHandle<Vehicle>(entity);
                    Log.Write($"Removed veh {veh.Model} | {veh.NumberPlate}", nLog.Type.Warn);
                    Player player = NAPI.Data.GetEntityData(entity, "TRUNK");
                    NAPI.Data.ResetEntityData(entity, "TRUNK");

                    player.ResetSharedData("attachToVehicleTrunk");
                    Trigger.PlayerEventInRange(player.Position, 500, "vehicledeattach", player);
                    Main.OffAntiAnim(player);
                    player.StopAnimation();
                    player.ResetData("VEH");
                }
            }
            catch (Exception e)
            {
                Log.Write("Event_OnEntityDeleted: " + e.Message, nLog.Type.Error);
            }
        }

        public static void deleteObject(GTANetworkAPI.Object obj, nItem item)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    obj.ResetData("DELETETIMER");
                    ItemsDropped.Remove(obj.GetData<int>("ID"));
                    InProcessering.Remove(obj.GetData<int>("ID"));
                    obj.Delete();
                    GarbageCans.GarbageCan gc = GarbageCans.GetNearestGarbageCan(obj.Position);
                    int tryAdd = GarbageCans.GarbageCan.TryAdd(gc, item);
                    if (tryAdd == -1 || tryAdd > 0) return;
                    GarbageCans.GarbageCan.Add(gc, item);
                }
                catch (Exception e)
                {
                    Log.Write("UpdateObject: " + e.Message, nLog.Type.Error);
                }
            }, 0);
        }

        public static void onUse(Player player, nItem item, int index)
        {
            try
            {
                var gender = Main.Players[player].Gender;
                var UUID = Main.Players[player].UUID;
                if (!nInventory.ClothesItems.Contains(item.Type) && (player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") != item.Type) && (player.HasSharedData("INVENTORY_ITEMINHANDS") && player.GetSharedData<bool>("INVENTORY_ITEMINHANDS") == true))
                {
                    Notify.Error(player, "Сначала уберите предмет из рук");
                    return;
                }
                #region ClothesAll
                if (nInventory.ClothesItems.Contains(item.Type) && item.Type != ItemType.BodyArmor && item.Type != ItemType.Mask)
                {
                    var data = (string)item.Data;
                    var clothesGender = Convert.ToBoolean(data.Split('_')[2]);
                    if (clothesGender != Main.Players[player].Gender)
                    {
                        var error_gender = (clothesGender) ? "мужская" : "женская";
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Это {error_gender} одежда", 3000);
                        return;
                    }
                    if ((player.GetData<bool>("ON_DUTY") && Fractions.Manager.FractionTypes[Main.Players[player].FractionID] == 2 && Main.Players[player].FractionID != 9 && Main.Players[player].FractionID != 15 && Main.Players[player].FractionID != 17 && Main.Players[player].FractionID != 18) || player.GetData<bool>("ON_WORK"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете использовать это сейчас", 3000);
                        return;
                    }
                }
                #endregion

                #region Weapons
                if (nInventory.WeaponsItems.Contains(item.Type) || nInventory.MeleeWeaponsItems.Contains(item.Type))
                {
                    if (item.Type == ItemType.SnowBall)
                    {
                        nInventory.Remove(player, item.Type, 1);
                        Trigger.PlayerEvent(player, "wgive", (int)Weapons.GetHash(item.Type.ToString()), 1, false, true);
                        return;
                    }
                    if (item.IsActive)
                    {
                        var wHash = Weapons.GetHash(item.Type.ToString());
                        Trigger.PlayerEvent(player, "takeOffWeapon", (int)wHash);
                        Commands.RPChat("me", player, $"убрал(а) {nInventory.InventoryItems.Find(x => x.ID == (int)item.Type).Name}");
                    }
                    else
                    {
                        // if (player.HasSharedData("attachToVehicleTrunk")) return;
                        var oldwItem = nInventory.Items[UUID].FirstOrDefault(i => (nInventory.WeaponsItems.Contains(i.Type) || nInventory.MeleeWeaponsItems.Contains(i.Type)) && i.IsActive);
                        if (oldwItem != null)
                        {
                            var oldwHash = Weapons.GetHash(oldwItem.Type.ToString());
                            Trigger.PlayerEvent(player, "serverTakeOffWeapon", (int)oldwHash);
                            oldwItem.IsActive = false;
                            InvInterface.Update(player, oldwItem, nInventory.Items[UUID].IndexOf(oldwItem));
                            Commands.RPChat("me", player, $"убрал(а) {nInventory.InventoryItems.Find(x => x.ID == (int)oldwItem.Type).Name}");
                        }

                        var wHash = Weapons.GetHash(item.Type.ToString());
                        if (Weapons.WeaponsAmmoTypes.ContainsKey(item.Type))
                        {
                            var ammoItem = nInventory.Find(UUID, Weapons.WeaponsAmmoTypes[item.Type]);
                            var ammo = (ammoItem == null) ? 0 : ammoItem.Count;
                            if (ammo > Weapons.WeaponsClipsMax[item.Type]) ammo = Weapons.WeaponsClipsMax[item.Type];
                            if (ammoItem != null) nInventory.Remove(player, ammoItem.Type, ammo);
                            Trigger.PlayerEvent(player, "wgive", (int)wHash, ammo, false, true);
                        }
                        else
                        {
                            Trigger.PlayerEvent(player, "wgive", (int)wHash, 1, false, true);
                        }

                        Commands.RPChat("me", player, $"достал(а) {nInventory.InventoryItems.Find(x => x.ID == (int)item.Type).Name}");
                        item.IsActive = true;
                        player.SetData("LastActiveWeap", item.Type);
                        InvInterface.Update(player, item, index);
                    }
                    return;
                }
                #endregion

                #region AlcoItems
                if (nInventory.AlcoItems.Contains(item.Type))
                {
                    int stage = Convert.ToInt32(item.Type.ToString().Split("Drink")[1]);
                    int curStage = player.GetData<int>("RESIST_STAGE");

                    if (player.HasData("RESIST_BAN"))
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы пьяны до такой степени, что не можете открыть бутылку", 3000);
                        return;
                    }

                    var stageTimes = new List<int>() { 0, 300, 420, 600 };

                    if (curStage == 0 || curStage == stage)
                    {
                        player.SetData("RESIST_STAGE", stage);
                        player.SetData("RESIST_TIME", player.GetData<int>("RESIST_TIME") + stageTimes[stage]);
                    }
                    else if (curStage < stage)
                    {
                        player.SetData("RESIST_STAGE", stage);
                    }
                    else if (curStage > stage)
                    {
                        player.SetData("RESIST_TIME", player.GetData<int>("RESIST_TIME") + stageTimes[stage]);
                    }

                    if (player.GetData<int>("RESIST_TIME") >= 1500)
                        player.SetData("RESIST_BAN", true);

                    Trigger.PlayerEvent(player, "setResistStage", player.GetData<int>("RESIST_STAGE"));
                    BasicSync.AttachObjectToPlayer(player, nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Model, 57005, nInventory.AlcoPosOffset[item.Type], nInventory.AlcoRotOffset[item.Type]);

                    Main.OnAntiAnim(player);
                    player.PlayAnimation("amb@world_human_drinking@beer@male@idle_a", "idle_c", 49);
                    NAPI.Task.Run(() =>
                    {
                        try
                        {
                            if (player != null)
                            {
                                if (!player.IsInVehicle) player.StopAnimation();
                                else player.SetData("ToResetAnimPhone", true);
                                Main.OffAntiAnim(player);
                                Trigger.PlayerEvent(player, "startScreenEffect", "PPFilter", player.GetData<int>("RESIST_TIME") * 1000, false);
                                BasicSync.DetachObject(player);
                            }
                        }
                        catch { }
                    }, 5000);

                    Commands.RPChat("me", player, "выпил бутылку " + nInventory.InventoryItems.Find(x => x.ID == (int)item.Type).Name);
                    GameLog.Items($"player({Main.Players[player].UUID})", "use", Convert.ToInt32(item.Type), 1, $"{item.Data}");
                }
                #endregion

                switch (item.Type)
                {
                    #region Clothes
                    case ItemType.Glasses:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Glasses.Variation = -1;
                                player.ClearAccessory(1);
                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                var mask = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation;
                                if (Customization.MaskTypes.ContainsKey(mask) && Customization.MaskTypes[mask].Item3)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете надеть эти очки с маской", 3000);
                                    return;
                                }
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Glasses = new ComponentItem(variation, texture);
                                player.SetAccessories(1, variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                                Main.PlayAnimation(player, "clothingspecs", "try_glasses_positive_a", 1, 7000);
                            }
                            return;
                        }
                    case ItemType.Hat:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Variation = -1;

                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                var mask = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation;
                                if (Customization.MaskTypes.ContainsKey(mask) && Customization.MaskTypes[mask].Item2)
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не можете надеть этот головной убор с маской", 3000);
                                    return;
                                }
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                            }
                            Customization.SetHat(player, Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Texture);
                            return;
                        }
                    case ItemType.Mask:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask = new ComponentItem(Customization.EmtptySlots[gender][1], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask = new ComponentItem(variation, texture);

                                if (Customization.MaskTypes.ContainsKey(variation))
                                {
                                    if (Customization.MaskTypes[variation].Item1)
                                    {
                                        player.SetClothes(2, 0, 0);
                                    }
                                    if (Customization.MaskTypes[variation].Item2)
                                    {
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Hat.Variation = -1;
                                        nInventory.UnActiveItem(player, ItemType.Hat);
                                        Customization.SetHat(player, -1, 0);
                                    }
                                    if (Customization.MaskTypes[variation].Item3)
                                    {
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Glasses.Variation = -1;
                                        nInventory.UnActiveItem(player, ItemType.Glasses);
                                        player.ClearAccessory(1);
                                    }
                                }

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                            }
                            Customization.SetMask(player, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Mask.Texture);
                            return;
                        }
                    case ItemType.Gloves:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(0, 0);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                if (!Customization.CorrectGloves[gender][variation].ContainsKey(Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Variation)) return;
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(variation, texture);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(Customization.CorrectGloves[gender][variation][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Variation], texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                                Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                            }
                            player.SetClothes(3, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso.Texture);
                            return;
                        }
                    case ItemType.Leg:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg = new ComponentItem(Customization.EmtptySlots[gender][4], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                                Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                            }
                            player.SetClothes(4, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Leg.Texture);
                            return;
                        }
                    case ItemType.Bag:
                        {
                            if (item.IsActive)
                            {

                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag = new ComponentItem(Customization.EmtptySlots[gender][5], 0);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Texture = 1;
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Variation = 0;
                                player.ResetData("BAG_UP");
                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                                if (player.HasData("OPENOUT_TYPE") && player.GetData<int>("OPENOUT_TYPE") == 10)
                                {
                                    Trigger.PlayerEvent(player, "client::inventory:setinfo", 5, false);
                                    player.ResetData("OPENOUT_TYPE");
                                }
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                                player.SetData("BAG_UP", true);
                                Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);

                                Bag.OpenBackPack(player);
                            }
                            player.SetClothes(5, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bag.Texture);
                            return;
                        }
                    case ItemType.Feet:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet = new ComponentItem(Customization.EmtptySlots[gender][6], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                                Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                            }
                            player.SetClothes(6, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Feet.Texture);
                            return;
                        }
                    case ItemType.Jewelry:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory = new ComponentItem(Customization.EmtptySlots[gender][7], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory = new ComponentItem(variation, texture);

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                                Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                            }
                            player.SetClothes(7, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Accessory.Texture);
                            return;
                        }
                    case ItemType.Accessories:
                        {
                            var itemData = (string)item.Data;
                            var variation = Convert.ToInt32(itemData.Split('_')[0]);
                            var texture = Convert.ToInt32(itemData.Split('_')[1]);

                            if (item.IsActive)
                            {
                                var watchesSlot = Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches;
                                if (watchesSlot.Variation == variation && watchesSlot.Texture == texture)
                                {
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches = new ComponentItem(-1, 0);
                                    player.ClearAccessory(6);
                                }
                                else
                                {
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Bracelets = new ComponentItem(-1, 0);
                                    player.ClearAccessory(7);
                                }

                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                if (Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches.Variation == -1)
                                {
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Watches = new ComponentItem(variation, texture);
                                    player.SetAccessories(6, variation, texture);

                                    nInventory.Items[UUID][index].IsActive = true;
                                    InvInterface.Update(player, item, index);
                                    Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                                }
                                else if (Customization.AccessoryRHand[gender].ContainsKey(variation))
                                {
                                    if (Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Bracelets.Variation == -1)
                                    {
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Accessory.Bracelets = new ComponentItem(Customization.AccessoryRHand[gender][variation], texture);
                                        player.SetAccessories(7, Customization.AccessoryRHand[gender][variation], texture);

                                        nInventory.Items[UUID][index].IsActive = true;
                                        InvInterface.Update(player, item, index);
                                        Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                                    }
                                    else
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Заняты обе руки", 3000);
                                        return;
                                    }
                                }
                                else
                                {
                                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Левая рука занята, а на правой никто часы не носит", 3000);
                                    return;
                                }
                            }
                            return;
                        }
                    case ItemType.Undershit:
                        {
                            var itemData = (string)item.Data;
                            var underwearID = Convert.ToInt32(itemData.Split('_')[0]);
                            var underwear = Customization.Underwears[gender][underwearID];
                            var texture = Convert.ToInt32(itemData.Split('_')[1]);
                            if (item.IsActive)
                            {
                                if (underwear.Top == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation)
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(Customization.EmtptySlots[gender][11], 0);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);

                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation == Customization.EmtptySlots[gender][11])
                                {
                                    if (underwear.Top == -1)
                                    {
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эту одежду можно одеть только под низ верхней", 3000);
                                        return;
                                    }
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(underwear.Top, texture);

                                    nInventory.UnActiveItem(player, item.Type);
                                    nInventory.Items[UUID][index].IsActive = true;
                                    InvInterface.Update(player, item, index);
                                    Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                                }
                                else
                                {
                                    var nowTop = Customization.Tops[gender].FirstOrDefault(t => t.Variation == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation);
                                    if (nowTop != null)
                                    {
                                        var topType = nowTop.Type;
                                        if (!underwear.UndershirtIDs.ContainsKey(topType))
                                        {
                                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эта одежда несовместима с Вашей верхней одеждой", 3000);
                                            return;
                                        }
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(underwear.UndershirtIDs[topType], texture);

                                        nInventory.UnActiveItem(player, item.Type);
                                        nInventory.Items[UUID][index].IsActive = true;
                                        InvInterface.Update(player, item, index);
                                        Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                                    }
                                    else
                                    {
                                        if (underwear.Top == -1)
                                        {
                                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Эту одежду можно одеть только под низ верхней", 3000);
                                            return;
                                        }
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(underwear.Top, texture);

                                        nInventory.UnActiveItem(player, item.Type);
                                        nInventory.Items[UUID][index].IsActive = true;
                                        InvInterface.Update(player, item, index);
                                        Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                                    }
                                }
                            }

                            var gloves = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation;
                            if (gloves != 0 &&
                                !Customization.CorrectGloves[gender][gloves].ContainsKey(Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation]))
                            {
                                nInventory.UnActiveItem(player, ItemType.Gloves);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(0, 0);
                            }

                            player.SetClothes(8, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture);
                            player.SetClothes(11, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Texture);
                            var noneGloves = Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation];
                            if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation == 0)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(noneGloves, 0);
                                player.SetClothes(3, noneGloves, 0);
                            }
                            else
                                player.SetClothes(3, Customization.CorrectGloves[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation][noneGloves], Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Texture);
                            return;
                        }
                    case ItemType.BodyArmor:
                        {
                            if (item.IsActive)
                            {
                                item.Data = player.Armor.ToString();
                                player.Armor = 0;
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 0;
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 0;
                                player.SetClothes(9, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture);
                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                var armor = Convert.ToInt32((string)item.Data);
                                player.Armor = armor;
                                switch (Main.Players[player].FractionID)
                                {
                                    case -1:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        // nInventory.UnActiveItem(player, item.Type);
                                        // nInventory.Items[UUID][index].IsActive = true;
                                        // InvInterface.Update(player, item, index);
                                        // Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не вступили в фракцию", 3000);

                                        break;
                                    case 1:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 0; //The families
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 2:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 2; //The Ballas Gang
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 3:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 1; //Los Santos Vagos
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 4:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 5; //Marabunta Grande
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 5:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 4; //Blood Street
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 6:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 8; //GOV
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 7:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 9; //LSPD police
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 8:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7; //EMS
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 9:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 6; //FBI 
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 10:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7; //Мафия 1
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 11:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7; //Мафия 2
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 12:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7; //Мафия 3
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 13:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7; //Мафия 4
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 14:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 9; // Army
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 17:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 9; // Меривейзер
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    case 18:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 9; // Групп6
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;

                                        break;
                                    default:
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture = 7;
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation = 28;
                                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У вашей фракции нет бронежилета.", 3000);

                                        break;

                                }
                                player.SetClothes(9, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Bodyarmor.Texture);
                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                                Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                            }

                            return;
                        }
                    case ItemType.Unknown:
                        {
                            if (item.IsActive)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals = new ComponentItem(Customization.EmtptySlots[gender][10], 0);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals = new ComponentItem(variation, texture);
                            }
                            player.SetClothes(10, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Decals.Texture);
                            return;
                        }
                    case ItemType.Top:
                        {
                            if (item.IsActive)
                            {
                                if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == Customization.EmtptySlots[gender][8] || (!gender && Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == 15))
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(Customization.EmtptySlots[gender][11], 0);
                                else
                                {
                                    var underwearID = Customization.Undershirts[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation];
                                    var underwear = Customization.Underwears[gender][underwearID];
                                    if (underwear.Top == -1)
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(Customization.EmtptySlots[gender][11], 0);
                                    else
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(underwear.Top, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture);
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);
                                }

                                nInventory.Items[UUID][index].IsActive = false;
                                InvInterface.Update(player, item, index);
                            }
                            else
                            {
                                var itemData = (string)item.Data;
                                var variation = Convert.ToInt32(itemData.Split('_')[0]);
                                var texture = Convert.ToInt32(itemData.Split('_')[1]);

                                if (Customization.Tops[gender].FirstOrDefault(t => t.Variation == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation) != null || Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation == Customization.EmtptySlots[gender][11])
                                {
                                    if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == Customization.EmtptySlots[gender][8] || (!gender && Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation == 15))
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(variation, texture);
                                    else
                                    {
                                        var underwearID = Customization.Undershirts[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation];
                                        var underwear = Customization.Underwears[gender][underwearID];
                                        var underwearTexture = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture;
                                        var topType = Customization.Tops[gender].FirstOrDefault(t => t.Variation == variation).Type;
                                        Log.Debug($"UnderwearID: {underwearID} | TopType: {topType}");
                                        if (!underwear.UndershirtIDs.ContainsKey(topType))
                                        {
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);
                                            nInventory.UnActiveItem(player, ItemType.Undershit);
                                        }
                                        else
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(underwear.UndershirtIDs[topType], underwearTexture);
                                        Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(variation, texture);
                                    }
                                }
                                else
                                {
                                    //var underwearID = 0;
                                    var underwear = Customization.Underwears[gender].Values.FirstOrDefault(u => u.Top == Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation);
                                    var underwearTexture = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Texture;
                                    if (underwear != null)
                                    {
                                        var topType = Customization.Tops[gender].FirstOrDefault(t => t.Variation == variation).Type;
                                        if (!underwear.UndershirtIDs.ContainsKey(topType))
                                        {
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(Customization.EmtptySlots[gender][8], 0);
                                            nInventory.UnActiveItem(player, ItemType.Undershit);
                                        }
                                        else
                                            Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit = new ComponentItem(underwear.UndershirtIDs[topType], underwearTexture);
                                    }
                                    Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top = new ComponentItem(variation, texture);
                                }

                                nInventory.UnActiveItem(player, item.Type);
                                nInventory.Items[UUID][index].IsActive = true;
                                InvInterface.Update(player, item, index);
                                Main.PlayAnimation(player, "mp_character_creation@customise@male_a", "drop_clothes_a", 1, 5500);
                            }

                            var gloves = Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation;
                            if (gloves != 0 &&
                                !Customization.CorrectGloves[gender][gloves].ContainsKey(Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation]))
                            {
                                nInventory.UnActiveItem(player, ItemType.Gloves);
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves = new ComponentItem(0, 0);
                            }

                            player.SetClothes(8, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Undershit.Texture);
                            player.SetClothes(11, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation, Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Texture);
                            var noneGloves = Customization.CorrectTorso[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Top.Variation];
                            if (Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation == 0)
                            {
                                Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Torso = new ComponentItem(noneGloves, 0);
                                player.SetClothes(3, noneGloves, 0);
                            }
                            else
                                player.SetClothes(3, Customization.CorrectGloves[gender][Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Variation][noneGloves], Customization.CustomPlayerData[Main.Players[player].UUID].Clothes.Gloves.Texture);
                            return;
                        }
                    #endregion

                    #region Food
                    case ItemType.Beer:
                        EatManager.AddWater(player, 12);
                        EatManager.AddEat(player, 2);
                        Commands.RPChat("me", player, $"выпил(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Beer).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_intdrink", "loop");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Burger:
                        EatManager.AddWater(player, -15);
                        EatManager.AddEat(player, 35);
                        BattlePass.AddProgressToQuest(player, 19, 1);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Burger).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.eCola:
                        EatManager.AddWater(player, 35);
                        EatManager.AddEat(player, 2);
                        Commands.RPChat("me", player, $"выпил(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.eCola).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_intdrink", "loop");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.HotDog:
                        EatManager.AddWater(player, -10);
                        EatManager.AddEat(player, 20);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.HotDog).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Pizza:
                        EatManager.AddWater(player, -10);
                        EatManager.AddEat(player, 65);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Pizza).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Sandwich:
                        EatManager.AddWater(player, -5);
                        EatManager.AddEat(player, 15);
                        BattlePass.AddProgressToQuest(player, 6, 1);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Sandwich).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Sprunk:
                        EatManager.AddWater(player, 30);
                        EatManager.AddEat(player, 2);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"выпил(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Sprunk).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_intdrink", "loop");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Сrisps:
                        EatManager.AddWater(player, -20);
                        EatManager.AddEat(player, 35);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Сrisps).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Apple:
                        EatManager.AddWater(player, 2);
                        EatManager.AddEat(player, 15);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Apple).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Banana:
                        EatManager.AddWater(player, -10);
                        EatManager.AddEat(player, 20);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Banana).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Dount:
                        EatManager.AddEat(player, 15);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Dount).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Salad:
                        EatManager.AddEat(player, 75);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Salad).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Nuggets:
                        EatManager.AddEat(player, 10);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"съел(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Nuggets).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.ChillAquaWater:
                        EatManager.AddWater(player, 40);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"выпил(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.ChillAquaWater).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_intdrink", "loop");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.ChillAquaBigWater:
                        EatManager.AddWater(player, 80);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"выпил(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.ChillAquaBigWater).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_intdrink", "loop");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.ChillAquaGaz:
                        EatManager.AddWater(player, 50);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"выпил(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.ChillAquaGaz).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_intdrink", "loop");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.PowerEngineer:
                        EatManager.AddWater(player, 45);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"выпил(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.PowerEngineer).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_intdrink", "loop");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    case ItemType.Cocktail:
                        EatManager.AddWater(player, 30);
                        EatManager.AddEat(player, 20);
                        if (player.GetData<int>("RESIST_TIME") < 600) Trigger.PlayerEvent(player, "stopScreenEffect", "PPFilter");
                        Commands.RPChat("me", player, $"выпил(а) {nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Cocktail).Name}");
                        if (!player.IsInVehicle)
                        {
                            NAPI.Player.PlayPlayerAnimation(player, 49, "mp_player_intdrink", "loop");
                            NAPI.Task.Run(() =>
                            {
                                NAPI.Player.StopPlayerAnimation(player);
                            }, delayTime: 1500);
                        }
                        break;
                    #endregion

                    #region Drugs
                    case ItemType.Drugs:
                        if (!player.HasData("USE_DRUGS") || DateTime.Now > player.GetData<DateTime>("USE_DRUGS"))
                        {
                            player.Health = (player.Health + 50 > 100) ? 100 : player.Health + 50;
                            Trigger.PlayerEvent(player, "startScreenEffect", "DrugsTrevorClownsFight", 300000, false);
                            Commands.RPChat("me", player, $"закурил(а) косяк");
                            player.SetData("USE_DRUGS", DateTime.Now.AddMinutes(3));
                            DrugAddiction.AddDrug(player, 7);
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Попробуйте использовать позже", 3000);
                            return;
                        }
                        break;
                    case ItemType.Kokos:
                        {
                            int fracid = Main.Players[player].FractionID;
                            if (fracid != 10 && fracid != 13)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Вы не умеете перерабатывать кокс", 3000);
                                return;
                            }
                            var mItem = nInventory.Find(Main.Players[player].UUID, ItemType.Kokos);
                            var count = (mItem == null) ? 0 : mItem.Count;
                            if (count < 5)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "У Вас недостаточно листьев кокса, нужно 5", 3000);
                                return;
                            }
                            var tryAdd = nInventory.TryAdd(player, new nItem(ItemType.Drugs, 1));
                            if (tryAdd == -1 || tryAdd > 0)
                            {
                                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно места в инвентаре", 3000);
                                return;
                            }
                            nInventory.Remove(player, ItemType.Kokos, 5);
                            nInventory.Add(player, new nItem(ItemType.Drugs, 1));
                            Notify.Send(player, NotifyType.Info, NotifyPosition.BottomCenter, $"Вы переработали 5 листьев в 1 наркотик", 3000);
                            return;
                        }
                    #endregion

                    #region Tools
                    case ItemType.PickAxe2:
                    case ItemType.PickAxe3:
                    case ItemType.PickAxe:
                        if (player.HasSharedData("PickAxe.InHands") && player.GetSharedData<bool>("PickAxe.InHands"))
                        {
                            BasicSync.DetachObject(player);
                            player.SetSharedData("PickAxe.InHands", false);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                            Commands.RPChat("me", player, $"убрал(а) {nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Name}");
                        }
                        else
                        {
                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_tool_pickaxe"), 57005, new Vector3(0.15, 0.05, 0.0), new Vector3(-65.0, 0.0, 0.0));
                            player.SetSharedData("PickAxe.InHands", true);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
                            Commands.RPChat("me", player, $"достал(а) {nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Name}");
                        }
                        return;
                    case ItemType.Axe:
                        if (player.HasSharedData("Axe.InHands") && player.GetSharedData<bool>("Axe.InHands"))
                        {
                            BasicSync.DetachObject(player);
                            player.SetSharedData("Axe.InHands", false);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                            Commands.RPChat("me", player, $"убрал(а) {nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Name}");
                        }
                        else
                        {
                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("w_me_hatchet"), 57005, new Vector3(0.1, -0.1, -0.02), new Vector3(-85, 0, 0));
                            player.SetSharedData("Axe.InHands", true);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
                            Commands.RPChat("me", player, $"достал(а) {nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Name}");
                        }
                        return;
                    case ItemType.Lockpick:
                        if (player.GetData<int>("INTERACTIONCHECK") != 3)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Невозможно использовать в данный момент", 3000);
                            InvInterface.Close(player);
                            return;
                        }
                        return;
                    case ItemType.ArmyLockpick:
                        if (!player.IsInVehicle || player.Vehicle.DisplayName != "BARRACKS")
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны находиться в военном перевозчике материалов", 3000);
                            return;
                        }
                        if (VehicleStreaming.GetEngineState(player.Vehicle))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Машину уже заведена", 3000);
                            return;
                        }
                        var lucky = new Random().Next(0, 5);
                        Log.Debug(lucky.ToString());
                        if (lucky == 5)
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"У Вас не получилось завести машину. Попробуйте ещё раз", 3000);
                        else
                        {
                            VehicleStreaming.SetEngineState(player.Vehicle, true);
                            Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"У Вас получилось завести машину", 3000);
                        }
                        break;
                    case ItemType.Bong:
                        if (!player.HasData("USE_DRUGS") || DateTime.Now > player.GetData<DateTime>("USE_DRUGS"))
                        {
                            player.Health = (player.Health + 10 > 100) ? 100 : player.Health + 10;
                            Main.OnAntiAnim(player);
                            player.PlayAnimation("anim@safehouse@bong", "bong_stage1", 31);
                            Commands.RPChat("me", player, $"закурил(а) кальян");
                            player.SetData("USE_DRUGS", DateTime.Now.AddMinutes(3));
                            BasicSync.AttachObjectToPlayer(player, nInventory.InventoryItems.Find(x => x.ItemType == ItemType.Bong).Model, 57005, new Vector3(0.25, 0, -0.03), new Vector3(-40, -70, 15));
                            DrugAddiction.AddDrug(player, 10);
                            NAPI.Task.Run(() => {
                                try
                                {
                                    if (player != null)
                                    {
                                        if (!player.IsInVehicle) player.StopAnimation();
                                        else player.SetData("ToResetAnimPhone", true);
                                        Main.OffAntiAnim(player);
                                        BasicSync.DetachObject(player);
                                        Trigger.PlayerEvent(player, "startScreenEffect", "DrugsTrevorClownsFight", 300000, false);
                                    }
                                }
                                catch { }
                            }, 5000);
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Попробуйте использовать позже", 3000);
                            return;
                        }
                        break;
                    case ItemType.Rod:
                        RodManager.useInventory(player, item, 1);
                        return;
                    case ItemType.RodUpgrade:
                        RodManager.useInventory(player, item, 3);
                        return;
                    case ItemType.KeyRing:
                        List<nItem> items = new List<nItem>();
                        string data = item.Data;
                        List<string> keys = (data.Length == 0) ? new List<string>() : new List<string>(data.Split('/'));
                        if (keys.Count > 0 && string.IsNullOrEmpty(keys[keys.Count - 1]))
                            keys.RemoveAt(keys.Count - 1);

                        foreach (var key in keys)
                            items.Add(new nItem(ItemType.CarKey, 1, key));
                        player.SetData("KEYRING", nInventory.Items[Main.Players[player].UUID].IndexOf(item));
                        InvInterface.OpenOut(player, items, "Связка ключей", 7);
                        return;
                    #endregion

                    #region Other Items
                    case ItemType.Note:
                        InvInterface.Close(player);
                        Trigger.PlayerEvent(player, "client::postal:opencard", "", "", "", 1, false);
                        return;
                    case ItemType.DoneNote:
                        InvInterface.Close(player);
                        var DataNote = item.Data;
                        Trigger.PlayerEvent(player, "client::postal:opencard", DataNote[0], DataNote[1], DataNote[2], DataNote[3], true);
                        return;
                    case ItemType.Boombox:
                        if (player.HasSharedData("BOOMBOXON") && !player.GetSharedData<bool>("BOOMBOXON"))
                        {
                            XMR.CMD_Boombox(player);
                        }
                        return;
                    case ItemType.Guitar:
                        if (player.IsInVehicle) return;
                        if (player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") == item.Type)
                        {
                            Main.StopSyncAnimation(player);
                            BasicSync.DetachObject(player);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            item.IsActive = false;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                        }
                        else
                        {
                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_acc_guitar_01"), 24818, new Vector3(-0.1, 0.31, 0.1), new Vector3(10, -20, 150));
                            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                            item.IsActive = true;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
                            Main.PlaySyncAnimation(player, "amb@world_human_musician@guitar@male@base", "base", 49);
                        }
                        return;
                    case ItemType.Umbrella:
                        if (player.IsInVehicle) return;
                        if (player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") == item.Type)
                        {
                            Main.StopSyncAnimation(player);
                            BasicSync.DetachObject(player);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            item.IsActive = false;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                        }
                        else
                        {
                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("p_amb_brolly_01"), 57005, new Vector3(0.09479946, 0.013351775, -0.020646578), new Vector3(-76.90267, 5.92244, -32.74062));
                            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                            item.IsActive = true;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
                            Main.PlaySyncAnimation(player, "anim@heists@humane_labs@finale@keycards", "ped_b_enter_loop", 49);
                        }
                        return;  
                    case ItemType.Micophone:
                        if (player.IsInVehicle) return;
                        if (player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") == item.Type)
                        {
                            Main.StopSyncAnimation(player);
                            BasicSync.DetachObject(player);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            item.IsActive = false;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                        }
                        else
                        {
                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("p_ing_microphonel_01"), 57005, new Vector3(0.13055836, 0.07557731, -0.0057103653), new Vector3(-83.314026, 7.7800093, -24.884037));
                            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                            item.IsActive = true;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
                            Main.PlaySyncAnimation(player, "anim@heists@humane_labs@finale@keycards", "ped_b_enter_loop", 49);
                        }
                        return;
                    case ItemType.Rose:
                        if (player.IsInVehicle) return;
                        if (player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") == item.Type)
                        {
                            Main.StopSyncAnimation(player);
                            BasicSync.DetachObject(player);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            item.IsActive = false;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                        }
                        else
                        {
                            BasicSync.AttachObjectToPlayer(player, nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Model, 57005, new Vector3(0.13055836, 0.07557731, -0.0057103653), new Vector3(-83.314026, 7.7800093, -24.884037));
                            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                            item.IsActive = true;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
                            Main.PlaySyncAnimation(player, "anim@heists@humane_labs@finale@keycards", "ped_b_enter_loop", 49);
                        }
                        return;
                    case ItemType.Camera:
                        if (player.IsInVehicle) return;
                        if (player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") == item.Type)
                        {
                            Main.StopSyncAnimation(player);
                            BasicSync.DetachObject(player);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            item.IsActive = false;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                        }
                        else
                        {
                            BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_v_cam_01"), 28422, new Vector3(-0.01, -0.25, 0.01), new Vector3(0, 0, 101));
                            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
                            item.IsActive = true;
                            Main.PlaySyncAnimation(player, "amb@world_human_mobile_film_shocking@female@base", "base", 49);
                        }
                        return;
                    case ItemType.PDildo:
                    case ItemType.BDildo:
                    case ItemType.RDildo:
                        if (player.IsInVehicle) return;
                        if (player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") == item.Type)
                        {
                            BasicSync.DetachObject(player);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            item.IsActive = false;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                        }
                        else
                        {
                            BasicSync.AttachObjectToPlayer(player, nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Model, 57005, new Vector3(0.15, 0.09, 0), new Vector3(-70, 0, 0));
                            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                            item.IsActive = true;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
                        }
                        return;  
                    case ItemType.GasCan:
                        if (player.IsInVehicle) return;
                        if (player.HasSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE") && player.GetSharedData<ItemType>("INVENTORY_ITEMINHANDS_ITEMTYPE") == item.Type)
                        {
                            BasicSync.DetachObject(player);
                            player.SetSharedData("INVENTORY_ITEMINHANDS", false);
                            item.IsActive = false;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", ItemType.Debug);
                        }
                        else
                        {
                            BasicSync.AttachObjectToPlayer(player, nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Model, 57005, new Vector3(0.55, -0.1, 0), new Vector3(-90, -10, 80));
                            player.SetSharedData("INVENTORY_ITEMINHANDS", true);
                            item.IsActive = true;
                            player.SetSharedData("INVENTORY_ITEMINHANDS_ITEMTYPE", item.Type);
                        }
                        return;
                    case ItemType.PopIt:
                        if (!item.IsActive)
                        {
                            InvInterface.Close(player);
                            if (!player.IsInVehicle)
                            {
                                player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_text_read_base", 49); // Анимация взгляда в телефон для персонажа
                                BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_popit"), 6286, new Vector3(0.11, 0.03, -0.01), new Vector3(0, -15, 120));
                            }
                            Trigger.PlayerEvent(player, "client::openpopitmenu");
                        }
                        return;
                    case ItemType.IDCard:
                        InvInterface.Close(player);
                        var acc = Main.Players[player];
                        string work = "Безработный";
                        List<object> dataIDCard = new List<object>
                            {
                                acc.UUID,
                                acc.FirstName,
                                acc.LastName,
                                acc.LVL,
                                acc.Gender,
                                acc.FractionID > 0 ? Fractions.Manager.FractionNames[acc.FractionID] : "Нет",
                                work
                            };
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(dataIDCard);
                        Trigger.PlayerEvent(player, "passport", json);
                        Trigger.PlayerEvent(player, "newPassport", player, acc.UUID);
                        return;
                    #endregion

                    #region Fractions Items
                    case ItemType.WalkieTalkie:
                        if (Main.Players[player].FractionID != 7)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не имеете доступа", 3000);
                            return;
                        }
                        if (player.GetData<bool>("Walkie_open") == true)
                        {
                            Utilis.Walkie.CloseWalkie(player);
                            InvInterface.Close(player);
                            player.ResetData("Walkie_open");
                            player.SetData<bool>("Walkie_open", false);
                            return;
                        }
                        else
                        {
                            Utilis.Walkie.OpenWalkie(player);
                            InvInterface.Close(player);
                            player.ResetData("Walkie_open");
                            player.SetData<bool>("Walkie_open", true);
                            return;
                        }
                    case ItemType.LSPDDrone:
                        if (Main.Players[player].FractionID != 7 && Main.Players[player].FractionID != 9)
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы не сотрудник!", 3000);
                            return;
                        }
                        if (!NAPI.Data.GetEntityData(player, "ON_DUTY"))
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Вы должны начать рабочий день", 3000);
                            return;
                        }

                        Trigger.PlayerEvent(player, "client:StartLSPDDrone");
                        break;
                    case ItemType.Balon:
                        if (Main.Players[player].FractionID == 1 || Main.Players[player].FractionID == 2 || Main.Players[player].FractionID == 3 || Main.Players[player].FractionID == 4 || Main.Players[player].FractionID == 5)
                        {
                            if (GraffitiWar.isWar)
                            {
                                if (player.HasData("graffiti"))
                                {
                                    player.PlayAnimation("anim@amb@business@weed@weed_inspecting_lo_med_hi@", "weed_spraybottle_stand_spraying_02_inspectorfemale", 1);
                                    Core.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_cs_spray_can"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));
                                    Main.OnAntiAnim(player);
                                    NAPI.Task.Run(() => { try { Main.OffAntiAnim(player); player.StopAnimation(); Core.BasicSync.DetachObject(player); Notify.Succ(player, "Вы успешно перекрасили граффити.", 3000); player.GetData<Graffiti>("graffiti").SetGang(Main.Players[player].FractionID); } catch { } }, 9000);
                                    nInventory.Remove(player, new nItem(ItemType.Balon, 1));

                                }
                                else
                                {
                                    Notify.Error(player, "Рядом нет граффити.", 3000);
                                    return;
                                }
                            }
                            else
                            {
                                Notify.Error(player, "Война за граффити еще не началась.", 3000);
                                return;
                            }
                        }
                        else
                        {
                            Notify.Error(player, "Вы не состоите в банде.", 3000);
                            return;
                        }
                        return;
                    #endregion

                    #region Health
                    case ItemType.HealthKit:
                        if (!player.HasData("USE_MEDKIT") || DateTime.Now > player.GetData<DateTime>("USE_MEDKIT"))
                        {
                            player.Health = 100;
                            player.SetData("USE_MEDKIT", DateTime.Now.AddMinutes(5));
                            Commands.RPChat("me", player, $"использовал(а) аптечку");
                            BattlePass.AddProgressToQuest(player, 10, 1);
                            if (!player.IsInVehicle)
                            {
                                Main.OnAntiAnim(player);
                                player.PlayAnimation("amb@code_human_wander_texting_fat@female@enter", "enter", 49);
                            }
                            NAPI.Task.Run(() => { try { if (player != null) { if (!player.IsInVehicle) { player.StopAnimation(); Main.OffAntiAnim(player); Main.OffAntiAnim(player); } } } catch { } }, 1000);
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Попробуйте использовать позже", 3000);
                            return;
                        }
                        break;
                    case ItemType.Bint:
                        if (!player.HasData("USE_MEDKIT") || DateTime.Now > player.GetData<DateTime>("USE_MEDKIT"))
                        {

                            if (player.Health > 99)
                            {
                                Notify.Error(player, "Вы не нуждаетесь в лечение бинтом");
                                return;
                            }
                            if (player.Health > 70)
                            {
                                Notify.Error(player, "Вы не можете использовать бинт");
                                return;
                            }
                            else
                            {
                                if (player.Health > 45)
                                    player.Health += 25 - player.Health;
                                else
                                    player.Health += 25;
                            }
                            player.SetData("USE_MEDKIT", DateTime.Now.AddSeconds(10));
                            Main.OnAntiAnim(player);
                            Main.PlayAnimation(player, "amb@code_human_wander_texting_fat@female@enter", "enter", 49, 4000);
                            Commands.RPChat("me", player, $"использовал(а) бинт");
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Попробуйте использовать позже", 3000);
                            return;
                        }
                        break;
                    case ItemType.Aptechka:
                        if (!player.HasData("USE_MEDKIT") || DateTime.Now > player.GetData<DateTime>("USE_MEDKIT"))
                        {

                            if (player.Health > 99)
                            {
                                Notify.Error(player, "Вы не нуждаетесь в лечение аптечкой");
                                return;
                            }
                            if (player.Health > 30)
                            {
                                var healthco = 100 - player.Health;
                                player.Health += healthco;
                            }
                            else
                            {
                                player.Health += 70;
                            }
                            player.SetData("USE_MEDKIT", DateTime.Now.AddSeconds(10));
                            Main.OnAntiAnim(player);
                            Main.PlayAnimation(player, "amb@code_human_wander_texting_fat@female@enter", "enter", 49, 4000);
                            Commands.RPChat("me", player, $"использовал(а) аптечку");
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Попробуйте использовать позже", 3000);
                            return;
                        }
                        break;
                    case ItemType.Tabletka:
                        if (!player.HasData("USE_MEDKIT") || DateTime.Now > player.GetData<DateTime>("USE_MEDKIT"))
                        {

                            if (player.Health > 99)
                            {
                                Notify.Error(player, "Вы не нуждаетесь в лечение таблеткой");
                                return;
                            }
                            if (player.Health > 90)
                            {
                                var healthco = 100 - player.Health;
                                player.Health += healthco;
                            }
                            else
                            {
                                player.Health += 10;
                            }
                            player.SetData("USE_MEDKIT", DateTime.Now.AddSeconds(10));
                            Main.OnAntiAnim(player);
                            Main.PlayAnimation(player, "amb@code_human_wander_texting_fat@female@enter", "enter", 49, 4000);
                            Commands.RPChat("me", player, $"использовал(а) таблетку");
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Попробуйте использовать позже", 3000);
                            return;
                        }
                        break;
                    case ItemType.Adrenalin:
                        if (!player.HasData("USE_MEDKIT") || DateTime.Now > player.GetData<DateTime>("USE_MEDKIT"))
                        {

                            if (player.Health > 99)
                            {
                                Notify.Error(player, "Вы не нуждаетесь в лечение шприцом с адреналином");
                                return;
                            }
                            if (player.Health > 80)
                            {
                                Notify.Error(player, "Вы не можете использовать шприц");
                                return;
                            }
                            else
                            {
                                player.Health += 70 - player.Health;
                            }
                            player.SetData("USE_MEDKIT", DateTime.Now.AddSeconds(10));
                            Main.OnAntiAnim(player);
                            Main.PlayAnimation(player, "amb@code_human_wander_texting_fat@female@enter", "enter", 49, 4000);
                            Commands.RPChat("me", player, $"использовал(а) шприц адреналина");
                        }
                        else
                        {
                            Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Попробуйте использовать позже", 3000);
                            return;
                        }
                        break;
                    #endregion

                    #region Default
                    default:
                        //BagWithDrill
                        //BagWithMoney
                        //Pocket
                        //NumberPlate
                        //Cuffs
                        //CuprumOre
                        //IronOre
                        //GoldOre
                        //SilverOre
                        //WoodPile
                        //Repair
                        return;
                        #endregion
                }
                nInventory.Remove(player, item.Type, 1);
                GameLog.Items($"player({Main.Players[player].UUID})", "use", Convert.ToInt32(item.Type), 1, $"{item.Data}");
            }
            catch (Exception e)
            {
                Log.Write($"EXCEPTION AT\"ITEM_USE\"/{item.Type}/{index}/{player.Name}/:\n" + e.ToString(), nLog.Type.Error);
            }
        }
        [RemoteEvent("server::inventory::popit:close")]
        public static void SERVER_CLOSEPOPIMENU(Player player)
        {
            if (!player.IsInVehicle)
            {
                player.StopAnimation();
                BasicSync.DetachObject(player);
            }
        }
        public static void onDrop(Player player, nItem item, dynamic data, bool remove = false)
        {
            try
            {
                var rnd = new Random();
                if (data != null && (int)data != 1)
                    Commands.RPChat("me", player, $"выбросил(а) {nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Name}");

                GameLog.Items($"player({Main.Players[player].UUID})", "ground", Convert.ToInt32(item.Type), 1, $"{item.Data}");

                if (!nInventory.ClothesItems.Contains(item.Type) && !nInventory.WeaponsItems.Contains(item.Type) && item.Type != ItemType.CarKey && item.Type != ItemType.KeyRing && !nInventory.IgnoreItems.Contains(item.Type))
                {
                    foreach (var o in NAPI.Pools.GetAllObjects())
                    {
                        if (player.Position.DistanceTo(o.Position) > 2) continue;
                        if (!o.HasSharedData("TYPE") || o.GetSharedData<string>("TYPE") != "DROPPED" || !o.HasData("ITEM")) continue;
                        nItem oItem = o.GetData<nItem>("ITEM");
                        if (oItem.Type == item.Type)
                        {
                            oItem.Count += item.Count;
                            o.SetData("ITEM", oItem);
                            o.SetSharedData("ITEM", oItem);
                            o.SetData("WILL_DELETE", DateTime.Now.AddMinutes(2));
                            o.SetSharedData("TYPE", "DROPPED");
                            InvInterface.sendItems(player);
                            return;
                        }
                    }
                }
                item.IsActive = false;
                item.FastSlots = -1;

                var xrnd = rnd.NextDouble();                                                                                                                                                                                        
                var yrnd = rnd.NextDouble();
                var obj = NAPI.Object.CreateObject(nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Model, player.Position + nInventory.InventoryItems.Find(x => x.ItemType == item.Type).ItemsPosOffset + new Vector3(xrnd, yrnd, 0), player.Rotation + nInventory.InventoryItems.Find(x => x.ItemType == item.Type).ItemsRotOffset, 255, player.Dimension);
                obj.SetSharedData("TYPE", "DROPPED");
                obj.SetSharedData("PICKEDT", false);
                obj.SetData("ITEM", item);
                obj.SetSharedData("ITEM", item);
                var id = rnd.Next(100000, 999999);
                while (ItemsDropped.Contains(id)) id = rnd.Next(100000, 999999);
                obj.SetData("ID", id);
                obj.SetData("DELETETIMER", Timers.StartOnce(1800000, () => deleteObject(obj, item)));
                if (remove)
                    nInventory.Remove(player, item);
                InvInterface.sendItems(player);
            }
            catch (Exception e) { Log.Write("onDrop: " + e.Message, nLog.Type.Error); }
        }
    }
}

using GTANetworkAPI;
using NeptuneEVO.SDK;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace NeptuneEVO.Core
{
    class nInventory : Script
    {
        public static Dictionary<int, List<nItem>> Items = new Dictionary<int, List<nItem>>();
        private static nLog Log = new nLog("nInventory");
        private static Timer SaveTimer;

        #region Constructor Item
        public class InventoryItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int Weight { get; set; }
            public uint Model { get; set; }
            public Vector3 ItemsPosOffset { get; set; }
            public Vector3 ItemsRotOffset { get; set; }
            public ItemType ItemType { get; set; }
            public int Stacks { get; set; }

            public InventoryItem(int id, string name, int weight, uint model, Vector3 possofset, Vector3 rotoffset, ItemType type, int stacks = 1)
            {
                ID = id;
                Name = name;
                Weight = weight;
                Model = model;
                ItemsPosOffset = possofset;
                ItemsRotOffset = rotoffset;
                ItemType = type;
                Stacks = stacks;
            }
        }
        #endregion

        #region Items
        public static List<InventoryItem> InventoryItems = new List<InventoryItem>()
        {
            new InventoryItem(-1, "Маска", 200, NAPI.Util.GetHashKey("q_box_m"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Mask),
            new InventoryItem(-3, "Перчатки", 200, NAPI.Util.GetHashKey("prop_gloves"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Gloves),
            new InventoryItem(-4, "Низ", 200, NAPI.Util.GetHashKey("q_box_pt"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Leg),
            new InventoryItem(-5, "Рюкзак", 200, 1234788901, new Vector3(0, 0, -0.85), new Vector3(0,0,0), ItemType.Bag),
            new InventoryItem(-6, "Обувь", 200, NAPI.Util.GetHashKey("q_box_sh"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Feet),
            new InventoryItem(-7, "Аксессуар", 200, NAPI.Util.GetHashKey("prop_earr"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Jewelry),
            new InventoryItem(-8, "Майка", 200, NAPI.Util.GetHashKey("q_box_cl"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Undershit),
            new InventoryItem(-9, "Бронежилет", 200, 701173564, new Vector3(0, 0, -0.88), new Vector3(0,0,0), ItemType.BodyArmor),
            new InventoryItem(-10, "Украшения", 200, 3887136870, new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Unknown),
            new InventoryItem(-11, "Верхняя одежда", 200, NAPI.Util.GetHashKey("q_box_cur"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Top),
            new InventoryItem(-12, "Головной убор", 200, NAPI.Util.GetHashKey("q_box_hat"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Hat),
            new InventoryItem(-13, "Очки", 200, NAPI.Util.GetHashKey("prop_glass"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Glasses),
            new InventoryItem(-14, "Аксессуар", 200, NAPI.Util.GetHashKey("prop_acs"), new Vector3(0, 0, -0.99), new Vector3(0,0,0), ItemType.Accessories),

            new InventoryItem(1, "Эпинефрин", 200, 678958360, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.HealthKit, 5),
            new InventoryItem(2, "Канистра с бензином", 1000, NAPI.Util.GetHashKey("prop_jerrycan_01a"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.GasCan),
            new InventoryItem(3, "Чипсы", 100, 2564432314, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Сrisps, 5),
            new InventoryItem(4, "Пиво", 100, NAPI.Util.GetHashKey("prop_amb_beer_bottle"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Beer, 5),
            new InventoryItem(5, "Пицца", 100, 604847691, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Pizza, 5),
            new InventoryItem(6, "Бургер", 100, NAPI.Util.GetHashKey("prop_food_bs_burg3"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Burger, 5),
            new InventoryItem(7, "Хот-Дог", 100, 2565741261, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.HotDog, 5),
            new InventoryItem(8, "Шаурма", 100, 987331897, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Sandwich, 5),
            new InventoryItem(9, "eCola", 100, 144995201, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.eCola, 5),
            new InventoryItem(10, "Sprunk", 100, 2973713592, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Sprunk, 5),

            new InventoryItem(11, "Отмычка для замков", 100 , 977923025, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Lockpick),
            new InventoryItem(12, "Сумка с деньгами", 2000, 977923025, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.BagWithMoney),
            new InventoryItem(13, "Материалы", 10, 3045218749, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Material, 5000),
            new InventoryItem(14, "Пакетик Green", 200, NAPI.Util.GetHashKey("prop_meth_bag"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Drugs),
            new InventoryItem(15, "Сумка с дрелью", 3800, 3887136870, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.BagWithDrill),
            new InventoryItem(16, "Военная отмычка", 500, 3887136870, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.ArmyLockpick),
            new InventoryItem(17, "Мешок", 100, 3887136870, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Pocket, 3),
            new InventoryItem(18, "Стяжки", 100, 3887136870, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Cuffs, 3),
            new InventoryItem(19, "Ключи от машины", 100, 977923025, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.CarKey),
            new InventoryItem(40, "Подарок", 500, NAPI.Util.GetHashKey("prop_cs_clothes_box"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Present, 15),
            new InventoryItem(41, "Связка ключей", 100, 977923025, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.KeyRing),
            new InventoryItem(42, "Балончик", 600, NAPI.Util.GetHashKey("prop_cs_spray_can"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Balon),

            new InventoryItem(43, "Апельсин", 100, NAPI.Util.GetHashKey("ng_proc_food_aple2a"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Orange, 75),
            new InventoryItem(44, "Яблоко", 100, NAPI.Util.GetHashKey("prop_donut_02"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Apple, 75),
            new InventoryItem(45, "Банан", 100, NAPI.Util.GetHashKey("prop_banan"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Banana, 75),
            new InventoryItem(46, "Пончик", 100, NAPI.Util.GetHashKey("prop_donut_02"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Dount, 25),
            new InventoryItem(47, "Салат Цезарь", 300, NAPI.Util.GetHashKey("v_prop_floatcandle"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Salad, 15),
            new InventoryItem(48, "Наггетсы", 100, NAPI.Util.GetHashKey("prop_food_cb_nugets"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Nuggets, 15),
            new InventoryItem(49, "Энергетик", 200, NAPI.Util.GetHashKey("prop_ld_flow_bottle"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.PowerEngineer, 5),
            new InventoryItem(50, "Вода ChillAqua 0.5л", 200, NAPI.Util.GetHashKey("ba_prop_club_water_bottle"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.ChillAquaWater, 5),
            new InventoryItem(51, "Вода ChillAqua 1л", 500, NAPI.Util.GetHashKey("vw_prop_casino_water_bottle_01a"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.ChillAquaBigWater, 5),
            new InventoryItem(52, "Газированная вода ChillAqua", 200, NAPI.Util.GetHashKey("prop_ld_flow_bottle"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.ChillAquaGaz, 5),
            new InventoryItem(53, "Коктейль", 100, NAPI.Util.GetHashKey("prop_cocktail"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Cocktail, 7),
            new InventoryItem(54, "Записка", 100, NAPI.Util.GetHashKey("hei_prop_hei_post_note_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Note, 15),
            new InventoryItem(55, "Красное дилдо", 700, NAPI.Util.GetHashKey("prop_reddildo"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.RDildo, 1),
            new InventoryItem(56, "Черное дилдо", 700, NAPI.Util.GetHashKey("prop_darkdildo"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.BDildo, 1),
            new InventoryItem(57, "Розовое дилдо", 700, NAPI.Util.GetHashKey("prop_pinkdildo"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.PDildo, 1),
            new InventoryItem(58, "Плюшевый мишка", 500, NAPI.Util.GetHashKey("v_ilev_mr_rasberryclean"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.BearToy, 1),
            new InventoryItem(59, "Гитара", 2500, NAPI.Util.GetHashKey("prop_acc_guitar_01"), new Vector3(0,0,-0.975), new Vector3(-90,0,0), ItemType.Guitar, 1),
            new InventoryItem(60, "Красная роза", 200, NAPI.Util.GetHashKey("prop_single_rose"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Rose, 3),
            new InventoryItem(61, "Вэйп", 800, NAPI.Util.GetHashKey("ba_prop_battle_vape_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Vape, 1),
            new InventoryItem(62, "Бонг", 1200, NAPI.Util.GetHashKey("prop_bong_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Bong, 1),
            new InventoryItem(63, "Бинокль", 1000, NAPI.Util.GetHashKey("prop_bong_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Binocular, 1),
            new InventoryItem(64, "Зонтик", 1000, NAPI.Util.GetHashKey("p_amb_brolly_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Umbrella, 1),
            new InventoryItem(65, "Камера", 2000, NAPI.Util.GetHashKey("prop_bong_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Camera, 1),
            new InventoryItem(66, "Микрофон", 1000, NAPI.Util.GetHashKey("p_ing_microphonel_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Micophone, 1),
            new InventoryItem(67, "Запечатанная записка", 100, NAPI.Util.GetHashKey("hei_prop_hei_post_note_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.DoneNote, 1),

            new InventoryItem(20, "Водка на корке лимона", 500, NAPI.Util.GetHashKey("prop_vodka_bottle"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.RusDrink1),
            new InventoryItem(21, "Водка на бруснике", 500, NAPI.Util.GetHashKey("prop_vodka_bottle"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.RusDrink2),
            new InventoryItem(22, "Русский стандарт", 500, NAPI.Util.GetHashKey("prop_vodka_bottle"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.RusDrink3),
            new InventoryItem(23, "Асахи", 500, NAPI.Util.GetHashKey("prop_cs_beer_bot_02"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.YakDrink1),
            new InventoryItem(24, "Cherenkov", 500, NAPI.Util.GetHashKey("prop_wine_red"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.YakDrink2),
            new InventoryItem(25, "Jack Janiel", 500, NAPI.Util.GetHashKey("p_whiskey_bottle_s"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.YakDrink3),
            new InventoryItem(26, "Champagne Bleuter'd", 500, NAPI.Util.GetHashKey("prop_wine_white"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.LcnDrink1),
            new InventoryItem(27, "Самбука", 500, NAPI.Util.GetHashKey("prop_vodka_bottle"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.LcnDrink2),
            new InventoryItem(28, "Кампари", 500, NAPI.Util.GetHashKey("prop_wine_red"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.LcnDrink3),
            new InventoryItem(29, "Дживан", 500, NAPI.Util.GetHashKey("prop_bottle_cognac"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.ArmDrink1),
            new InventoryItem(30, "Арарат", 500, NAPI.Util.GetHashKey("prop_bottle_cognac"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.ArmDrink2),
            new InventoryItem(31, "Ноян Тапан", 500, NAPI.Util.GetHashKey("prop_bottle_cognac"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.ArmDrink3),

            new InventoryItem(100, "Pistol", 1500, NAPI.Util.GetHashKey("w_pi_pistol"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Pistol),
            new InventoryItem(101, "Combat Pistol", 1500, NAPI.Util.GetHashKey("w_pi_combatpistol"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.CombatPistol),
            new InventoryItem(102, "Pistol 50", 1500, NAPI.Util.GetHashKey("w_pi_pistol50"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Pistol50),
            new InventoryItem(103, "SNS Pistol", 1500, NAPI.Util.GetHashKey("w_pi_sns_pistol"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SNSPistol),
            new InventoryItem(104, "Heavy Pistol", 1500, NAPI.Util.GetHashKey("w_pi_heavypistol"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.HeavyPistol),
            new InventoryItem(105, "Vintage Pistol", 1500, NAPI.Util.GetHashKey("w_pi_vintage_pistol"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.VintagePistol),
            new InventoryItem(106, "Marksman Pistol", 1500, NAPI.Util.GetHashKey("w_pi_singleshot"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.MarksmanPistol),
            new InventoryItem(107, "Revolver", 1500, NAPI.Util.GetHashKey("w_pi_revolver"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Revolver),
            new InventoryItem(108, "AP Pistol", 1500, NAPI.Util.GetHashKey("w_pi_appistol"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.APPistol),
            new InventoryItem(109, "Stun Gun", 1500, NAPI.Util.GetHashKey("w_pi_stungun"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.StunGun),
            new InventoryItem(110, "Flare Gun", 1500, NAPI.Util.GetHashKey("w_pi_flaregun"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.FlareGun),
            new InventoryItem(111, "Double Action", 1500, NAPI.Util.GetHashKey("mk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.DoubleAction),
            new InventoryItem(112, "Pistol Mk2", 1500, NAPI.Util.GetHashKey("w_pi_pistolmk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.PistolMk2),
            new InventoryItem(113, "SNSPistol Mk2", 1500, NAPI.Util.GetHashKey("w_pi_sns_pistolmk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SNSPistolMk2),
            new InventoryItem(114, "Revolver Mk2", 1500, NAPI.Util.GetHashKey("w_pi_revolvermk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.RevolverMk2),

            new InventoryItem(115, "Micro SMG", 2500, NAPI.Util.GetHashKey("w_sb_microsmg"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.MicroSMG),
            new InventoryItem(116, "Machine Pistol", 2500, NAPI.Util.GetHashKey("w_sb_compactsmg"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.MachinePistol),
            new InventoryItem(117, "SMG", 2500, NAPI.Util.GetHashKey("w_sb_smg"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SMG),
            new InventoryItem(118, "Assault SMG", 2500, NAPI.Util.GetHashKey("w_sb_assaultsmg"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.AssaultSMG),
            new InventoryItem(119, "Combat PDW", 2500, NAPI.Util.GetHashKey("w_sb_pdw"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.CombatPDW),
            new InventoryItem(120, "MG", 2500, NAPI.Util.GetHashKey("w_mg_mg"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.MG),
            new InventoryItem(121, "Combat MG", 2500, NAPI.Util.GetHashKey("w_mg_combatmg"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.CombatMG),
            new InventoryItem(122, "Gusenberg", 2500, NAPI.Util.GetHashKey("w_sb_gusenberg"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Gusenberg),
            new InventoryItem(123, "Mini SMG", 2500, NAPI.Util.GetHashKey("w_sb_minismg"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.MiniSMG),
            new InventoryItem(124, "SMG Mk2", 2500, NAPI.Util.GetHashKey("w_sb_smgmk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SMGMk2),
            new InventoryItem(125, "Combat MG Mk2", 2500, NAPI.Util.GetHashKey("w_mg_combatmgmk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.CombatMGMk2),

            new InventoryItem(126, "Assault Rifle", 3000, NAPI.Util.GetHashKey("w_ar_assaultrifle"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.AssaultRifle),
            new InventoryItem(127, "Carbine Rifle", 3000, NAPI.Util.GetHashKey("w_ar_carbinerifle"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.CarbineRifle),
            new InventoryItem(128, "Advanced Rifle", 3000, NAPI.Util.GetHashKey("w_ar_advancedrifle"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.AdvancedRifle),
            new InventoryItem(129, "Special Carbine", 3000, NAPI.Util.GetHashKey("w_ar_specialcarbine"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SpecialCarbine),
            new InventoryItem(130, "Bullpup Rifle", 3000, NAPI.Util.GetHashKey("w_ar_bullpuprifle"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.BullpupRifle),
            new InventoryItem(131, "Compact Rifle", 3000, NAPI.Util.GetHashKey("w_ar_assaultrifle_smg"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.CompactRifle),
            new InventoryItem(132, "Assault Rifle Mk2", 3000, NAPI.Util.GetHashKey("w_ar_assaultriflemk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.AssaultRifleMk2),
            new InventoryItem(133, "Carbine Rifle Mk2", 3000, NAPI.Util.GetHashKey("w_ar_carbineriflemk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.CarbineRifleMk2),
            new InventoryItem(134, "Special Carbine Mk2", 3000, NAPI.Util.GetHashKey("w_ar_specialcarbinemk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SpecialCarbineMk2),
            new InventoryItem(135, "Bullpup Rifle Mk2", 3000, NAPI.Util.GetHashKey("w_ar_bullpupriflemk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.BullpupRifleMk2),

            new InventoryItem(136, "Sniper Rifle", 4000, NAPI.Util.GetHashKey("w_sr_sniperrifle"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SniperRifle),
            new InventoryItem(137, "Heavy Sniper", 4000, NAPI.Util.GetHashKey("w_sr_heavysniper"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.HeavySniper),
            new InventoryItem(138, "Marksman Rifle", 4000, NAPI.Util.GetHashKey("w_sr_marksmanrifle"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.MarksmanRifle),
            new InventoryItem(139, "Heavy Sniper Mk2", 4000, NAPI.Util.GetHashKey("w_sr_heavysnipermk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.HeavySniperMk2),
            new InventoryItem(140, "Marksman Rifle Mk2", 4000, NAPI.Util.GetHashKey("w_sr_marksmanriflemk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.MarksmanRifleMk2),

            new InventoryItem(141, "Pump Shotgun", 2500, NAPI.Util.GetHashKey("w_sg_pumpshotgun"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.PumpShotgun),
            new InventoryItem(142, "SawnOff Shotgun", 2500, NAPI.Util.GetHashKey("w_sg_sawnoff"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SawnOffShotgun),
            new InventoryItem(143, "Bullpup Shotgun", 2500, NAPI.Util.GetHashKey("w_sg_bullpupshotgun"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.BullpupShotgun),
            new InventoryItem(144, "Assault Shotgun", 2500, NAPI.Util.GetHashKey("w_sg_assaultshotgun"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.AssaultShotgun),
            new InventoryItem(145, "Musket", 2500, NAPI.Util.GetHashKey("w_ar_musket"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Musket),
            new InventoryItem(146, "Heavy Shotgun", 2500, NAPI.Util.GetHashKey("w_sg_heavyshotgun"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.HeavyShotgun),
            new InventoryItem(147, "Double Barrel Shotgun", 2500, NAPI.Util.GetHashKey("w_sg_doublebarrel"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.DoubleBarrelShotgun),
            new InventoryItem(148, "Sweeper Shotgun", 2500, NAPI.Util.GetHashKey("mk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SweeperShotgun),
            new InventoryItem(149, "Pump Shotgun Mk2", 2500, NAPI.Util.GetHashKey("w_sg_pumpshotgunmk2"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.PumpShotgunMk2),

            new InventoryItem(180, "Нож", 600, NAPI.Util.GetHashKey("w_me_knife_01"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Knife),
            new InventoryItem(181, "Дубинка", 500, NAPI.Util.GetHashKey("w_me_nightstick"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Nightstick),
            new InventoryItem(182, "Молоток", 500, NAPI.Util.GetHashKey("w_me_hammer"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Hammer),
            new InventoryItem(183, "Бита", 500, NAPI.Util.GetHashKey("w_me_bat"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Bat),
            new InventoryItem(184, "Лом", 500, NAPI.Util.GetHashKey("w_me_crowbar"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Crowbar),
            new InventoryItem(185, "Гольф клюшка", 500, NAPI.Util.GetHashKey("w_me_gclub"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.GolfClub),
            new InventoryItem(186, "Бутылка", 200, NAPI.Util.GetHashKey("w_me_bottle"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Bottle),
            new InventoryItem(187, "Кинжал", 500, NAPI.Util.GetHashKey("w_me_dagger"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Dagger),
            new InventoryItem(188, "Топор", 500, NAPI.Util.GetHashKey("w_me_hatchet"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Hatchet),
            new InventoryItem(189, "Кастет", 500, NAPI.Util.GetHashKey("w_me_knuckle"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.KnuckleDuster),
            new InventoryItem(190, "Мачете", 500, NAPI.Util.GetHashKey("prop_ld_w_me_machette"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Machete),
            new InventoryItem(191, "Фонарик", 500, NAPI.Util.GetHashKey("w_me_flashlight"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Flashlight),
            new InventoryItem(192, "Швейцарский нож", 500, NAPI.Util.GetHashKey("w_me_switchblade"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.SwitchBlade),
            new InventoryItem(193, "Кий", 500, NAPI.Util.GetHashKey("prop_pool_cue"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.PoolCue),
            new InventoryItem(194, "Ключ", 500, NAPI.Util.GetHashKey("prop_cs_wrench"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.Wrench),
            new InventoryItem(195, "Боевой топор", 500, NAPI.Util.GetHashKey("w_me_battleaxe"), new Vector3(0,0,-0.99), new Vector3(90, 0, 0), ItemType.BattleAxe),

            new InventoryItem(200, "Пистолетный калибр", 1, NAPI.Util.GetHashKey("w_am_case"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.PistolAmmo, 500),
            new InventoryItem(201, "Малый калибр", 1, NAPI.Util.GetHashKey("w_am_case"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.SMGAmmo, 500),
            new InventoryItem(202, "Автоматный калибр", 1, NAPI.Util.GetHashKey("w_am_case"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.RiflesAmmo, 500),
            new InventoryItem(203, "Снайперский калибр", 1, NAPI.Util.GetHashKey("w_am_case"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.SniperAmmo, 500),
            new InventoryItem(204, "Дробь", 1, NAPI.Util.GetHashKey("w_am_case"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.ShotgunsAmmo, 500),

            new InventoryItem(205, "Удочка", 1000, NAPI.Util.GetHashKey("prop_fishing_rod_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Rod),
            new InventoryItem(206, "Улучшенная удочка", 1000, NAPI.Util.GetHashKey("prop_fishing_rod_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.RodUpgrade),
            new InventoryItem(207, "Улучшенная удочка 2", 1000, NAPI.Util.GetHashKey("prop_fishing_rod_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.RodMK2),
            new InventoryItem(208, "Банка с червями", 10, NAPI.Util.GetHashKey("ng_proc_paintcan02a"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Naz, 25),
            new InventoryItem(209, "Корюшка", 100, NAPI.Util.GetHashKey("ribka_tor1"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Koroska, 15),
            new InventoryItem(210, "Кунджа", 100, NAPI.Util.GetHashKey("ribka_tor1"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Kyndja, 15),
            new InventoryItem(211, "Лосось", 100, NAPI.Util.GetHashKey("ribka_tor1"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Lococ, 15),
            new InventoryItem(212, "Окунь", 100, NAPI.Util.GetHashKey("ribka_tor7"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Okyn, 15),
            new InventoryItem(213, "Осётр", 100, NAPI.Util.GetHashKey("ribka_tor1"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Ocetr, 15),
            new InventoryItem(214, "Скат", 100, NAPI.Util.GetHashKey("ribka_tor6"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Skat, 15),
            new InventoryItem(215, "Тунец", 100, NAPI.Util.GetHashKey("ribka_tor5"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Tunec, 15),
            new InventoryItem(216, "Угорь", 100, NAPI.Util.GetHashKey("ribka_tor1"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Ygol, 15),
            new InventoryItem(217, "Чёрный амур", 100, NAPI.Util.GetHashKey("ribka_tor1"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Amyr, 15),
            new InventoryItem(218, "Щука", 100, NAPI.Util.GetHashKey("ribka_tor4"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Chyka, 15),

            new InventoryItem(229, "Фишки", 1, NAPI.Util.GetHashKey("vw_prop_chip_10kdollar_st"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.CasinoChips, 1000000),
            new InventoryItem(250, "Рем. комплект", 500, NAPI.Util.GetHashKey("gr_prop_gr_tool_box_02a"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Repair, 3),

            new InventoryItem(251, "Бинт", 200, NAPI.Util.GetHashKey("bandage_shell"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Bint, 15),
            new InventoryItem(252, "Аптечки", 200, 678958360, new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Aptechka, 3),
            new InventoryItem(253, "Таблетки", 100, NAPI.Util.GetHashKey("prop_cs_pills"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Tabletka, 8),
            new InventoryItem(254, "Шприц адреналина", 200, NAPI.Util.GetHashKey("bandage_shell"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Adrenalin, 3),

            new InventoryItem(255, "Косяк", 100, NAPI.Util.GetHashKey("p_a4_sheets_s"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Kokos, 30),
            new InventoryItem(258, "Снежок", 10, NAPI.Util.GetHashKey("w_ex_snowball"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.SnowBall, 500),

            new InventoryItem(779, "Дрон", 1000, NAPI.Util.GetHashKey("ch_prop_casino_drone_02a"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.LSPDDrone),
            new InventoryItem(566, "Рация", 500, NAPI.Util.GetHashKey("prop_cs_walkie_talkie"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.WalkieTalkie),
            new InventoryItem(610, "Бумбокс", 1200, NAPI.Util.GetHashKey("prop_boombox_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Boombox),
            new InventoryItem(302, "Fidget Pop It", 500, NAPI.Util.GetHashKey("prop_popit"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.PopIt),
            new InventoryItem(303, "ID Карта", 300, NAPI.Util.GetHashKey("prop_cs_swipe_card"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.IDCard),
            new InventoryItem(304, "Автомобильный номер", 500, NAPI.Util.GetHashKey("p_num_plate_01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.NumberPlate),

            new InventoryItem(305, "Кирка", 1000, NAPI.Util.GetHashKey("prop_tool_pickaxe"), new Vector3(0,0,-0.99), new Vector3(90,0,0), ItemType.PickAxe),
            new InventoryItem(313, "Улучшенная кирка", 1000, NAPI.Util.GetHashKey("prop_tool_pickaxe"), new Vector3(0,0,-0.99), new Vector3(90,0,0), ItemType.PickAxe2),
            new InventoryItem(314, "Золотая кирка", 1000, NAPI.Util.GetHashKey("prop_tool_pickaxe"), new Vector3(0,0,-0.99), new Vector3(90,0,0), ItemType.PickAxe3),

            new InventoryItem(306, "Золотая руда", 100, NAPI.Util.GetHashKey("proc_mntn_stone01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.GoldOre),
            new InventoryItem(307, "Серебряная руда", 100, NAPI.Util.GetHashKey("proc_mntn_stone01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.SilverOre),
            new InventoryItem(308, "Железная руда", 100, NAPI.Util.GetHashKey("proc_mntn_stone01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.IronOre),
            new InventoryItem(309, "Медная руда", 100, NAPI.Util.GetHashKey("proc_mntn_stone01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.CuprumOre),
            new InventoryItem(310, "Камень", 100, NAPI.Util.GetHashKey("proc_mntn_stone01"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.Stone),
            new InventoryItem(311, "Древесина", 100, NAPI.Util.GetHashKey("mj_wood_pile_qnx"), new Vector3(0,0,-0.99), new Vector3(0,0,0), ItemType.WoodPile),
            new InventoryItem(312, "Топор", 100, NAPI.Util.GetHashKey("w_me_hatchet"), new Vector3(0,0,-0.975), new Vector3(90,0,0), ItemType.Axe),
        };
        #endregion

        #region Lists
        public static List<ItemType> ClothesItems = new List<ItemType>()
        {
            ItemType.Mask,
            ItemType.Gloves,
            ItemType.Leg,
            ItemType.Bag,
            ItemType.Feet,
            ItemType.Jewelry,
            ItemType.Undershit,
            ItemType.BodyArmor,
            ItemType.Unknown,
            ItemType.Top,
            ItemType.Hat,
            ItemType.Glasses,
            ItemType.Accessories,
        };
        public static List<ItemType> WeaponsItems = new List<ItemType>()
        {
            ItemType.Pistol,
            ItemType.CombatPistol,
            ItemType.Pistol50,
            ItemType.SNSPistol,
            ItemType.HeavyPistol,
            ItemType.VintagePistol,
            ItemType.MarksmanPistol,
            ItemType.Revolver,
            ItemType.APPistol,
            ItemType.FlareGun,
            ItemType.DoubleAction,
            ItemType.PistolMk2,
            ItemType.SNSPistolMk2,
            ItemType.RevolverMk2,

            ItemType.MicroSMG,
            ItemType.MachinePistol,
            ItemType.SMG,
            ItemType.AssaultSMG,
            ItemType.CombatPDW,
            ItemType.MG,
            ItemType.CombatMG,
            ItemType.Gusenberg,
            ItemType.MiniSMG,
            ItemType.SMGMk2,
            ItemType.CombatMGMk2,

            ItemType.AssaultRifle,
            ItemType.CarbineRifle,
            ItemType.AdvancedRifle,
            ItemType.SpecialCarbine,
            ItemType.BullpupRifle,
            ItemType.CompactRifle,
            ItemType.AssaultRifleMk2,
            ItemType.CarbineRifleMk2,
            ItemType.SpecialCarbineMk2,
            ItemType.BullpupRifleMk2,

            ItemType.SniperRifle,
            ItemType.HeavySniper,
            ItemType.MarksmanRifle,
            ItemType.HeavySniperMk2,
            ItemType.MarksmanRifleMk2,

            ItemType.PumpShotgun,
            ItemType.SawnOffShotgun,
            ItemType.BullpupShotgun,
            ItemType.AssaultShotgun,
            ItemType.Musket,
            ItemType.HeavyShotgun,
            ItemType.DoubleBarrelShotgun,
            ItemType.SweeperShotgun,
            ItemType.PumpShotgunMk2,

        };
        public static List<ItemType> MeleeWeaponsItems = new List<ItemType>()
        {
            ItemType.Knife,
            ItemType.Nightstick,
            ItemType.Hammer,
            ItemType.Bat,
            ItemType.Crowbar,
            ItemType.GolfClub,
            ItemType.Bottle,
            ItemType.Dagger,
            ItemType.Hatchet,
            ItemType.KnuckleDuster,
            ItemType.Machete,
            ItemType.Flashlight,
            ItemType.SwitchBlade,
            ItemType.PoolCue,
            ItemType.Wrench,
            ItemType.BattleAxe,
            ItemType.StunGun,

            ItemType.SnowBall,

        };
        public static List<ItemType> AmmoItems = new List<ItemType>()
        {
            ItemType.PistolAmmo,
            ItemType.RiflesAmmo,
            ItemType.ShotgunsAmmo,
            ItemType.SMGAmmo,
            ItemType.SniperAmmo
        };
        public static List<ItemType> AlcoItems = new List<ItemType>()
        {
            ItemType.LcnDrink1,
            ItemType.LcnDrink2,
            ItemType.LcnDrink3,
            ItemType.RusDrink1,
            ItemType.RusDrink2,
            ItemType.RusDrink3,
            ItemType.YakDrink1,
            ItemType.YakDrink2,
            ItemType.YakDrink3,
            ItemType.ArmDrink1,
            ItemType.ArmDrink2,
            ItemType.ArmDrink3,
        };
        public static List<ItemType> IgnoreItems = new List<ItemType>()
        {
            ItemType.IDCard,
            ItemType.NumberPlate,
            ItemType.PopIt,
            ItemType.Note,
            ItemType.DoneNote,
            ItemType.Camera,
            ItemType.Micophone,
            ItemType.Rose,
            ItemType.BDildo,
            ItemType.PDildo,
            ItemType.RDildo,
            ItemType.Vape,
            ItemType.Bong,
            ItemType.PickAxe,
            ItemType.PickAxe2,
            ItemType.PickAxe3,
            ItemType.Axe,
            ItemType.Umbrella,
            ItemType.Guitar,
            ItemType.BearToy,
        };
        public static Dictionary<ItemType, Vector3> AlcoPosOffset = new Dictionary<ItemType, Vector3>()
        {
            { ItemType.LcnDrink1, new Vector3(0.15, -0.25, -0.1) },
            { ItemType.LcnDrink2, new Vector3(0.15, -0.25, -0.1) },
            { ItemType.LcnDrink3, new Vector3(0.15, -0.23, -0.1) },
            { ItemType.RusDrink1, new Vector3(0.15, -0.23, -0.1) },
            { ItemType.RusDrink2, new Vector3(0.15, -0.23, -0.1) },
            { ItemType.RusDrink3, new Vector3(0.15, -0.23, -0.1) },
            { ItemType.YakDrink1, new Vector3(0.12, -0.02, -0.03) },
            { ItemType.YakDrink2, new Vector3(0.15, -0.23, -0.10) },
            { ItemType.YakDrink3, new Vector3(0.15, 0.03, -0.06) },
            { ItemType.ArmDrink1, new Vector3(0.15, -0.18, -0.10) },
            { ItemType.ArmDrink2, new Vector3(0.15, -0.18, -0.10) },
            { ItemType.ArmDrink3, new Vector3(0.15, -0.18, -0.10) },
        };
        public static Dictionary<ItemType, Vector3> AlcoRotOffset = new Dictionary<ItemType, Vector3>()
        {
            { ItemType.LcnDrink1, new Vector3(-80, 0, 0) },
            { ItemType.LcnDrink2, new Vector3(-80, 0, 0) },
            { ItemType.LcnDrink3, new Vector3(-80, 0, 0) },
            { ItemType.RusDrink1, new Vector3(-80, 0, 0) },
            { ItemType.RusDrink2, new Vector3(-80, 0, 0) },
            { ItemType.RusDrink3, new Vector3(-80, 0, 0) },
            { ItemType.YakDrink1, new Vector3(-80, 0, 0) },
            { ItemType.YakDrink2, new Vector3(-80, 0, 0) },
            { ItemType.YakDrink3, new Vector3(-80, 0, 0) },
            { ItemType.ArmDrink1, new Vector3(-80, 0, 0) },
            { ItemType.ArmDrink2, new Vector3(-80, 0, 0) },
            { ItemType.ArmDrink3, new Vector3(-80, 0, 0) },
        };
        #endregion

        #region Constructor
        [ServerEvent(Event.ResourceStart)]
        public void onResourceStart()
        {
            try
            {
                Log.Write("Loading player items...", nLog.Type.Success);
                // // //
                var result = MySQL.QueryRead($"SELECT * FROM `inventory`");
                if (result == null || result.Rows.Count == 0)
                {
                    Log.Write("DB return null result", nLog.Type.Warn);
                    return;
                }
                foreach (DataRow Row in result.Rows)
                {
                    int UUID = Convert.ToInt32(Row["uuid"]);
                    string json = Convert.ToString(Row["items"]);
                    List<nItem> items = JsonConvert.DeserializeObject<List<nItem>>(json);
                    Items.Add(UUID, items);
                }
                SaveTimer = new Timer(new TimerCallback(SaveAll), null, 0, 1800000);
                Log.Write("Items loaded.", nLog.Type.Success);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_CONSTRUCT\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region Add/Remove item
        public static void Add(Player player, nItem item)
        {
            try
            {
                int UUID = Main.Players[player].UUID;
                int index = FindIndex(UUID, item.Type);
                if (ClothesItems.Contains(item.Type) || WeaponsItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing || nInventory.IgnoreItems.Contains(item.Type))
                {
                    Items[UUID].Add(item);
                    InvInterface.Update(player, item, Items[UUID].IndexOf(item));
                }
                else
                {
                    if (index != -1)
                    {
                        int count = Items[UUID][index].Count;
                        Items[UUID][index].Count = count + item.Count;
                        InvInterface.Update(player, Items[UUID][index], index);
                        Log.Debug($"Added existing item! {UUID.ToString()}:{index.ToString()}");
                    }
                    else
                    {
                        Items[UUID].Add(item);
                        InvInterface.Update(player, item, Items[UUID].IndexOf(item));
                    }
                }
                Log.Debug($"Item added. {UUID.ToString()}:{index.ToString()}");
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_ADD\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static int GetWeight(List<nItem> items)
        {
            try
            {
                int total = 0;
                foreach (nItem item in items) 
                {
                    if (item.Type != ItemType.CasinoChips) 
                        total += item.Count * nInventory.InventoryItems.Find(x => x.ItemType == item.Type).Weight;    
                }
                return total;
            }
            catch { return 0; }
        }

        public static bool IsFullWeight(List<nItem> items, int max, nItem toadd)
        {
            try
            {
                max = max * 1000;
                int total = GetWeight(items) + toadd.Count * nInventory.InventoryItems.Find(x => x.ItemType == toadd.Type).Weight;
                return total > max;
            }
            catch { return false; }
        }

        public static int TryAdd(Player client, nItem item)
        {
            try
            {
                int UUID = Main.Players[client].UUID;
                int index = FindIndex(UUID, item.Type);
                int tail = 0;

                if (IsFullWeight(nInventory.Items[UUID], 20, item))
                    return -1;

                return tail;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_ADD\":\n" + e.ToString(), nLog.Type.Error);
                return 0;
            }
        }
        public static void Remove(Player player, ItemType type, int count)
        {
            try
            {
                int UUID = Main.Players[player].UUID;
                int Index = FindIndex(UUID, type);
                if (Index != -1)
                {
                    int temp = Items[UUID][Index].Count - count;
                    if (temp > 0)
                    {
                        Items[UUID][Index].Count = temp;
                        InvInterface.Update(player, Items[UUID][Index], Index);
                    }
                    else
                    {
                        Items[UUID].RemoveAt(Index);
                        InvInterface.sendItems(player);
                    }
                }
                Log.Debug($"Item removed. {UUID.ToString()}:{Index.ToString()}");
                return;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_REMOVE\":\n" + e.ToString(), nLog.Type.Error);
            }

        }
        public static void Remove(Player player, nItem item)
        {
            try
            {
                int UUID = Main.Players[player].UUID;

                if (ClothesItems.Contains(item.Type) || WeaponsItems.Contains(item.Type) || MeleeWeaponsItems.Contains(item.Type) || item.Type == ItemType.BagWithDrill
                    || item.Type == ItemType.BagWithMoney || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing || nInventory.IgnoreItems.Contains(item.Type))
                {
                    Items[UUID].Remove(item);
                    InvInterface.sendItems(player);
                    Log.Debug($"Item removed. {UUID.ToString()}:TYPE {(int)item.Type}");
                }
                else
                {
                    int Index = FindIndex(UUID, item.Type);
                    if (Index != -1)
                    {
                        int temp = Items[UUID][Index].Count - item.Count;
                        if (temp > 0)
                        {
                            Items[UUID][Index].Count = temp;
                            InvInterface.Update(player, Items[UUID][Index], Index);
                        }
                        else
                        {
                            Items[UUID].RemoveAt(Index);
                            InvInterface.sendItems(player);
                        }
                    }
                    Log.Debug($"Item removed. {UUID.ToString()}:{Index.ToString()}");
                }
                return;
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_REMOVE\":\n" + e.ToString(), nLog.Type.Error);
            }

        }
        #endregion

        #region Save items to db
        public static void SaveAll(object state = null)
        {
            try
            {
                if (Items.Count == 0) return;
                Dictionary<int, List<nItem>> cItems = new Dictionary<int, List<nItem>>(Items);

                List<int> uuids = new List<int>();

                foreach (Player ply in NAPI.Pools.GetAllPlayers())
                    if (Main.Players.ContainsKey(ply))
                        uuids.Add(Main.Players[ply].UUID);

                foreach (KeyValuePair<int, List<nItem>> kvp in cItems)
                {
                    if (!uuids.Contains(kvp.Key)) continue;

                    int UUID = kvp.Key;
                    string json = JsonConvert.SerializeObject(kvp.Value);
                    MySQL.Query($"UPDATE `inventory` SET items='{json}' WHERE uuid={UUID}");
                }

            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_SAVEALL\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        public static void Save(int UUID)
        {
            try
            {
                if (!Items.ContainsKey(UUID)) return;
                Log.Write($"Saving items for {UUID}", nLog.Type.Info);
                string json = JsonConvert.SerializeObject(Items[UUID]);
                MySQL.Query($"UPDATE `inventory` SET items='{json}' WHERE uuid={UUID}");
                Log.Write("Saved!", nLog.Type.Save);
            }
            catch (Exception e)
            {
                Log.Write("EXCEPTION AT \"INVENTORY_SAVE\":\n" + e.ToString(), nLog.Type.Error);
            }
        }
        #endregion

        #region SPECIAL
        public static nItem Find(int UUID, ItemType type)
        {
            List<nItem> items = Items[UUID];
            nItem result = items.Find(i => i.Type == type);
            return result;
        }
        public static int FindIndex(int UUID, ItemType type)
        {
            List<nItem> items = Items[UUID];
            int result = items.FindIndex(i => i.Type == type);
            return result;
        }

        public static bool isFull(int UUID)
        {
            Player ply = NAPI.Player.GetPlayerFromName(Main.PlayerNames[UUID]);
            if (ply.HasData("BAG_UP"))
                return Items[UUID].Count >= 42;

            return Items[UUID].Count >= 20;
        }

        public static void Check(int uuid)
        { //if items dict does not contains account uuid, then add him
            if (!Items.ContainsKey(uuid))
            {
                Items.Add(uuid, new List<nItem>());
                MySQL.Query($"INSERT INTO `inventory`(`uuid`,`items`) VALUES ({uuid},'{JsonConvert.SerializeObject(new List<nItem>())}')");
                Log.Debug("Player added");
            }
        }

        public static void UnActiveItem(Player player, ItemType type)
        {
            var items = Items[Main.Players[player].UUID];
            foreach (var i in items)
                if (i.Type == type && i.IsActive)
                {
                    i.IsActive = false;
                    InvInterface.Update(player, i, items.IndexOf(i));
                }
            Items[Main.Players[player].UUID] = items;
        }
        public static void ClearWithoutClothes(Player player)
        {
            try
            {
                int uuid = Main.Players[player].UUID;
                List<nItem> items = Items[uuid];
                List<nItem> upd = new List<nItem>();
                foreach (nItem item in items)
                    if (ClothesItems.Contains(item.Type) || item.Type == ItemType.CarKey || item.Type == ItemType.KeyRing || nInventory.IgnoreItems.Contains(item.Type)) upd.Add(item);

                Items[uuid] = upd;
                InvInterface.sendItems(player);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        public static void ClearAllClothes(Player Player)
        {
            try
            {
                int uuid = Main.Players[Player].UUID;
                List<nItem> items = Items[uuid];
                List<nItem> upd = new List<nItem>();
                foreach (nItem item in items)
                    if (!ClothesItems.Contains(item.Type)) upd.Add(item);

                Items[uuid] = upd;
                InvInterface.sendItems(Player);
            }
            catch (Exception e)
            {
                Log.Write(e.ToString(), nLog.Type.Error);
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace NeptuneEVO.SDK
{
    public enum ItemType
    {
        Mask = -1,
        Gloves = -3,
        Leg = -4,
        Bag = -5,
        Feet = -6, 
        Jewelry = -7,
        Undershit = -8,
        BodyArmor = -9,
        Unknown = -10, 
        Top = -11,
        Hat = -12, 
        Glasses = -13, 
        Accessories = -14,

        Debug = 0,
        BagWithMoney = 12,
        Material = 13,    
        Drugs = 14,      
        BagWithDrill = 15,
        HealthKit = 1,    
        GasCan = 2,       
        Сrisps = 3,       
        Beer = 4,         
        Pizza = 5,        
        Burger = 6,       
        HotDog = 7,       
        Sandwich = 8,     
        eCola = 9,        
        Sprunk = 10,     
        Lockpick = 11,   
        ArmyLockpick = 16,
        Pocket = 17,     
        Cuffs = 18,      
        CarKey = 19,     
        Present = 40,    
        KeyRing = 41,    
        Balon = 42,      

        Orange = 43,
        Apple = 44,
        Banana = 45,
        Dount = 46,
        Salad = 47,
        Nuggets = 48,
        PowerEngineer = 49,
        ChillAquaWater = 50,
        ChillAquaBigWater = 51,
        ChillAquaGaz = 52,
        Cocktail = 53,
        Note = 54,
        RDildo = 55,
        BDildo = 56,
        PDildo = 57,
        BearToy = 58,
        Guitar = 59,
        Rose = 60,
        Vape = 61,
        Bong = 62,
        Binocular = 63,
        Umbrella = 64,
        Camera = 65,
        Micophone = 66,
        DoneNote = 67,

        LSPDDrone = 779,

        /* Drinks */
        RusDrink1 = 20,
        RusDrink2 = 21,
        RusDrink3 = 22,

        YakDrink1 = 23,
        YakDrink2 = 24,
        YakDrink3 = 25,

        LcnDrink1 = 26,
        LcnDrink2 = 27,
        LcnDrink3 = 28,

        ArmDrink1 = 29,
        ArmDrink2 = 30,
        ArmDrink3 = 31,

        /* Weapons */
        /* Pistols */
        Pistol = 100,
        CombatPistol = 101,
        Pistol50 = 102,
        SNSPistol = 103,
        HeavyPistol = 104,
        VintagePistol = 105,
        MarksmanPistol = 106,
        Revolver = 107,
        APPistol = 108,
        FlareGun = 110,
        DoubleAction = 111,
        PistolMk2 = 112,
        SNSPistolMk2 = 113,
        RevolverMk2 = 114,
        /* SMG */
        MicroSMG = 115,
        MachinePistol = 116,
        SMG = 117,
        AssaultSMG = 118,
        CombatPDW = 119,
        MG = 120,
        CombatMG = 121,
        Gusenberg = 122,
        MiniSMG = 123,
        SMGMk2 = 124,
        CombatMGMk2 = 125,
        /* Rifles */
        AssaultRifle = 126,
        CarbineRifle = 127,
        AdvancedRifle = 128,
        SpecialCarbine = 129,
        BullpupRifle = 130,
        CompactRifle = 131,
        AssaultRifleMk2 = 132,
        CarbineRifleMk2 = 133,
        SpecialCarbineMk2 = 134,
        BullpupRifleMk2 = 135,
        /* Sniper */
        SniperRifle = 136,
        HeavySniper = 137,
        MarksmanRifle = 138,
        HeavySniperMk2 = 139,
        MarksmanRifleMk2 = 140,
        /* Shotguns */
        PumpShotgun = 141,
        SawnOffShotgun = 142,
        BullpupShotgun = 143,
        AssaultShotgun = 144,
        Musket = 145,
        HeavyShotgun = 146,
        DoubleBarrelShotgun = 147,
        SweeperShotgun = 148,
        PumpShotgunMk2 = 149,
        /* MELEE WEAPONS */
        StunGun = 109,
        Knife = 180,
        Nightstick = 181,
        Hammer = 182,
        Bat = 183,
        Crowbar = 184,
        GolfClub = 185,
        Bottle = 186,
        Dagger = 187,
        Hatchet = 188,
        KnuckleDuster = 189,
        Machete = 190,
        Flashlight = 191,
        SwitchBlade = 192,
        PoolCue = 193,
        Wrench = 194,
        BattleAxe = 195,
        /* Ammo */
        PistolAmmo = 200,
        SMGAmmo = 201,
        RiflesAmmo = 202,
        SniperAmmo = 203,
        ShotgunsAmmo = 204,

        /* Fishing */
        Rod = 205,        
        RodUpgrade = 206,  
        RodMK2 = 207,      
        Naz = 208,         
        Koroska = 209,    
        Kyndja = 210,      
        Lococ = 211,       
        Okyn = 212,        
        Ocetr = 213,      
        Skat = 214,        
        Tunec = 215,     
        Ygol = 216,        
        Amyr = 217,        
        Chyka = 218,      

        CasinoChips = 229, 

        Repair = 250,      

        Bint = 251,        
        Aptechka = 252,    
        Tabletka = 253,    
        Adrenalin = 254,  

        Kokos = 255,       
        SnowBall = 258,     

        WalkieTalkie = 566,
        Boombox = 610,
        PopIt = 302,
        IDCard = 303,
        NumberPlate = 304,
        PickAxe = 305,
        GoldOre = 306,
        SilverOre = 307,
        IronOre = 308,
        CuprumOre = 309,
        Stone = 310,
        WoodPile = 311,
        Axe = 312,
        PickAxe2 = 313,
        PickAxe3 = 314,

        MoneyItem = 315,
        ExpItem = 316,
        DonateItem = 317,
    }

    public class nItem
    {
        public int ID { get; internal set; }
        public ItemType Type { get; internal set; }
        public int Count { get; set; }
        public bool IsActive { get; set; }
        public dynamic Data;
        public float Wear { get; set; }
        public object subData { get; set; } = null;
        public int FastSlots { get; set; } = -1;

        public nItem(ItemType type, int count = 1, dynamic data = null, bool isActive = false, float wear = 100f, object subdata = null, int fs = -1)
        {
            ID = Convert.ToInt32(type);
            Type = type;
            Count = count;
            Data = data;
            IsActive = isActive;
            Wear = wear;
            subData = subdata;
            FastSlots = fs;
        }
    }
    public class PostalItem
    {
        public string Sender { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public nItem Item { get; set; }
        public int Time { get; set; }

        public PostalItem(string sender, string name, string date, nItem item, int time = 0)
        {
            Sender = sender;
            Name = name;
            Date = date;
            Item = item;
            Time = time;
        }
    }
}

using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NeptuneEVO.SDK;
using NeptuneEVO.Core;

namespace NeptuneEVO.Businesses
{
    class ShopI : Script
    {

        private static nLog Log = new nLog("SHOP");

        private static int LastID = 0;
        public static List<Product> DeliveryClubItems = new List<Product>
        {
            new Product(ItemType.Burger, 35000),
            new Product(ItemType.HotDog, 32000),
            new Product(ItemType.Sandwich, 35000),
            new Product(ItemType.Сrisps, 30000),
            new Product(ItemType.Pizza, 40000),
            new Product(ItemType.Apple, 4000),
            new Product(ItemType.Banana, 4000),
            new Product(ItemType.Dount, 4000),
            new Product(ItemType.Salad, 4000),
            new Product(ItemType.Nuggets, 4000),

            new Product(ItemType.eCola, 35000),
            new Product(ItemType.Sprunk, 32000),
            new Product(ItemType.Beer, 32000),
            new Product(ItemType.ChillAquaWater, 35000),
            new Product(ItemType.ChillAquaBigWater, 30000),
            new Product(ItemType.ChillAquaGaz, 40000),
            new Product(ItemType.PowerEngineer, 4000),
            new Product(ItemType.Cocktail, 4000),
        };
        public static Dictionary<int, Dictionary<string, List<Product>>> List = new Dictionary<int, Dictionary<string, List<Product>>>
        {
            { 0, new Dictionary<string, List<Product>>
                {
                    {"Еда", new List<Product>  
                        {
                            new Product(ItemType.Burger, 35000),
                            new Product(ItemType.HotDog, 32000),
                            new Product(ItemType.Sandwich, 35000),
                            new Product(ItemType.Сrisps, 30000),
                            new Product(ItemType.Pizza, 40000),
                            new Product(ItemType.Apple, 4000),
                            new Product(ItemType.Banana, 4000),
                            new Product(ItemType.Dount, 4000),
                            new Product(ItemType.Salad, 4000),
                            new Product(ItemType.Nuggets, 4000),
                        }
                    },
                    {"Вода", new List<Product>
                        {
                            new Product(ItemType.eCola, 35000),
                            new Product(ItemType.Sprunk, 32000),
                            new Product(ItemType.Beer, 32000),
                            new Product(ItemType.ChillAquaWater, 35000),
                            new Product(ItemType.ChillAquaBigWater, 30000),
                            new Product(ItemType.ChillAquaGaz, 40000),
                            new Product(ItemType.PowerEngineer, 4000),
                            new Product(ItemType.Cocktail, 4000),
                        }
                    },
                    {"Инструменты", new List<Product>
                        {
                            new Product(ItemType.Knife, 35000),
                            new Product(ItemType.Flashlight, 32000),
                            new Product(ItemType.Hammer, 35000),
                        }
                    },
                    {"Разное", new List<Product>
                        {
                            new Product(ItemType.BearToy, 35000),
                            new Product(ItemType.Note, 35000),
                            new Product(ItemType.Guitar, 35000),
                            new Product(ItemType.Rose, 35000),
                            new Product(ItemType.Umbrella, 35000),
                            new Product(ItemType.RDildo, 35000),
                            new Product(ItemType.BDildo, 32000),
                            new Product(ItemType.PDildo, 35000),
                            new Product(ItemType.Bong, 35000),
                        }
                    },
                }
            },
            { 1, new Dictionary<string, List<Product>>
                {
                    {"Рыболовство", new List<Product>
                        {
                            new Product(ItemType.Rod, 750000),
                            new Product(ItemType.Naz, 120000),
                        }
                    },  
                    {"Фермерство", new List<Product>
                        {
                            /*new Product(ItemType.SPotata, 35000),
                            new Product(ItemType.SMorkov, 35000),
                            new Product(ItemType.SClever, 35000),
                            new Product(ItemType.SPshenica, 35000),       */
                        }
                    },
                    {"Разное", new List<Product>
                        {
                            new Product(ItemType.Binocular, 35000),
                        }
                    },
                }
            },
            { 2, new Dictionary<string, List<Product>>
                {
                    {"Электроника", new List<Product>
                        {
                            new Product(ItemType.Vape, 120000),
                            new Product(ItemType.Camera, 1000000),
                            new Product(ItemType.Micophone, 150000),
                            new Product(ItemType.WalkieTalkie, 150000),
                        }
                    },
                }
            },
            { 4, new Dictionary<string, List<Product>>
                {
                    {"Расходующие материалы", new List<Product>
                        {
                            new Product(ItemType.Aptechka, 750000),
                            new Product(ItemType.Bint, 120000),
                            new Product(ItemType.Adrenalin, 1000000),
                            new Product(ItemType.Tabletka, 150000),
                        }
                    },
                }
            },
        };

        public class Product
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int Price { get; set; }
            public int ItemTInt { get; set; }
            public Product(ItemType type, int p)
            {
                ID = LastID; Name = nInventory.InventoryItems.Find(x => x.ItemType == type).Name; Price = p; ItemTInt = (int)type;
                LastID++;
            }
        }

        public class HealShop : BCore.Bizness
        {
            public HealShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 18;
                Name = "Аптека";
                BlipColor = 11;
                BlipType = 153;
                Range = 1f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 4, Type, Name);
            }
        }

        public class SimShop : BCore.Bizness
        {
            public SimShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 16;
                Name = "Магазин электроники";
                BlipColor = 4;
                BlipType = 606;
                Range = 1f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 2, Type, Name);
            }
        }

        public class EatShop : BCore.Bizness
        {
            public EatShop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 8;
                Name = "Магазин туристического снаряжения";
                BlipColor = 17;
                BlipType = 266;
                Range = 1f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 1, Type, Name);
            }
        }

        public class Shop : BCore.Bizness
        {
            public Shop(int id, string owner, Vector3 position, Vector3 matposition, int cost, int mafia, int bankid, int mat, Vector3 infopoint, int ear) : base(id, owner, position, matposition, cost, mafia, bankid, mat, infopoint, ear)
            {
                Type = 0;
                Name = "Магазин 24/7";
                BlipColor = 4;
                BlipType = 52;
                Range = 1f;

                CreateStuff();
            }

            public override void InteractPress(Player player)
            {
                if (!Main.Players.ContainsKey(player)) return;
                SimpleInteract(player, 0, Type, Name);
            }
        }

        // REMOTE EVENTS //

        [RemoteEvent("server::shop:buy")]
        public static void ClientEvent_SHOP_BUY(Player player, bool card, string json)
        {
            try
            {
                if (!player.HasData("IDS")) return;
                Buy(player, card, json);
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }

        public static void SimpleInteract(Player player, int index, int type, string nameM)
        {
            List<List<List<object>>> items = new List<List<List<object>>>();
            List<string> cats = new List<string>();
            foreach (string name in List[index].Keys)
                cats.Add(name);
            foreach (List<Product> prods in List[index].Values) {
                List<List<object>> prodcat = new List<List<object>>();
                foreach (Product prod in prods)
                {
                    List<object> item = new List<object>();
                    item.Add(prod.ID);
                    item.Add(prod.Name);
                    item.Add(prod.ItemTInt);
                    item.Add(prod.Price);
                    prodcat.Add(item);
                }
                items.Add(prodcat);
            }
            Trigger.PlayerEvent(player, "client::shop:open", nameM, JsonConvert.SerializeObject(cats), JsonConvert.SerializeObject(items));
            player.SetData("IDS", index);
        }

        public static void Buy(Player player, bool card, string json)
        {
            if (!Main.Players.ContainsKey(player)) return;
            List<List<object>> items = JsonConvert.DeserializeObject<List<List<object>>>(json);
            int total = 0;
            foreach (List<object> prod in items)
            {
                total += Convert.ToInt32(prod[3]) * Convert.ToInt32(prod[4]);
            }
            if (Main.Players[player].Money < total)
            {
                Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Недостаточно средств", 3000);
                return;
            }
            foreach (List<object> prod in items)
            {
                ItemType itemproduct = (ItemType)Convert.ToInt32(prod[2]);
                var tryAdd = nInventory.TryAdd(player, new nItem(itemproduct, Convert.ToInt32(prod[4])));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                    return;
                }
            }
            foreach (List<object> prod in items)
            {
                ItemType itemproduct = (ItemType)Convert.ToInt32(prod[2]);
                var tryAdd = nInventory.TryAdd(player, new nItem(itemproduct));
                if (tryAdd == -1 || tryAdd > 0)
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"Недостаточно места в инвентаре", 3000);
                    return;
                }
                if (Convert.ToString(prod[1]) == "Сим-карта")
                {
                    if (Main.Players[player].Sim != -1) Main.SimCards.Remove(Main.Players[player].Sim);
                    Main.Players[player].Sim = Main.GenerateSimcard(Main.Players[player].UUID);
                    Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы купили сим-карту с номером {Main.Players[player].Sim}", 3000);
                    InvInterface.sendStats(player);
                }
                else
                {
                    nItem item = (itemproduct == ItemType.KeyRing) ? new nItem(ItemType.KeyRing, 1, "") : new nItem(itemproduct, Convert.ToInt32(prod[4]));
                    nInventory.Add(player, item);
                }
            }
            MoneySystem.Wallet.Change(player, -total);
            Notify.Succ(player, "Вы успешно купили предметы");
        }
    }
}

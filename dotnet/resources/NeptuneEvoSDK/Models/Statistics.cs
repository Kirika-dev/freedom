using System;

namespace NeptuneEVO.SDK
{
    public class Statictic
    {
        public int Arrest { get; set; } 
        public int Warns { get; set; } 
        public int Kills { get; set; } 
        public int Deaths { get; set; } 
        public int MoneyEarn { get; set; } 
        public int MoneySpent { get; set; } 
        public Statictic(int arrest, int warns, int kills, int deaths, int earn, int spent) 
        {
            Arrest = arrest;
            Warns = warns;
            Kills = kills;
            Deaths = deaths;
            MoneyEarn = earn;
            MoneySpent = spent;
        }
    } 
}
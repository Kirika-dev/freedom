using System;

namespace NeptuneEVO.Utils.Promocodes
{
    internal class Promocode
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int Prize { get; set; }
        public int Used { get; set; }
        public int UsedAndGet { get; set; }
        public int Time { get; set; }
        public DateTime Limit { get; set; }

        public Promocode(string name, string type, int prize, int used, int usedandget, int time, DateTime limit)
        {
            Name = name;
            Type = type;
            Prize = prize;
            Used = used;
            UsedAndGet = usedandget;
            Time = time;
            Limit = limit;
        }
    }
}

﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace NeptuneEVO.SDK
{ 
    public class WantedLevel
    {
        public int Level { get; set; }
        public string WhoGive { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }

        public WantedLevel(int level, string whoGive, DateTime date, string reason)
        {
            Level = level;
            WhoGive = whoGive;
            Date = date;
            Reason = reason;
        }
    }
}

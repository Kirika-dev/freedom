using System;
using System.Collections.Generic;

namespace NeptuneEVO.SDK
{
    public class Pass
    {
        public bool Buyed { get; set; }
        public Dictionary<int, ItemPass> Free { get; set; }
        public Dictionary<int, ItemPass> Premium { get; set; }
        public int Lvl { get; set; }
        public int EXP { get; set; }
        public List<QuestPass> QuestList { get; set; }
        public DateTime DateLastUpdate { get; set; }
        public int CountQuestsComplete { get; set; }
        public int TimeGiveExp { get; set; }
        public Pass(bool buy, Dictionary<int, ItemPass> free, Dictionary<int, ItemPass> prem, int lvl, int exp, List<QuestPass> questslist, DateTime date, int cqc, int timeexp)
        {
            Buyed = buy;
            Free = free;
            Premium = prem;
            Lvl = lvl;
            EXP = exp;
            QuestList = questslist;
            DateLastUpdate = date;
            CountQuestsComplete = cqc;
            TimeGiveExp = timeexp;
        }
    }
    public class QuestPass
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Progress { get; set; }
        public int Max { get; set; }
        public int Rewards { get; set; }
        public bool Complete { get; set; }
        public QuestPass(int id, string name, string desc, int max, int rew, bool compl = false, int prgs = 0)
        {
            ID = id;
            Name = name;
            Description = desc;
            Progress = prgs;
            Max = max;
            Rewards = rew;
            Complete = compl;
        }
    }
    public class ItemPass
    {
        public List<IPass> items { get; set; } = new List<IPass>();
        public ItemPass(List<IPass> itemss)
        {
            items = itemss;
        }
    }
    public class IPass
    {
        public string Name { get; set; }
        public bool Taken { get; set; }
        public string Picture { get; set; }
        public string Type { get; set; }
        public string Settings { get; set; }
        public IPass(string name, string pic, string type, string settings, bool taken = false)
        {
            Name = name;
            Picture = pic;
            Type = type;
            Settings = settings;
            Taken = taken;
        }
    }
}
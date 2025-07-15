using System;
using System.Collections.Generic;

namespace NeptuneEVO.SDK
{
    public class Achievements
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Progress { get; set; }
        public int Max { get; set; }
        public List<nItem> Rewards { get; set; } = new List<nItem>();
        public bool Complete { get; set; }
        public Achievements(int id, string name, string desc, int max, List<nItem> rew, bool compl = false, int prgs = 0)
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
}
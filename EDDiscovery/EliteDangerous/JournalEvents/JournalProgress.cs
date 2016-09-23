﻿using Newtonsoft.Json.Linq;
using System.Linq;

namespace EDDiscovery.EliteDangerous.JournalEvents
{
    //When written: at startup
    //Parameters:
    //•	Combat: percent progress to next rank
    //•	Trade: 		“
    //•	Explore: 	“
    //•	Empire: 	“
    //•	Federation: 	“
    //•	CQC: 		“ ranks: 0=’Helpless’, 1=’Mostly Helpless’, 2=’Amateur’, 3=’Semi Professional’, 4=’Professional’, 5=’Champion’, 6=’Hero’, 7=’Legend’, 8=’Elite’

    public class JournalProgress : JournalEntry
    {

        public JournalProgress(JObject evt ) : base(evt, JournalTypeEnum.Progress)
        {
            Combat = Tools.GetInt(evt["Combat"]);
            Trade = Tools.GetInt(evt["Trade"]);
            Explore = Tools.GetInt(evt["Explore"]);
            Empire = Tools.GetInt(evt["Empire"]);
            Federation = Tools.GetInt(evt["Federation"]);
            CQC = Tools.GetInt(evt["CQC"]);

        }
        public int Combat { get; set; }
        public int Trade { get; set; }
        public int Explore { get; set; }
        public int Empire { get; set; }
        public int Federation { get; set; }
        public int CQC { get; set; }
    }
}

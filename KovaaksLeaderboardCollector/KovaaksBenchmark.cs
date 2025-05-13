using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace KovaaksLeaderboardCollector
{
    [XmlRoot("Scenario")]
    public class KvksScenario
    {
        public string Name { get; set; }
        public string LeaderboardID { get; set; }

        [XmlIgnore]
        public float score;
    }
}

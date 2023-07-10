namespace YYZ.JTS
{
    using System;
    using System.Collections.Generic;

    public class JTSParser
    {
        public JTSSeries Series;

        public JTSParser(JTSSeries series)
        {
            this.Series = series;
        }

        static Dictionary<String, JTSSeries> codeMap = new()
        {
            {"NB", JTSSeries.NapoleonicBattle},
            {"CWB", JTSSeries.CivilWarBattle},
            {"PZC", JTSSeries.PanzerCampaign}
        };

        public static JTSParser FromCode(string s)
        {
            return new JTSParser(codeMap[s]);
        }

        public JTSScenario ParseScenario(string scenarioStr)
        {
            JTSScenario sc = null;
            switch(Series)
            {
                case JTSSeries.NapoleonicBattle:
                    sc = new NBScenario();
                    break;
                case JTSSeries.CivilWarBattle:
                    sc = new CWBScenario();
                    break;
                case JTSSeries.PanzerCampaign:
                    sc = new PZCScenario();
                    break;
            }
            sc.Extract(scenarioStr);
            return sc;
        }

        public UnitGroup ParseOOB(string oobStr)
        {
            return JTSOobParser.FromSeries(Series).ParseUnits(oobStr);
        }

        
        public MapFile ParseMap(string mapStr, bool strict=true)
        {
            MapFile mp = null;
            switch(Series)
            {
                case JTSSeries.NapoleonicBattle:
                    mp = new NBMapFile();
                    break;
                case JTSSeries.CivilWarBattle:
                    mp = new CWBMapFile();
                    break;
                case JTSSeries.PanzerCampaign:
                    mp = new PZCMapFile();
                    break;
            }
            mp.Extract(mapStr, strict);
            return mp;
        }
    }
}

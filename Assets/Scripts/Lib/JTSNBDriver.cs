namespace YYZ.JTS.NB
{


    using System.Collections;
    using System.Collections.Generic;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    // using UnityEngine;

    /*
    public enum GroupSize
    {
        Army,
        Wing,
        Corp,
        Division,
        Brigade,
    }
    */

    public enum UnitType
    {
        Infantry,
        Cavalry,
        Artillery
    }

    public class UnitSubType
    {
        public string Name;
        public UnitType Type;
        public string Code;

        public override string ToString()
        {
            return $"UnitSubType({Name}, {Type}, {Code})";
        }
    }

    public abstract class AbstractUnit
    {
        public string Name;
        public UnitGroup Parent;
        public string Icon2D;

        string country;
        public string Country
        {
            get
            {
                if (country == null)
                    return Parent.Country;
                return country;
            }
            set
            {
                country = value;
            }
        }

        public override string ToString() => $"AbstractUnit({Name})";
    }

    public class UnitGroup : AbstractUnit
    {
        // public GroupSize Size;
        public string Size; // A -> Army, W -> Wing, C -> Corp, D -> Divsion, B -> Brigade
        public List<AbstractUnit> Units = new List<AbstractUnit>();

        public IEnumerable<AbstractUnit> Walk()
        {
            yield return this;
            foreach (var unit in Units)
            {
                var group = unit as UnitGroup;
                if (group == null)
                {
                    yield return unit;
                }
                else
                {
                    foreach (var _unit in group.Walk()) // TODO: Better grammar?
                        yield return _unit;
                }
            }
        }

        public void Summary(out int strength, out int guns)
        {
            strength = 0;
            guns = 0;
            foreach (var unit in Units)
            {
                var group = unit as UnitGroup;
                if (group != null)
                {
                    group.Summary(out var _strength, out var _guns);
                    strength += _strength;
                    guns += _guns;
                }
                var _unit = unit as UnitOob;
                if (_unit != null)
                {
                    // Debug.Log(_unit);
                    switch (_unit.Type.Type)
                    {
                        case UnitType.Infantry:
                        case UnitType.Cavalry:
                            strength += _unit.Strength;
                            break;
                        case UnitType.Artillery:
                            guns += _unit.Strength;
                            break;
                    }
                }
            }
        }

        public override string ToString()
        {
            Summary(out var strength, out var guns);
            return $"Group({Name}, {Size}, {strength}, {guns})";
        }
    }

    public class UnitOob : AbstractUnit
    {
        // U 718 6 U I 91 56 1/95th Rifles
        // U: Unit label
        // 718: Strength
        // 6: Morale
        // U: Type (A: Heavy Art, B: Light Art, C: Horse Art, E: Emplaced Guns, D: Dragoon, L: Light Cav, H: Heavy Cav, K: Cossack, I: Line Inf, M: Militia, V: Light Inf, G: Guard Inf, R: Restricted Inf)
        // I: Weapon (Name is defined in Data/weapon.dat, parameters are defined in *.pdt files)
        // 91: Icon 2D (Point to the spritesheet Info/Unit.bmp)
        // 56: Icon 3D (Map/3DUnits*)
        // 1/95th Rifles: Unit Name

        public int Strength;
        public int Morale;
        // public string Type;
        public UnitSubType Type;
        public string Weapon;
        public string Icon3D;
        public override string ToString() => $"Unit({Name}, {Strength}, Morale={Morale}, Type={Type}, Weapon={Weapon})";

        public static UnitSubType[] UnitSubTypes =
        {
            new UnitSubType(){Name="Heavy Artillery", Type=UnitType.Artillery, Code="A"},
            new UnitSubType(){Name="Light Artillery", Type=UnitType.Artillery, Code="B"},
            new UnitSubType(){Name="Horse Artillery", Type=UnitType.Artillery, Code="C"},
            new UnitSubType(){Name="Emplaced Guns", Type=UnitType.Artillery, Code="E"},

            new UnitSubType(){Name="Dragoon", Type=UnitType.Cavalry, Code="D"},
            new UnitSubType(){Name="Light Cavalry", Type=UnitType.Cavalry, Code="L"},
            new UnitSubType(){Name="Heavy Cavalry", Type=UnitType.Cavalry, Code="H"},
            new UnitSubType(){Name="Cossack", Type=UnitType.Cavalry, Code="K"},

            new UnitSubType(){Name="Line Infantry", Type=UnitType.Infantry, Code="I"},
            new UnitSubType(){Name="Militia", Type=UnitType.Infantry, Code="M"},
            new UnitSubType(){Name="Light Infantry", Type=UnitType.Infantry, Code="V"},
            new UnitSubType(){Name="Guard Infantry", Type=UnitType.Infantry, Code="G"},
            new UnitSubType(){Name="Restricted Infantry", Type=UnitType.Infantry, Code="R"},

            new UnitSubType(){Name="Line Infantry (2 rank)", Type=UnitType.Infantry, Code="T"},
            new UnitSubType(){Name="Light Infantry (2 rank)", Type=UnitType.Infantry, Code="U"},
            new UnitSubType(){Name="Guard Infantry (2 rank)", Type=UnitType.Infantry, Code="F"},

            new UnitSubType(){Name="Independent Skirmisher", Type=UnitType.Infantry, Code="S"},
            new UnitSubType(){Name="Pioneer", Type=UnitType.Infantry, Code="P"},
        };

        public static Dictionary<string, UnitSubType> UnitTypeMap;// = new Dictionary<string, UnitType>()

        static UnitOob()
        {
            UnitTypeMap = new Dictionary<string, UnitSubType>();
            foreach (var unitSubType in UnitSubTypes)
            {
                UnitTypeMap[unitSubType.Code] = unitSubType;
            }
        }
    }

    public class Leader : AbstractUnit
    {
        public int Rating;
        public int Leadership;
        public override string ToString() => $"Leader({Name}, {Rating}, {Leadership})";
    }

    public class SupplyWagon : AbstractUnit
    {
        public int Strength;

        public override string ToString() => $"SupplyWagon({Name}, {Strength})";
    }

    public static class JTSOobParser
    {
        public static UnitGroup ParseUnits(string s)
        {
            var lines = s.Split("\n");
            Debug.Assert(lines[0].Trim() == "2");

            var rootGroup = new UnitGroup();
            var stack = new Stack<UnitGroup>();
            stack.Push(rootGroup);
            foreach (var _line in lines.Skip(1))
            {
                var line = _line.Trim();
                var top = stack.Peek();
                switch (line)
                {
                    case "":
                        break;
                    case "Begin":
                        var groupBegin = top.Units[top.Units.Count - 1] as UnitGroup;
                        stack.Push(groupBegin);
                        break;
                    case "End":
                        stack.Pop();
                        break;
                    default:
                        var unit = ParseUnit(stack, line);
                        // Debug.Log($"{line} => {unit}");
                        top.Units.Add(unit);
                        unit.Parent = top;
                        break;
                }
            }
            return rootGroup;
        }

        public static AbstractUnit ParseUnit(Stack<UnitGroup> stack, string line)
        {
            if (stack.Count == 1) // Top unit has 1 extra prefix to define its country
            {
                var s = line.Split(" ", 2);
                var unit = _ParseUnit(s[1]);
                unit.Country = s[0];
                return unit;
            }
            return _ParseUnit(line);
        }

        static AbstractUnit _ParseUnit(string line)
        {
            var ss = line.Split(" ", 2);
            switch (ss[0])
            {
                case "L":
                    var sss = ss[1].Split(" ", 4);
                    return new Leader()
                    {
                        Rating = int.Parse(sss[0]),
                        Leadership = int.Parse(sss[1]),
                        Icon2D = sss[2],
                        Name = sss[3]
                    };
                case "U":
                    sss = ss[1].Split(" ", 7);
                    return new UnitOob()
                    {
                        Strength = int.Parse(sss[0]),
                        Morale = int.Parse(sss[1]),
                        Type = UnitOob.UnitTypeMap[sss[2]],
                        // Type = sss[2],
                        Weapon = sss[3],
                        Icon2D = sss[4],
                        Icon3D = sss[5],
                        Name = sss[6]
                    };
                case "S":
                    sss = ss[1].Split(" ", 3);
                    return new SupplyWagon()
                    {
                        Strength = int.Parse(sss[0]),
                        Icon2D = sss[1],
                        Name = sss[2]
                    };
                default: // group
                    return new UnitGroup()
                    {
                        Size = ss[0],
                        Name = ss[1],
                    };
            }
        }
    }

    public class UnitState
    {
        public AbstractUnit OobItem;

        public int CurrentStrength;
        public int Fatigue;
        public int X;
        public int Y;

        public override string ToString()
        {
            return $"UnitState(CurrentStrength={CurrentStrength}, Fatigue={Fatigue}, X={X}, Y={Y}, {OobItem})";
        }
    }

    public class JTSUnitStates
    {
        // extract unit state (position, direction, current strength, disorder & fatigue state)
        // from a scenario file (*.scn) or a save file (*.btl, *.bte)

        public List<UnitState> UnitStates = new List<UnitState>();
        public Dictionary<AbstractUnit, UnitState> Unit2state = new Dictionary<AbstractUnit, UnitState>();

        // 1 3.5.1 2 1 3 0 0 262144 7 27
        static string unitPattern = @"(\d+) ((?:\d+\.)*\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+)";

        public void Extract(UnitGroup oobRoot, string s)
        {
            foreach(Match match in Regex.Matches(s, unitPattern)) // Groups[0] => full match, Groups[1] => First group, ...
            {
                var oobIndex = match.Groups[2].Value;
                var unit = oobRoot;
                AbstractUnit unitSelected = null;
                foreach(var idx in oobIndex.Split(".").Select(int.Parse))
                {
                    unitSelected = unit.Units[idx-1];
                    unit = unitSelected as UnitGroup;
                }
                
                /*
                Debug.Log($"match={match}");
                for(var i=0; i<match.Groups.Count; i++)
                {
                    Debug.Log($"match.Groups[{i}].Value={match.Groups[i].Value}");
                }
                */

                var unitState = new UnitState()
                {
                    OobItem = unitSelected,
                    CurrentStrength = int.Parse(match.Groups[5].Value),
                    Fatigue = int.Parse(match.Groups[6].Value),
                    X = int.Parse(match.Groups[9].Value),
                    Y = int.Parse(match.Groups[10].Value)
                };

                UnitStates.Add(unitState);
                Unit2state[unitSelected] = unitState;
            }
        }

        public void ExtractByLines(UnitGroup oobRoot, List<string> sl)
        {
            foreach (var s in sl)
            {
                if (s[0] == ' ')
                    continue;

                var ss = s.Trim().Split(" ");

                if (ss[0] != "1")
                    continue;

                var oobIndex = ss[1];
                var unit = oobRoot;
                AbstractUnit unitSelected = null;
                // UnityEngine.Debug.Log($"oobIndex={oobIndex}, s={s}");
                foreach (var idx in oobIndex.Split(".").Select(int.Parse))
                {
                    unitSelected = unit.Units[idx - 1];
                    unit = unitSelected as UnitGroup;
                }

                var unitState = new UnitState()
                {
                    OobItem = unitSelected,
                    CurrentStrength = int.Parse(ss[4]),
                    Fatigue = int.Parse(ss[5]),
                    X = int.Parse(ss[8]),
                    Y = int.Parse(ss[9])
                };

                UnitStates.Add(unitState);
                Unit2state[unitSelected] = unitState;
            }
        }


        public Dictionary<string, List<UnitState>> GroupByCountry()
        {
            var ret = new Dictionary<string, List<UnitState>>();
            foreach(var unitState in UnitStates)
            {
                var country = unitState.OobItem.Country;
                if (ret.TryGetValue(country, out var unitStateList))
                {
                    unitStateList.Add(unitState);
                }
                else
                {
                    ret[country] = new List<UnitState>() { unitState };
                }
            }
            return ret;
        }

        public Dictionary<UnitGroup, List<UnitState>> GroupByBrigade()
        {
            var ret = new Dictionary<UnitGroup, List<UnitState>>();
            foreach(var unitState in UnitStates)
            {
                var parent = unitState.OobItem.Parent;
                var group = parent as UnitGroup;
                if(group != null && group.Size == "B")
                {
                    if(ret.TryGetValue(group, out var unitStateList))
                    {
                        unitStateList.Add(unitState);
                    }
                    else
                    {
                        ret[group] = new List<UnitState>() { unitState };
                    }
                }
            }
            return ret;
        }
    }

    public class JTSScenario
    {
        public string Name;

        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
        public int Turn;
        public int TurnLimit;

        public string MapFile;
        public string OobFile;
        public string PdtFile;

        public List<string> DynamicCommandBlock = new(); // unit states (positions, direction,...), reinforcement
        public List<string> AICommandBlock = new();
        public string Description;

        public void Extract(string s)
        {
            var sl = s.Split("\n");
            Name = sl[1].Trim();

            // 1808 10 31 8 0 0 0 1 32
            //  0   1   2 3 4 5 6 7 8
            var ds = sl[2].Trim().Split(" ").Select(int.Parse).ToArray();
            Year = ds[0];
            Month = ds[1];
            Day = ds[2];
            Hour = ds[3];
            Minute = ds[4];
            // Current Side and Step Mode (15min vs 10min?) or Night state or PBEM flag? 
            Turn = ds[7];
            TurnLimit = ds[8];

            MapFile = sl[6];
            OobFile = sl[7];
            PdtFile = sl[8];

            var idx = 13;
            while (sl[idx][0] != '0')
            {
                DynamicCommandBlock.Add(sl[idx]);
                idx++;
            }
            idx = idx + 2;
            var pair = sl[idx].Split(" ", 2);
            var aiCommandSize = int.Parse(pair[0]);
            var DescriptionIdx = idx + aiCommandSize + 1;

            for (var i = idx + 1; i < DescriptionIdx; i++)
                AICommandBlock.Add(sl[i]);

            var descriptionList = new List<string>();
            for (var i = DescriptionIdx; i < sl.Length; i++)
                descriptionList.Add(sl[i]);

            Description = string.Join("\n", descriptionList);
        }
    }

    
}
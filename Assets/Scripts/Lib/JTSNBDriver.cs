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

    public enum UnitCategory
    {
        Infantry,
        Cavalry,
        Artillery
    }

    public class UnitType
    {
        public string Name;
        public UnitCategory Category;
        public string Code;

        public override string ToString()
        {
            return $"UnitSubType({Name}, {Category}, {Code})";
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
                    switch (_unit.Type.Category)
                    {
                        case UnitCategory.Infantry:
                        case UnitCategory.Cavalry:
                            strength += _unit.Strength;
                            break;
                        case UnitCategory.Artillery:
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
        public UnitType Type;
        public string Weapon;
        public string Icon3D;
        public override string ToString() => $"Unit({Name}, {Strength}, Morale={Morale}, Type={Type}, Weapon={Weapon})";

        /*
        static UnitType[] NBUnitSubTypes = // Napoleonic Battles
        {
            new UnitType(){Name="Heavy Artillery", Category=UnitCategory.Artillery, Code="A"},
            new UnitType(){Name="Light Artillery", Category=UnitCategory.Artillery, Code="B"},
            new UnitType(){Name="Horse Artillery", Category=UnitCategory.Artillery, Code="C"},
            new UnitType(){Name="Emplaced Guns", Category=UnitCategory.Artillery, Code="E"},

            new UnitType(){Name="Dragoon", Category=UnitCategory.Cavalry, Code="D"},
            new UnitType(){Name="Light Cavalry", Category=UnitCategory.Cavalry, Code="L"},
            new UnitType(){Name="Heavy Cavalry", Category=UnitCategory.Cavalry, Code="H"},
            new UnitType(){Name="Cossack", Category=UnitCategory.Cavalry, Code="K"},

            new UnitType(){Name="Line Infantry", Category=UnitCategory.Infantry, Code="I"},
            new UnitType(){Name="Militia", Category=UnitCategory.Infantry, Code="M"},
            new UnitType(){Name="Light Infantry", Category=UnitCategory.Infantry, Code="V"},
            new UnitType(){Name="Guard Infantry", Category=UnitCategory.Infantry, Code="G"},
            new UnitType(){Name="Restricted Infantry", Category=UnitCategory.Infantry, Code="R"},

            new UnitType(){Name="Line Infantry (2 rank)", Category=UnitCategory.Infantry, Code="T"},
            new UnitType(){Name="Light Infantry (2 rank)", Category=UnitCategory.Infantry, Code="U"},
            new UnitType(){Name="Guard Infantry (2 rank)", Category=UnitCategory.Infantry, Code="F"},

            new UnitType(){Name="Independent Skirmisher", Category=UnitCategory.Infantry, Code="S"},
            new UnitType(){Name="Pioneer", Category=UnitCategory.Infantry, Code="P"},
        };

        public static UnitType[] CwbSubTypes = // Civil War Battles
        {
            new UnitType(){Name="Artillery", Category=UnitCategory.Artillery, Code="A"},
            new UnitType(){Name="Cavalry", Category=UnitCategory.Cavalry, Code="C"},
            new UnitType(){Name="Horse Artillery", Category=UnitCategory.Artillery, Code="H"},
            new UnitType(){Name="Infantry", Category=UnitCategory.Infantry, Code="I"},
            new UnitType(){Name="Militia", Category=UnitCategory.Infantry, Code="M"},
            new UnitType(){Name="Infantry (Z)", Category=UnitCategory.Infantry, Code="Z"},
        };
        */

        static Dictionary<string, UnitType[]> SubTypesMap = new()
        {
            {"NB", new UnitType[]{
                new UnitType(){Name="Heavy Artillery", Category=UnitCategory.Artillery, Code="A"},
                new UnitType(){Name="Light Artillery", Category=UnitCategory.Artillery, Code="B"},
                new UnitType(){Name="Horse Artillery", Category=UnitCategory.Artillery, Code="C"},
                new UnitType(){Name="Emplaced Guns", Category=UnitCategory.Artillery, Code="E"},

                new UnitType(){Name="Dragoon", Category=UnitCategory.Cavalry, Code="D"},
                new UnitType(){Name="Light Cavalry", Category=UnitCategory.Cavalry, Code="L"},
                new UnitType(){Name="Heavy Cavalry", Category=UnitCategory.Cavalry, Code="H"},
                new UnitType(){Name="Cossack", Category=UnitCategory.Cavalry, Code="K"},

                new UnitType(){Name="Line Infantry", Category=UnitCategory.Infantry, Code="I"},
                new UnitType(){Name="Militia", Category=UnitCategory.Infantry, Code="M"},
                new UnitType(){Name="Light Infantry", Category=UnitCategory.Infantry, Code="V"},
                new UnitType(){Name="Guard Infantry", Category=UnitCategory.Infantry, Code="G"},
                new UnitType(){Name="Restricted Infantry", Category=UnitCategory.Infantry, Code="R"},

                new UnitType(){Name="Line Infantry (2 rank)", Category=UnitCategory.Infantry, Code="T"},
                new UnitType(){Name="Light Infantry (2 rank)", Category=UnitCategory.Infantry, Code="U"},
                new UnitType(){Name="Guard Infantry (2 rank)", Category=UnitCategory.Infantry, Code="F"},

                new UnitType(){Name="Independent Skirmisher", Category=UnitCategory.Infantry, Code="S"},
                new UnitType(){Name="Pioneer", Category=UnitCategory.Infantry, Code="P"},
            }},
            {"CWB", new UnitType[]{
                new UnitType(){Name="Artillery", Category=UnitCategory.Artillery, Code="A"},
                new UnitType(){Name="Cavalry", Category=UnitCategory.Cavalry, Code="C"},
                new UnitType(){Name="Horse Artillery", Category=UnitCategory.Artillery, Code="H"},
                new UnitType(){Name="Infantry", Category=UnitCategory.Infantry, Code="I"},
                new UnitType(){Name="Militia", Category=UnitCategory.Infantry, Code="M"},
                new UnitType(){Name="Infantry (Z)", Category=UnitCategory.Infantry, Code="Z"},
            }}
        };

        // game series name => unit code => unit type
        public static Dictionary<string, Dictionary<string, UnitType>> Series2Code2Type;// = new Dictionary<string, UnitType>()

        static UnitOob()
        {
            Series2Code2Type = new();
            foreach(var subTypes in SubTypesMap)
            {
                Series2Code2Type[subTypes.Key] = subTypes.Value.ToDictionary(d => d.Code);
            }
        }

        public UnitCategory Category => Type.Category;
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

    public class JTSOobParser
    {
        public Dictionary<string, UnitType> UnitTypeMap; // C# 11: required

        public static JTSOobParser FromCode(string name)
        {
            return new JTSOobParser(){UnitTypeMap = UnitOob.Series2Code2Type[name]};
        }

        public UnitGroup ParseUnits(string s)
        {
            var lines = s.Split("\n");
            Debug.Assert(lines[0].Trim() == "2" || lines[0].Trim() == "3");

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

        public AbstractUnit ParseUnit(Stack<UnitGroup> stack, string line)
        {
            if (stack.Count == 1) // Top unit has 1 extra prefix to define its country
            {
                var s = line.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
                var unit = _ParseUnit(s[1]);
                unit.Country = s[0];
                return unit;
            }
            return _ParseUnit(line);
        }

        AbstractUnit _ParseUnit(string line)
        {
            var ss = line.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
            switch (ss[0])
            {
                case "L":
                    var sss = ss[1].Split((char[])null, 4, StringSplitOptions.RemoveEmptyEntries);
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
                        Type = UnitTypeMap[sss[2]], // TODO: Raise a custom exception notifying user that perhaps a wrong parser is used. 
                        // Type = sss[2],
                        Weapon = sss[3],
                        Icon2D = sss[4],
                        Icon3D = sss[5],
                        Name = sss[6]
                    };
                case "S":
                    sss = ss[1].Split((char[])null, 3, StringSplitOptions.RemoveEmptyEntries);
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

    /*
     *  32---- 1
     * 16/    \2 
     *   \____/
     *  8     4
     */
    public enum UnitDirection
    {
        RightTop = 1,
        Right = 2,
        RightBottom = 4,
        LeftBottom = 8,
        Left = 16,
        LeftTop = 32
    }

    public class UnitState
    {
        public AbstractUnit OobItem;

        public int CurrentStrength;
        public int Fatigue;
        public int X;
        public int Y;

        public int DirectionCode;
        public int Slot;
        public int UsedMovementPoints;
        public int Flags;

        public override string ToString()
        {
            return $"UnitState(X={X}, Y={Y}, CurrentStrength={CurrentStrength}, Fatigue={Fatigue}, DirectionCode={DirectionCode}, Slot={Slot}, UsedMovement={UsedMovementPoints}, Flags={Flags}, {OobItem})";
        }

        public UnitDirection Direction { get => (UnitDirection)DirectionCode; }
    }

    public class JTSUnitStates
    {
        // extract unit state (position, direction, current strength, disorder & fatigue state)
        // from a scenario file (*.scn) or a save file (*.btl, *.bte)

        public List<UnitState> UnitStates = new();
        public Dictionary<AbstractUnit, UnitState> Unit2state = new();

        // 1 2.3.4.4 4 4 393 300 0 4194304 36 23
        // [0]: 1: Dynamic Command Type (Unit Info, reinforcement, ...)
        // [1]: 2.3.4.4: Locate a unit in the OOB file
        // [2]: 4: Direction. 1 => Top-Right, 2=> Right, 4 => Down-Right, 8 => Down-Right, 16 => Left, 32 => Top-Left
        // [3]: 4: Some Sort of "Slot", non-leader units in the same hex will has a unique enum value (1, 2, 4, 8, ...). However I can't see how does it effect gameplay and it's also not the order shown in game.
        // [4]: 393: Current Strength
        // [5]: 300: Fatigue
        // [6]: 0: Used Movement
        // [7]: 4194304: A lot of binary Flags, which encode formation, disorder, formation, isolate and etc... 
        // [8]: X
        // [9]: Y
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

                var unitState = new UnitState()
                {
                    OobItem = unitSelected,
                    CurrentStrength = int.Parse(match.Groups[5].Value),
                    Fatigue = int.Parse(match.Groups[6].Value),
                    X = int.Parse(match.Groups[9].Value),
                    Y = int.Parse(match.Groups[10].Value),
                    DirectionCode = int.Parse(match.Groups[3].Value),
                    Slot = int.Parse(match.Groups[4].Value),
                    UsedMovementPoints = int.Parse(match.Groups[7].Value),
                    Flags = int.Parse(match.Groups[8].Value)
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

                // var ss = s.Trim().Split(" ");
                var ss = s.Trim().Split();

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
                    Y = int.Parse(ss[9]),
                    DirectionCode = int.Parse(ss[2]),
                    Slot = int.Parse(ss[3]),
                    UsedMovementPoints = int.Parse(ss[6]),
                    Flags = int.Parse(ss[7])
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

        public override string ToString()
        {
            return $"Scenario({Name}, ({Year},{Month},{Day},{Hour},{Minute}), {Turn}/{TurnLimit}, DC:{DynamicCommandBlock.Count}, AC:{AICommandBlock.Count})";
        }

        public void Extract(string s)
        {
            var sl = s.Split("\n");
            Name = sl[1].Trim();

            // 1808 10 31 8 0 0 0 1 32
            //  0   1   2 3 4 5 6 7 8
            // var ds = sl[2].Trim().Split(" ").Select(int.Parse).ToArray();
            var ds = sl[2].Trim().Split().Select(int.Parse).ToArray();
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
            var pair = sl[idx].Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
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
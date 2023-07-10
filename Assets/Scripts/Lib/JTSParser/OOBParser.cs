using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;

namespace YYZ.JTS
{

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

        public AbstractUnit Select(string oobIndex)
        {
            // Ex. 1.4.3.4
            var unit = this;
            AbstractUnit unitSelected = null;
            // UnityEngine.Debug.Log($"oobIndex={oobIndex}, s={s}");
            foreach (var idx in oobIndex.Split(".").Select(int.Parse))
            {
                unitSelected = unit.Units[idx - 1];
                unit = unitSelected as UnitGroup;
            }
            return unitSelected;
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

        static Dictionary<JTSSeries, UnitType[]> SubTypesMap = new()
        {
            {JTSSeries.NapoleonicBattle, new UnitType[]{
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
            {JTSSeries.CivilWarBattle, new UnitType[]{
                new UnitType(){Name="Artillery", Category=UnitCategory.Artillery, Code="A"},
                new UnitType(){Name="Cavalry", Category=UnitCategory.Cavalry, Code="C"},
                new UnitType(){Name="Horse Artillery", Category=UnitCategory.Artillery, Code="H"},
                new UnitType(){Name="Infantry", Category=UnitCategory.Infantry, Code="I"},
                new UnitType(){Name="Militia", Category=UnitCategory.Infantry, Code="M"},
                new UnitType(){Name="Infantry (Z)", Category=UnitCategory.Infantry, Code="Z"},
            }}
        };

        // game series => unit code => unit type
        public static Dictionary<JTSSeries, Dictionary<string, UnitType>> Series2Code2Type;// = new Dictionary<string, UnitType>()

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

        public static JTSOobParser FromSeries(JTSSeries series)
        {
            return new JTSOobParser(){UnitTypeMap = UnitOob.Series2Code2Type[series]};
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
                        Type = UnitTypeMap[sss[2]],
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
}
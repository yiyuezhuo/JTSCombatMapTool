using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;

namespace YYZ.JTS.NB
{


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
            if (country == "")
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

    enum UnitType
    {
        Infantry,
        Cavalry,
        Artillery
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
                switch (unitTypeMap[_unit.Type])
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

    class UnitSubType
    {
        public string Name;
        public UnitType Type;
        public string Code;
    }

    static UnitSubType[] unitSubTypes =
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

    static Dictionary<string, UnitType> unitTypeMap;// = new Dictionary<string, UnitType>()

    static UnitGroup()
    {
        unitTypeMap = new Dictionary<string, UnitType>();
        foreach (var unitSubType in unitSubTypes)
        {
            unitTypeMap[unitSubType.Code] = unitSubType.Type;
        }
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
    public string Type;
    public string Weapon;
    public string Icon3D;
    public override string ToString() => $"Unit({Name}, {Strength}, Moraole={Morale}, Type={Type}, Weapon={Weapon})";
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
                    Type = sss[2],
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



}
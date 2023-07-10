namespace YYZ.JTS
{
    using System.Collections.Generic;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;


    public class UnitState
    {
        public AbstractUnit OobItem;

        public int CurrentStrength;
        public int Fatigue;
        public int X;
        public int Y;
        public int I{get => Y;}
        public int J{get => X;}
        public float YCorrected{get => X % 2 == 0 ? Y : Y - 0.5f;}

        public int DirectionCode;
        public int Slot;
        public int UsedMovementPoints;
        public int Flags;

        public override string ToString()
        {
            return $"UnitState(X={X}, Y={Y}, CurrentStrength={CurrentStrength}, Fatigue={Fatigue}, DirectionCode={DirectionCode}, Slot={Slot}, UsedMovement={UsedMovementPoints}, Flags={Flags}, {OobItem})";
        }

        public UnitDirection Direction { get => (UnitDirection)DirectionCode; }

        public float Distance2(UnitState other)
        {
            var dx = other.X - X;
            var dy = other.Y - Y;
            return dx*dx + dy*dy;
        }

        public float Distance2Corrected(UnitState other)
        {
            var dx = other.X - X;
            var dy = other.YCorrected - YCorrected;
            return dx*dx + dy*dy;
        }

        public float Distance(UnitState other) => MathF.Sqrt(Distance2(other));
        public float DistanceCorrected(UnitState other) => MathF.Sqrt(Distance2Corrected(other));
        
        // public float Distance()
    }

    public class JTSUnitStates
    {
        // Extract unit state (position, direction, current strength, disorder & fatigue state)
        // from a scenario file (*.scn) or a save file (*.btl, *.bte)
        public override string ToString()
        {
            return $"JTSUnitStates({UnitStates.Count})";
        }

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

                var unitSelected = oobRoot.Select(oobIndex);

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
                var unitSelected = oobRoot.Select(oobIndex);

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

        public List<Formation> GetBrigadeFormations()
        {
            var formations = new List<Formation>();
            foreach((var group, var states) in GroupByBrigade())
            {
                formations.Add(new Formation(){Group=group, States=states});
            }
            return formations;
        }
    }

    public class Formation // Brigade
    {
        public UnitGroup Group;
        public List<UnitState> States;

        public override string ToString() => $"Formation({Group}, [{States.Count}])";

        public float GetSumAndMean(out float x, out float y)
        {
            x = 0f;
            y = 0f;
            var strengthSum = 0f;
            foreach(var state in States)
            {
                x += state.CurrentStrength * state.X;
                y += state.CurrentStrength * state.Y;
                strengthSum += state.CurrentStrength;
            }
            x /= strengthSum;
            y /= strengthSum;

            return strengthSum;
        }

        public UnitState GetCenterUnitSum(out float strengthSum)
        {
            strengthSum = GetSumAndMean(out var x, out var y);

            UnitState minState = null;
            var minValue = float.MaxValue;
            foreach(var state in States)
            {
                var dx = x - state.X;
                var dy = y - state.Y;
                var d2 = dx * dx + dy * dy;
                if(d2 < minValue)
                {
                    minValue = d2;
                    minState = state;
                }
            }

            return minState;
        }

        /*
        public float Distance2(Formation other)
        {
            return 
        }
        */
    }

    public class JTSTime
    {
        public int Year;
        public int Month;
        public int Day;
        public int Hour;
        public int Minute;
        public override string ToString()
        {
            return $"JTSTime({Year},{Month},{Day},{Hour},{Minute})";
        }
    }

    public enum AIOrderType
    {
        DefendExtreme,
        Defend,
        NoOrder,
        Attack,
        AttackExtreme
    }

    public class AIOrder
    {
        // 1.3 1809 1 16 17 0 18 10 3
        public AbstractUnit Unit;
        public JTSTime Time;
        public int X;
        public int Y;
        public AIOrderType Type;

        public static AIOrder Decode(UnitGroup oobRoot, string s)
        {
            var ret = new AIOrder();
            var sl = s.Split();
            ret.Unit = oobRoot.Select(sl[0]);
            ret.Time = new JTSTime()
            {
                Year = int.Parse(sl[1]),
                Month = int.Parse(sl[2]),
                Day = int.Parse(sl[3]),
                Hour = int.Parse(sl[4]),
                Minute = int.Parse(sl[5]),
            };
            ret.X = int.Parse(sl[6]);
            ret.Y = int.Parse(sl[7]);
            ret.Type = (AIOrderType)int.Parse(sl[8]);

            return ret;
        }

        public override string ToString()
        {
            return $"AIOrder({Unit}, {Time}, {X}, {Y}, {Type})";
        }
    }

    public class AIScript
    {
        public string Name;
        public List<AIOrder> Orders = new();

        public override string ToString()
        {
            return $"AIScript({Name}, {Orders.Count})";
        }
    }

    public abstract class JTSScenario
    {
        public string Name;

        public JTSTime Time;
        public int Turn;
        public int TurnLimit;

        public string MapFile;
        public string OobFile;
        public string PdtFile;

        public List<string> DynamicCommandBlock = new(); // unit states (positions, direction,...), reinforcement
        public List<List<string>> AICommandScripts = new();
        public string Description;

        public string[] Lines;
        public int AIBegin;
        public int AIEnd;

        public List<Objective> Objectives = new();

        public override string ToString()
        {
            return $"{GetType().Name}({Name}, {Time}, {Turn}/{TurnLimit}, DC:{DynamicCommandBlock.Count}, AC:{AICommandScripts.Count}, OB:{Objectives.Count})";
        }

        protected abstract int GetFilesBeginIndex();
        protected abstract int GetDynamicBlockBeginIndex();
        protected abstract void ExtractTime(int[] ds);

        protected abstract char ObjectiveCommandSymbol{get;}

        public void Extract(string s)
        {
            var sl = Lines = s.Split("\n");
            Name = sl[1].Trim();

            // 1808 10 31 8 0 0 0 1 32
            //  0   1   2 3 4 5 6 7 8
            var ds = sl[2].Trim().Split().Select(int.Parse).ToArray();
            ExtractTime(ds);

            var filesBeginIndex = GetFilesBeginIndex();
            MapFile = sl[filesBeginIndex];
            OobFile = sl[filesBeginIndex + 1];
            PdtFile = sl[filesBeginIndex + 2];

            var idx = GetDynamicBlockBeginIndex();
            while (sl[idx][0] != '0')
            {
                if(sl[idx][0] == ObjectiveCommandSymbol) // Objective Command
                {
                    var objective = new Objective();
                    objective.Extract(sl[idx]);
                    Objectives.Add(objective);
                }
                    // Objectives.Add(Objective.Parse(sl[idx]));

                DynamicCommandBlock.Add(sl[idx]);
                idx++;
            }
            idx += 1; // Skip Dynamic Command Block's ending trailing 0

            idx = ParseAIScripts(sl, idx); // point to AI Beginning of AI Scripts (NB/CWB) or Description (PZC)

            var descriptionList = new List<string>();
            for (; idx < sl.Length; idx++)
                descriptionList.Add(sl[idx]);

            Description = string.Join("\n", descriptionList);
        }

        protected abstract int ParseAIScripts(string[] sl, int idx);
    }

    public abstract class Pre1Scenario: JTSScenario
    {
        protected override int GetDynamicBlockBeginIndex() => 13;
        protected override void ExtractTime(int[] ds)
        {
            Time = new JTSTime(){Year = ds[0], Month = ds[1], Day = ds[2], Hour = ds[3], Minute = ds[4]};
            Turn = ds[7];
            TurnLimit = ds[8];
        }
        protected override char ObjectiveCommandSymbol{get=> '7';}
        protected override int ParseAIScripts(string[] sl, int idx) // idx point to dynamic command block's trailing 0
        {
            AIBegin = idx;

            var AIScriptCountAndUnknown = sl[idx].Split();
            var AIScriptCount = int.Parse(AIScriptCountAndUnknown[0]);

            idx += 1;

            for(var i=0; i<AIScriptCount; i++)
            {
                var scriptLines = new List<string>(){sl[idx]};

                var pair = sl[idx].Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
                var aiCommandSize = int.Parse(pair[0]);
                var aiScriptName = pair[1];

                idx += 1;
                for(var j=0; j<aiCommandSize; idx++, j++)
                {
                    scriptLines.Add(sl[idx]);
                }
                AICommandScripts.Add(scriptLines);
            }

            AIEnd = idx;
            return idx;
        }
        
    }

    public class NBScenario: Pre1Scenario
    {
        protected override int GetFilesBeginIndex() => 6;
    }

    public class CWBScenario: Pre1Scenario
    {
        protected override int GetFilesBeginIndex() => 8;
    }

    public class PZCScenario: JTSScenario
    {
        protected override int GetFilesBeginIndex() => 12;
        protected override int GetDynamicBlockBeginIndex() => 15;
        protected override void ExtractTime(int[] ds)
        {
            Time = new JTSTime(){Year = ds[0], Month = ds[1], Day = ds[2], Hour = ds[3]};
            // Current Side and Step Mode (15min vs 10min?) or Night state or PBEM flag? 
            Turn = ds[6];
            TurnLimit = ds[7];
        }
        protected override char ObjectiveCommandSymbol{get=> '6';}
        protected override int ParseAIScripts(string[] sl, int idx)
        {
            return idx; // TODO: Parse AI Scripts from dynamic block
        }
    }

    public class AIStatus
    {
        public List<AIScript> AIScripts = new();

        public void Extract(UnitGroup oobRoot, List<List<string>> aiScripts)
        {
            foreach(var aiScriptLines in aiScripts)
            {
                var pair = aiScriptLines[0].Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
                var name = pair[1];
                var orders = aiScriptLines.Skip(1).Select(line => AIOrder.Decode(oobRoot, line)).ToList();
                var aiScript = new AIScript(){Name = name, Orders=orders};
                AIScripts.Add(aiScript);
            }
        }

        public void GetInvMap(UnitGroup unit, string prefix, Dictionary<AbstractUnit, string> outMap)
        {
            for(var i=0; i<unit.Units.Count; i++)
            {
                var subUnit = unit.Units[i];
                var code = prefix.Length == 0 ? $"{i+1}" : $"{prefix}.{i+1}";
                outMap[subUnit] = code;
                var subGroup = subUnit as UnitGroup;
                if(subGroup != null)
                {
                    GetInvMap(subGroup, code, outMap);
                }
            }
        }

        public Dictionary<AbstractUnit, string> GetInvMap(UnitGroup oobRoot)
        {
            var outMap = new Dictionary<AbstractUnit, string>();
            GetInvMap(oobRoot, "", outMap);
            return outMap;
        }

        public string Transform(JTSScenario scenario, UnitGroup oobRoot)
        {
            var aiLines = new List<string>(){$"{AIScripts.Count} 0"};

            var invMap = GetInvMap(oobRoot);
            foreach(var aiScript in AIScripts)
            {
                aiLines.Add($"{aiScript.Orders.Count} {aiScript.Name}");
                foreach(var order in aiScript.Orders)
                {
                    var idxStr = invMap[order.Unit];
                    var type = (int)order.Type;
                    var t = order.Time;
                    aiLines.Add($"{idxStr} {t.Year} {t.Month} {t.Day} {t.Hour} {t.Minute} {order.X} {order.Y} {type}");
                }
            }

            var lines = scenario.Lines.Take(scenario.AIBegin).Concat(aiLines).Concat(scenario.Lines.Skip(scenario.AIEnd));
            return string.Join("\n", lines);
        }

        public override string ToString()
        {
            return $"AIStatus({AIScripts.Count})";
        }
    }

    public class Objective
    {
        // 7 7 16 200 0
        // 7 11 28 200/20 1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1
        public int X;
        public int Y;
        public int VP; // VP for the occupier once game ends. 
        public string Side; // NB/CWB: int, PZC: string
        public int VPPerTurn1;
        public int VPPerTurn2;
        public int TurnBegin;
        public int TurnEnd;
        public List<string> History = null;

        public int I{get => Y;}
        public int J{get => X;}

        public void Extract(string s)
        {
            // 7 18 10 150 0
            // 7 11 28 200/20 1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1

            // var sl = s.Trim().Split().Skip(1).Select(int.Parse).ToArray();
            var sl = s.Trim().Split();

            X = int.Parse(sl[1]);
            Y = int.Parse(sl[2]);

            VP = 0;
            VPPerTurn1 = 0;
            VPPerTurn2 = 0;

            var VPStr = sl[3];
            var perTurnVPMatch = Regex.Match(VPStr, @"(\d+)/(\d+)");
            if(perTurnVPMatch.Success)
            {
                VPPerTurn1 = int.Parse(perTurnVPMatch.Groups[1].Value);
                VPPerTurn2 = int.Parse(perTurnVPMatch.Groups[2].Value);

                var limitMatch = Regex.Match(VPStr, @"(\d+)-(\d+)");
                if(limitMatch.Success)
                {
                    TurnBegin = int.Parse(limitMatch.Groups[1].Value);
                    TurnEnd = int.Parse(limitMatch.Groups[2].Value);
                }
            }
            else
            {
                VP = int.Parse(VPStr);
            }

            Side = sl[4];
            if(sl.Length > 5)
            {
                History = sl.Skip(5).ToList();
            }
        }

        public override string ToString() => $"Objective({X}, {Y}, VP:{VP}/{VPPerTurn1}/{VPPerTurn2}, Side:{Side}, Hist:[{History.Count}])";
    }
}

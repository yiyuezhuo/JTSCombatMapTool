
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
// using System.Diagnostics;
using YYZ.PathFinding;

namespace YYZ.JTS
{


    public class EdgeState
    {
        public bool[] RawData = new bool[6];
        public bool ByDirection(HexDirection d) => RawData[(int)d];

        public static EdgeState FromIntString(string s)
        {
            // "124" => {True, True, False, True, False, False}
            var ret = new EdgeState();
            foreach(var c in s)
            {
                switch(c)
                {
                    case '1':
                        ret.RawData[0] = true;
                        break;
                    case '2':
                        ret.RawData[1] = true;
                        break;
                    case '3':
                        ret.RawData[2] = true;
                        break;
                    case '4':
                        ret.RawData[3] = true;
                        break;
                    case '5':
                        ret.RawData[4] = true;
                        break;
                    case '6':
                        ret.RawData[5] = true;
                        break;
                }
            }
            return ret;
        }

        public override string ToString()
        {
            var s = string.Join(",", RawData);
            return $"EdgeState({s})";
        }
    }

    // For road, river, wall, ...
    public class EdgeLayer
    {
        public static Dictionary<char, EdgeState> CodeMap;

        static EdgeLayer() // Static Constructors 
        {
            CodeMap = new(){{' ', new EdgeState()}};
            foreach(var line in StaticData.EdgeCodeString.Split("\n"))
            {
                foreach(var record in line.Split("\t"))
                {
                    if(record.Length == 0)
                        continue;
                    var _d_code = record.Split(",", 2);
                    var d = _d_code[0];
                    var code = _d_code[1];
                    if(code == "")
                        continue;
                    CodeMap[code[0]] = EdgeState.FromIntString(d);
                }
            }
        }

        public bool Defined;
        public EdgeState[,] Data;

        public bool HasEdge(int i, int j, HexDirection direction) => Defined && Data[i, j].ByDirection(direction);

        public override string ToString()
        {
            if(!Defined)
                return $"EdgeLayer(Not Defined)";
            return $"EdgeLayer({Data.GetLength(0)}, {Data.GetLength(1)})";
        }
    }

    public class MapLabel
    {
        public double X;
        public double Y;
        public int Size; // 2 => small, 3 => important
        public int V2;
        public int Color; // 0 => Land (Black), 1 => Sea (Blue)
        public string Name;

        public static MapLabel ParseLine(string s)
        {
            // 38.6538 16.6333 2 1 0 Castillo de San Diego
            var ss = s.Split(" ", 6);
            return new MapLabel()
            {
                X = double.Parse(ss[0]),
                Y = double.Parse(ss[1]),
                Size = int.Parse(ss[2]),
                V2 = int.Parse(ss[3]),
                Color = int.Parse(ss[4]),
                Name = ss[5]
            };
        }

        public override string ToString()
        {
            return $"MapLabel({X}, {Y}, {Name})";
        }
    }

    
    public class HexTerrain
    {
        public string Name;
        public char Code;
        public override string ToString()
        {
            return $"HexTerrainType({Name}, '{Code}')";
        }
    }

    public class HexTerrainSystem
    {
        public Dictionary<char, HexTerrain> Code2TerrainMap;
        public Dictionary<string, HexTerrain> Name2TerrainMap;

        public static HexTerrain UnknownTerrain = new(){Code='0', Name="Unknown"};

        public static HexTerrainSystem Parse(string s)
        {
            // ' ' => Clear
            // 'x' => Blocked

            var code2TerrainMap = new Dictionary<char, HexTerrain>(){{UnknownTerrain.Code, UnknownTerrain}};
            var name2TerrainMap = new Dictionary<string, HexTerrain>(){{UnknownTerrain.Name, UnknownTerrain}};
            foreach(Match match in Regex.Matches(s, @"'(.)'\s*=>\s*(\w+)"))
            {
                var code = match.Groups[1].Value[0];
                var name = match.Groups[2].Value;
                var terrain = new HexTerrain(){Code=code, Name=name};
                code2TerrainMap[code] = terrain;
                name2TerrainMap[name] = terrain;
            }
            return new HexTerrainSystem(){Code2TerrainMap=code2TerrainMap, Name2TerrainMap=name2TerrainMap};
        }

        public HexTerrain GetValue(char c) => Code2TerrainMap[c];
        public HexTerrain GetValue(string s) => Name2TerrainMap[s];
        public bool TryGetValue(char c, out HexTerrain o) => Code2TerrainMap.TryGetValue(c, out o);
        public bool TryGetValue(string s, out HexTerrain o) => Name2TerrainMap.TryGetValue(s, out o);
        public int Count{get => Code2TerrainMap.Count;}
        // public bool ContainsKey(string s) => Name2TerrainMap.ContainsKey(s);
        public IEnumerable<HexTerrain> Terrains{get => Code2TerrainMap.Values;}
        public override string ToString()
        {
            var ts = string.Join(",", Terrains);
            return $"HexTerrainSystem({ts})";
        }
    }

    public class EdgeTerrain
    {
        public string Name;
        public EdgeTerrain(string name)
        {
            this.Name = name;
        }
        public override string ToString() => $"EdgeTerrain({Name})";
    }

    public class EdgeTerrainSystem // In general, road and river are 2 different systems.
    {
        public Dictionary<string, EdgeTerrain> Name2Terrain;
        public EdgeTerrainSystem(Dictionary<string, EdgeTerrain> name2Terrain)
        {
            this.Name2Terrain = name2Terrain;
        }
        public EdgeTerrainSystem(IEnumerable<string> names)
        {
            this.Name2Terrain = names.ToDictionary(x=>x, x=>new EdgeTerrain(x));
        }
        public bool TryGetValue(string s, out EdgeTerrain o) => Name2Terrain.TryGetValue(s, out o);
        public EdgeTerrain GetValue(string s) => Name2Terrain[s];
        // public bool ContainsKey(string s) => Name2Terrain.ContainsKey(s);
        public int Count{get => Name2Terrain.Count;}
        public IEnumerable<EdgeTerrain> Terrains{get => Name2Terrain.Values;}
        public override string ToString()
        {
            var ts = string.Join(",", Terrains);
            return $"EdgeTerrainSystem({ts})";
        }
    }

    public abstract class MapFile
    {
        public override string ToString()
        {
            return $"MapFile(Width={Width}, Height={Height}, EdgeLayerMap={EdgeLayerMap.Count}, LabelsN={Labels.Count})";
        }

        public int Version; // ?
        public int Width;
        public int Height;
        // -40 40 0 44370 0
        public HexTerrain[,] TerrainMap;
        public int[,] HeightMap;

        // public List<EdgeLayer> EdgeLayers;
        /*
        Road Layers:
            Path, Road, Pike, Railroad,
        River Layers:
            Stream, Creek
        Other Layers (EX.Wall):
        */

        public List<MapLabel> Labels;

        public TerrainSystem CurrentTerrainSystem;

        static int ParseHeight(char c) // '0' => 0, '1' => 1, ..., 'a' => 10, 'b' => 11, ...
        {
            if(c >= '0' && c <= '9')
                return c - '0';
            return c - 'a' + 10;
        }

        public void Extract(string s, bool strict=true)
        {
            CreateTerrainSystem();

            var lines = s.Split("\n"); // Trim should not be called here since space is used to represent clear terrain.
            Version = int.Parse(lines[0]);

            var sizeStr = lines[1].Split(" "); // "170 168" (NB/CWB) or "346 160 260938" PZC
            Width = int.Parse(sizeStr[0]);
            Height = int.Parse(sizeStr[1]);

            TerrainMap = new HexTerrain[Height, Width];
            HeightMap = new int[Height, Width];

            // Skip some unknown data until terrain matrix
            // TODO: Separated parsers or more robust method
            var matOffset = 2;
            while(int.TryParse(lines[matOffset].Split()[0], out _))
            {
                matOffset++;
            }

            for(int i=0; i<Height; i++)
                for(int j=0; j<Width; j++)
                {
                    var terrainCode = lines[matOffset+i][j];
                    if(!CurrentTerrainSystem.Hex.TryGetValue(terrainCode, out TerrainMap[i, j]))
                    {
                        if(strict)
                            throw new ArgumentException($"Unknown Terrain Code: {terrainCode} in ({i}, {j}), x={j}, y={i}");
                        TerrainMap[i, j] = HexTerrainSystem.UnknownTerrain;
                    }
                    // TerrainMap[i, j] = CurrentTerrainSystem.Hex.GetValue(terrainCode);
                    var hc = lines[matOffset + i + Height][j];
                    HeightMap[i, j] = ParseHeight(hc); // TODO: Performance Issue?
                }

            var idx = ParseEdgeMap(lines, matOffset + Height * 2);

            Labels = new List<MapLabel>();
            while(idx < lines.Length)
            {
                if(lines[idx].Length == 0)
                    break;

                Labels.Add(MapLabel.ParseLine(lines[idx]));
                idx += 1;
            }
        }

        protected abstract void CreateTerrainSystem();

        public Dictionary<EdgeTerrain, EdgeLayer> EdgeLayerMap = new();

        protected abstract int ParseEdgeMap(string[] lines, int idx);

        protected int CreateEdgeLayer(string[] lines, int idx, bool defined, out EdgeLayer edgeLayer)
        {
            edgeLayer = new EdgeLayer(){Defined = defined}; 
            if(defined)
            {
                var data = edgeLayer.Data = new EdgeState[Height, Width];
                for(var i=0; i<Height; i++)
                    for(var j=0; j<Width; j++)
                        data[i, j] = EdgeLayer.CodeMap[lines[idx+i][j]];
                idx += Height;
            }
            return idx;
        }

        protected int ParseEdgeLayerPositioned(string[] lines, int idx, out List<EdgeLayer> edgeLayers)
        {
            // "Position" based method used by NB/CWB
            edgeLayers = new();
            while(idx < lines.Length)
            {
                var test = lines[idx];
                
                if(test.Trim().Length == 1)
                {
                    idx += 1;
                    var defined = int.Parse(test) != 0; // ? false : true;
                    
                    idx = CreateEdgeLayer(lines, idx, defined, out var edgeLayer);
                    edgeLayers.Add(edgeLayer);
                }
                else{
                    break;
                }
            }
            return idx;
        }

        protected int ParseEdgeLayerKeyed(string[] lines, int idx, out Dictionary<char, EdgeLayer> outMap)
        {
            outMap = new();
            while(idx < lines.Length)
            {
                var keyS = lines[idx].Trim();
                if(keyS.Length == 1)
                {
                    var key = keyS[0];
                    if(key == 'x') // Skip two unknoe hex layout
                    {
                        idx += 2 * Height + 2;
                        return idx;
                    }
                    else
                    {
                        idx += 1;
                        idx = CreateEdgeLayer(lines, idx, true, out var edgeLayer);
                        outMap[key] = edgeLayer;
                    }
                }
                else
                {
                    idx += 1;
                    return idx;
                }
            }
            return idx;
        }
    }

    public abstract class PreWW1MapFile: MapFile
    {
        protected override int ParseEdgeMap(string[] lines, int idx)
        {
            // var idx = matOffset + Height * 2;

            idx = ParseEdgeLayerPositioned(lines, idx, out var edgeLayers);
            StoreEdge(edgeLayers);

            return idx;
        }

        protected override void CreateTerrainSystem()
        {
            CurrentTerrainSystem = new TerrainSystem()
            {
                Hex = HexTerrainSystem.Parse(GetTerrainCode()),
                Road = new EdgeTerrainSystem(GetRoadNames()),
                River = new EdgeTerrainSystem(GetRiverNames()),
            };
        }

        protected void StoreEdge(List<EdgeLayer> edgeLayers)
        {
            var roadNames = GetRoadNames();
            var riverNames = GetRiverNames();

            var road = CurrentTerrainSystem.Road;
            var river = CurrentTerrainSystem.River;

            EdgeLayerMap[road.GetValue(roadNames[0])] = edgeLayers[0];
            EdgeLayerMap[road.GetValue(roadNames[1])] = edgeLayers[1];
            EdgeLayerMap[road.GetValue(roadNames[2])] = edgeLayers[2];
            EdgeLayerMap[road.GetValue(roadNames[3])] = edgeLayers[3];
            EdgeLayerMap[river.GetValue(riverNames[0])] = edgeLayers[4];
            EdgeLayerMap[river.GetValue(riverNames[1])] = edgeLayers[5];
        }

        protected abstract string GetTerrainCode();
        protected abstract string[] GetRoadNames();
        protected abstract string[] GetRiverNames();
    }

    public class NBMapFile: PreWW1MapFile
    {
        protected override string GetTerrainCode() => StaticData.NBTerrainCode;
        protected override string[] GetRoadNames() => new string[]{"Path", "Road", "Pike", "Rail"};
        protected override string[] GetRiverNames() => new string[]{"Stream", "Creek"};
    }

    public class CWBMapFile: PreWW1MapFile
    {
        protected override string GetTerrainCode() => StaticData.CWBTerrainCode;
        protected override string[] GetRoadNames() => new string[]{"Trail", "Road", "Pike", "Rail"};
        protected override string[] GetRiverNames() => new string[]{"Stream", "Creek"};
    }

    public class PZCMapFile: MapFile
    {
        protected override void CreateTerrainSystem()
        {
            CurrentTerrainSystem = new TerrainSystem()
            {
                Hex = HexTerrainSystem.Parse(StaticData.PZCTerrainCode),
                Road = new EdgeTerrainSystem(new string[]{"Trail", "Secondary", "Primary", "Rail"}),
                River = new EdgeTerrainSystem(new string[]{"Stream", "Gully", "Canal", "River"}), 
                // TODO: Handle PZC's Bridge
                // Lt Bridge, Med Bridge, Hvy Bridge
            };
        }

        EdgeLayer GetFromKeyed(Dictionary<char, EdgeLayer> dict, char key)
        {
            if(dict.TryGetValue(key, out var ret))
                return ret;
            return new EdgeLayer(){Defined=false};
        }

        protected override int ParseEdgeMap(string[] lines, int idx)
        {
            // var idx = matOffset + Height * 2;

            idx = ParseEdgeLayerKeyed(lines, idx, out var edgeLayersKeyed);

            var road = CurrentTerrainSystem.Road;
            var river = CurrentTerrainSystem.River;
            
            EdgeLayerMap[road.GetValue("Trail")] = GetFromKeyed(edgeLayersKeyed, 't');
            EdgeLayerMap[road.GetValue("Secondary")] = GetFromKeyed(edgeLayersKeyed, 's');
            EdgeLayerMap[road.GetValue("Primary")] = GetFromKeyed(edgeLayersKeyed, 'p');
            EdgeLayerMap[road.GetValue("Rail")] = GetFromKeyed(edgeLayersKeyed, 'r');

            EdgeLayerMap[river.GetValue("Stream")] = GetFromKeyed(edgeLayersKeyed, 'e');
            EdgeLayerMap[river.GetValue("Gully")] = GetFromKeyed(edgeLayersKeyed, 'g');
            EdgeLayerMap[river.GetValue("Canal")] = new EdgeLayer(){Defined=false}; // TODO: Find the key
            EdgeLayerMap[river.GetValue("River")] = new EdgeLayer(){Defined=false}; // TODO: Find the key
            // 'h' => heavy bridge

            return idx;
        }
    }

    public class HexEdge
    {
        public HashSet<EdgeTerrain> RiverSet = new();
        public HashSet<EdgeTerrain> RoadSet = new();

        public bool ContainsRiver(EdgeTerrain e) => RiverSet.Contains(e);
        public bool ContainsRoad(EdgeTerrain e) => RoadSet.Contains(e);
        public void AddRiver(EdgeTerrain e) => RiverSet.Add(e);
        public void AddRoad(EdgeTerrain e) => RoadSet.Add(e);
        
        public override string ToString()
        {
            var river_s = string.Join(",", RiverSet.Select(e => e.Name));
            var road_s = string.Join(",", RoadSet.Select(e => e.Name));
            return $"HexEdge({road_s}, {river_s})";
        }
    }

    public class Hex
    {
        public int I;
        public int J;
        public int X{get => J;}
        public int Y{get => I;}
        public HexTerrain Terrain;
        public int Height;
        public Dictionary<Hex, HexEdge> EdgeMap = new();

        public override string ToString()
        {
            var es = string.Join(",", EdgeMap.Select(KV => $"(I={KV.Key.I}, J={KV.Key.J}): {KV.Value}"));
            return $"Hex(I={I}, J={J}, X={X}, Y={Y}, {Terrain}, {Height}, {es})";
        }
    }

    public class HexNetwork // "Advanced" representation for map
    {
        // (i, j) offsets
        static HexDirection[] directions = new []{HexDirection.Top, HexDirection.TopRight, HexDirection.BottomRight, HexDirection.Bottom, HexDirection.BottomLeft, HexDirection.TopLeft};
        static int[][] evenNeighborOffsets = new int[][]{new int[]{-1, 0}, new int[]{0, 1}, new int[]{1, 1}, new int[]{1, 0}, new int[]{1, -1}, new int[]{0, -1}}; // Follwing HexDirection order
        static int[][] oddNeighborOffsets = new int[][]{new int[]{-1, 0}, new int[]{-1, 1}, new int[]{0, 1}, new int[]{1, 0}, new int[]{0, -1}, new int[]{-1, -1}};

        public Hex[,] HexMat;

        public static HexNetwork FromMapFile(MapFile map)
        {
            var hexMat = new Hex[map.Height, map.Width];
            for(var i=0; i<map.Height; i++)
                for(var j=0; j<map.Width; j++)
                {
                    hexMat[i, j] = new Hex()
                    {
                        I=i, J=j,
                        Terrain=map.TerrainMap[i, j],
                        Height=map.HeightMap[i, j]
                    };
                }

            for(var i=0; i<map.Height; i++)
                for(var j=0; j<map.Width; j++)
                {
                    var src = hexMat[i, j];
                    var offsets = j % 2 == 0 ? evenNeighborOffsets : oddNeighborOffsets;
                    for(var e=0; e<6; e++)
                    {
                        var offset = offsets[e];
                        var direction = directions[e];

                        var ii = i + offset[0];
                        var jj = j + offset[1];
                        if(ii >= 0 && ii < map.Height && jj >= 0 && jj < map.Width)
                        {
                            var dst = hexMat[ii, jj];
                            var edge = src.EdgeMap[dst] = new HexEdge();

                            // TODO: Avoid Hash overhead which is not necessary? Move it to outer level?
                            foreach(var edgeType in map.CurrentTerrainSystem.Road.Name2Terrain.Values)
                            {
                                var edgeLayer = map.EdgeLayerMap[edgeType];
                                if(edgeLayer.HasEdge(i, j, direction))
                                    edge.AddRoad(edgeType);
                            }

                            foreach(var edgeType in map.CurrentTerrainSystem.River.Name2Terrain.Values)
                            {
                                var edgeLayer = map.EdgeLayerMap[edgeType];
                                if(edgeLayer.HasEdge(i, j, direction))
                                    edge.AddRiver(edgeType);
                            }
                        }
                    }
                }

            return new HexNetwork(){HexMat=hexMat};
        }

        bool HasEdge(EdgeLayer layer, int i, int j, HexDirection direction) => layer.Defined && layer.Data[i, j].ByDirection(direction);

        public IEnumerable<Hex> Nodes()
        {
            var height = HexMat.GetLength(0);
            var width = HexMat.GetLength(1);
            for(var i=0; i<height; i++)
            {
                for(var j=0; j<width; j++)
                {
                    yield return HexMat[i, j];
                }
            }
        }

        public List<List<Hex>> SimplifyRoad(EdgeTerrain T)
        {
            var roadMap = new Dictionary<Hex, List<Hex>>();

            foreach(var src in Nodes())
            {
                foreach(var KV in src.EdgeMap)
                {
                    var dst = KV.Key;
                    var edge = KV.Value;
                    if(edge.ContainsRoad(T))
                    {
                        if(roadMap.TryGetValue(src, out var roads))
                        {
                            roads.Add(dst);
                        }
                        else
                        {
                            roadMap[src] = new List<Hex>{dst};
                        }
                    }
                }
            }

            var stationSet = new HashSet<Hex>();
            var relaySet = new HashSet<Hex>();
            foreach(var KV in roadMap)
            {
                if(KV.Value.Count == 2)
                    relaySet.Add(KV.Key);
                else
                    stationSet.Add(KV.Key);
            }

            var ret = new List<List<Hex>>();
            foreach(var station in stationSet)
            {
                foreach(var firstDst in roadMap[station])
                {
                    var left = station;
                    var right = firstDst;
                    var road = new List<Hex>(){left, right};
                    while(relaySet.Contains(right))
                    {
                        var next = roadMap[right][0] == left ? roadMap[right][1] : roadMap[right][0];
                        road.Add(next);
                        left = right;
                        right = next;
                    }

                    ret.Add(road);
                }
            }
            return ret;
        }

        public override string ToString()
        {
            return $"HexNetwork(Height={HexMat.GetLength(0)}, Width={HexMat.GetLength(1)})";
        }
    }

    public class TerrainSystem
    {
        public HexTerrainSystem Hex;
        public EdgeTerrainSystem Road;
        public EdgeTerrainSystem River;
        public override string ToString() => $"TerrainSystem({Hex.Count}, {Road.Count}, {River.Count})";
    }

    public class DistanceSystem
    {
        // Infantry, Motorized, ...
        public string Name; // Motorized, Tracked, ...
        public Dictionary<HexTerrain, float> BaseCostMap = new();
        public Dictionary<EdgeTerrain, float> RoadCostMap = new();
        public Dictionary<EdgeTerrain, float> RiverCostMap = new();

        public void Extract(TerrainSystem ts, Dictionary<string, string> dict)
        {
            // {{"Blocked", "0"}, {"Clear", "3"}, ...}
            foreach((var terrainName, var costStr) in dict)
            {
                var cost = ParseCost(costStr);
                if(ts.Hex.TryGetValue(terrainName, out var terrain))
                {
                    BaseCostMap[terrain] = cost;
                }
                if(ts.Road.TryGetValue(terrainName, out var road))
                {
                    RoadCostMap[road] = cost;
                }
                if(ts.River.TryGetValue(terrainName, out var river))
                {
                    RiverCostMap[river] = cost;
                }
            }
        }

        public int ParseCost(string s)
        {
            if(s.Contains("MP")) // PZC
                return int.Parse(s.Replace("MP", "").Trim());
            return int.Parse(s);
        }

        public override string ToString() => $"DistanceSystem({Name}, {BaseCostMap.Count}, {RoadCostMap.Count}, {RiverCostMap.Count})";

        public float BaseCost(HexTerrain t) => BaseCostMap[t];
        public float RoadCostCost(EdgeTerrain t) => RoadCostMap[t];
        public float RiverCostCost(EdgeTerrain t) => RiverCostMap[t];
    }
    

    public class DistanceGraph: IGraphEnumerable<Hex>
    {
        public HexNetwork Network;
        public DistanceSystem Distance;

        bool HasEdge(EdgeLayer layer, int i, int j, HexDirection direction) => layer.Defined && layer.Data[i, j].ByDirection(direction);

        public IEnumerable<Hex> Neighbors(Hex pos)
        {
            foreach((var nei, var edge) in pos.EdgeMap)
            {
                // if(CurrentDistanceSystem.GetBaseCost(nei.Terrain) > 0 && !edge.Contains(RiverType.Creek))
                if(Distance.BaseCost(nei.Terrain) > 0) // TODO: Handle Cost 0 edge block in some level
                    yield return nei;
            }
        }

        public float MoveCost(Hex src, Hex dst)
        {
            // dst is assumed to be in the EdgeMap
            var edge = src.EdgeMap[dst];

            var roads = Distance.RoadCostMap.Where(KV => edge.ContainsRoad(KV.Key));
            if(roads.Count() > 0) // use road
                return roads.Select(KV => KV.Value).Min();

            var baseCost = Distance.BaseCostMap[dst.Terrain];
            
            var rivers = Distance.RiverCostMap.Where(KV => edge.ContainsRiver(KV.Key));
            var riversCost = rivers.Count() == 0 ? 0 : rivers.Select(KV => KV.Value).Max();

            return baseCost + riversCost;
        }

        public float EstimateCost(Hex src, Hex dst)
        {
            // TODO: even offset?
            var dI = src.I - dst.I;
            var dJ = src.J - dst.J; // Based on Road Cost
            return MathF.Sqrt(dI * dI + dJ * dJ); // TODO: Use Hex Distance (derived from Cube representation) instead of Euclidian distance?
        }

        public override string ToString()
        {
            return $"DistanceGraph({Network}, {Distance})";
        }

        public IEnumerable<Hex> Nodes() => Network.Nodes();

    }
}
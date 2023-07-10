namespace YYZ.AI
{
    using YYZ.PathFinding;
    using System.Collections.Generic;
    using System;


    public class InfluenceMap<T>
    {
        public float Decay;
        public float Momentum;
        public IGraphEnumerable<T> Graph;
        public Dictionary<T, float> InfluenceDict = new();

        public static float Lerp(float initialValue, float finalValue, float weight)
        {
            return initialValue + (finalValue - initialValue) * weight;
        }

        public void Step()
        {
            var newDict = new Dictionary<T, float>();
            foreach (var node in Graph.Nodes())
            {
                var maxInfluence = 0f;
                foreach(var neiNode in Graph.Neighbors(node))
                {
                    var tmpInfluence = InfluenceDict[neiNode] * System.MathF.Exp(-Decay * Graph.MoveCost(node, neiNode));
                    maxInfluence = MathF.Max(tmpInfluence, maxInfluence);
                }
                newDict[node] = Lerp(InfluenceDict[node], maxInfluence, Momentum);
            }
            InfluenceDict = newDict;
        }

        public override string ToString()
        {
            return $"InfluenceMap(Decay={Decay}, Momentum={Momentum}, graph={Graph})";
        }
    }

    public class InfluenceMap2<T>
    {
        public float Alpha;
        public IGraphEnumerable<T> Graph;
        public Dictionary<T, float> InfluenceDict = new();

        public void Stamp()
        {

        }
    }
}

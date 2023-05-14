using System;
using System.Collections.Generic;
using System.Linq;

namespace YYZ.Stats
{
    public static class Clustering
    {
        public static List<List<T>> Cluster<T>(List<T> nodes, Func<T,T,float> dist, float threshold)
        {
            var dists = new List<Tuple<T, T, float>>();

            var groups = new HashSet<List<T>>();
            var groupMap = new Dictionary<T, List<T>>();
            for (var i = 0; i < nodes.Count; i++)
            {
                var group = new List<T>() { nodes[i] };
                groups.Add(group);
                groupMap[nodes[i]] = group;

                for (var j = i + 1; j < nodes.Count; j++)
                {
                    var d = dist(nodes[i], nodes[j]);
                    if (d <= threshold)
                    {
                        dists.Add(new Tuple<T, T, float>(nodes[i], nodes[j], d));
                    }
                }
            }

            dists.Sort((t1, t2) => t1.Item3.CompareTo(t2.Item3));

            foreach(var tuple in dists)
            {
                // UnityEngine.Debug.Log($"{tuple}, groups.Count={groups.Count}");
                var group1 = groupMap[tuple.Item1];
                var group2 = groupMap[tuple.Item2];
                if (group1 == group2)
                    continue;

                group1.AddRange(group2);
                foreach(var node in group2)
                    groupMap[node] = group1;

                groups.Remove(group2);
            }

            return groups.ToList();
        }
    }
}
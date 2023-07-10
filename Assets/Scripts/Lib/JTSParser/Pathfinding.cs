using System.Collections;
using System.Collections.Generic;
using System.Linq;
// using UnityEngine;
// using Godot;
using System;

namespace YYZ.PathFinding
{

    public interface IGeneralGraph<IndexT>
    {
        /// <summary>
        /// src and dst are expected to be neighbor.
        /// </summary>
        float MoveCost(IndexT src, IndexT dst);

        IEnumerable<IndexT> Neighbors(IndexT pos);
    }

    public interface IGraph<IndexT> : IGeneralGraph<IndexT>
    {
        /// <summary>
        /// A heuristic comes from Euclidean space or something like that.
        /// </summary>
        float EstimateCost(IndexT src, IndexT dst);
    }

    public interface IGraphEnumerable<IndexT> : IGraph<IndexT>
    {
        IEnumerable<IndexT> Nodes();
    }


    public static class PathFinding<IndexT>
    {
        static List<IndexT> ReconstructPath(Dictionary<IndexT, IndexT> cameFrom, IndexT current)
        {
            var total_path = new List<IndexT> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                total_path.Add(current);
            }
            total_path.Reverse();
            return total_path;
        }

        static float TryGet(Dictionary<IndexT, float> dict, IndexT key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return float.PositiveInfinity;
        }

        /// <summary>
        /// Path finding using A* algorithm, if failed it will return a empty list.
        /// </summary>
        public static List<IndexT> AStar(IGraph<IndexT> graph, IndexT src, IndexT dst)
        {
            var openSet = new HashSet<IndexT> { src };
            var cameFrom = new Dictionary<IndexT, IndexT>();

            var gScore = new Dictionary<IndexT, float> { { src, 0 } }; // default Mathf.Infinity

            var fScore = new Dictionary<IndexT, float> { { src, graph.EstimateCost(src, dst) } };

            while (openSet.Count > 0)
            {
                IEnumerator<IndexT> openSetEnumerator = openSet.GetEnumerator();

                openSetEnumerator.MoveNext(); // assert?
                IndexT current = openSetEnumerator.Current;
                float lowest_f_score = fScore[current];

                while (openSetEnumerator.MoveNext())
                {
                    IndexT pos = openSetEnumerator.Current;
                    if (fScore[pos] < lowest_f_score)
                    {
                        lowest_f_score = TryGet(fScore, pos);
                        current = pos;
                    }
                }

                if (current.Equals(dst))
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);
                foreach (IndexT neighbor in graph.Neighbors(current))
                {
                    float tentative_gScore = TryGet(gScore, current) + graph.MoveCost(current, neighbor);
                    if (tentative_gScore < TryGet(gScore, neighbor))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative_gScore;
                        fScore[neighbor] = TryGet(gScore, neighbor) + graph.EstimateCost(neighbor, dst);

                        openSet.Add(neighbor);
                    }
                }
            }
            return new List<IndexT>(); // failure
        }

        public class DijkstraResult
        {
            public Dictionary<IndexT, Path> nodeToPath;
            public IndexT pickedNode;

            public List<IndexT> Reconstruct(IndexT node)
            {
                var p = node;
                var ret = new List<IndexT>();
                while (p != null)
                {
                    ret.Add(p);
                    p = nodeToPath[p].prev;
                }
                ret.Reverse();
                return ret;
            }

            public float Cost(IndexT node) => nodeToPath[node].cost;
        }

        public static DijkstraResult Dijkstra(IGeneralGraph<IndexT> graph, IEnumerable<IndexT> srcIter, Func<IndexT, bool> Predicate, float budget)
        {
            var ret = new DijkstraResult();

            var nodeToPath = ret.nodeToPath = new Dictionary<IndexT, Path>();
            var closeSet = srcIter.ToHashSet();
            var openSet = new HashSet<IndexT>();
            foreach(var closed in closeSet)
            {
                foreach (var node in graph.Neighbors(closed))
                    openSet.Add(node);
                nodeToPath[closed] = new Path();
            }

            while(openSet.Count > 0)
            {
                // pick
                /*
                openSet.Min(node => 
                    graph.Neighbors(node).Where(nei => closeSet.Contains(nei)).Select(nei => 
                        graph.MoveCost(node, nei)
                    ).Min()
                );
                */

                IndexT pickedNode = default(IndexT);
                IndexT pickedClosedNei = default(IndexT);
                Path pickedPath = null;
                float pickedCost = -1;

                bool picked = false;

                foreach(var openNode in openSet)
                {
                    foreach(var closedNei in graph.Neighbors(openNode).Where(nei => closeSet.Contains(nei)))
                    {
                        var path = nodeToPath[closedNei];
                        var cost = graph.MoveCost(openNode, closedNei) + path.cost;

                        if(!picked || cost < pickedCost)
                        {
                            picked = true;

                            pickedNode = openNode;
                            pickedClosedNei = closedNei;
                            pickedPath = path;
                            pickedCost = cost;
                        }
                    }
                }

                // Asymmetric Graph may raise exception here.
                nodeToPath[pickedNode] = new Path() { cost = pickedCost, prev = pickedClosedNei };

                if (Predicate(pickedNode))
                {
                    ret.pickedNode = pickedNode;
                    break;
                }

                openSet.Remove(pickedNode);
                closeSet.Add(pickedNode);

                if (budget - pickedCost <= 0) // We allows the value to be negative, but it will not be allowed to "propagate".
                    continue;

                foreach(var nei in graph.Neighbors(pickedNode).Where(nei => !closeSet.Contains(nei)))
                {
                    openSet.Add(nei);
                }
            }
            return ret;
        }

        public static DijkstraResult GetReachable(IGeneralGraph<IndexT> graph, IndexT src, float budget)
        {
            var srcIter = new IndexT[] { src };
            return Dijkstra(graph, srcIter, DummyFalsePredicate, budget);
        }

        static bool DummyFalsePredicate(IndexT node) => false;

        public static List<IndexT> ExploreNearestTarget(IGeneralGraph<IndexT> graph, IndexT src, Func<IndexT, bool> Predicate)
        {
            var srcIter = new IndexT[] { src };
            var result = Dijkstra(graph, srcIter, Predicate, float.PositiveInfinity);
            return result.Reconstruct(result.pickedNode);
        }

        public class Path
        {
            public IndexT prev;
            public float cost;

            public override string ToString() => $"Path({prev}, {cost})";
        }

        public class PathComparer : IComparer<IndexT>
        {
            Dictionary<IndexT, Path> nodeToPath;
            public PathComparer(Dictionary<IndexT, Path> nodeToPath)
            {
                this.nodeToPath = nodeToPath;
            }
            public int Compare(IndexT x, IndexT y)
            {
                return nodeToPath[x].cost.CompareTo(nodeToPath[y].cost);
            }
        }

        /// <summary>
        /// ccw: ccw > 0 if three points make a counter-clockwise turn, clockwise if ccw < 0, and collinear if ccw = 0.
        /// </summary>
        static float ccw(System.Numerics.Vector2 p1, System.Numerics.Vector2 p2, System.Numerics.Vector2 p3)
        {
            var p1p2 = p2 - p1;
            var p1p3 = p3 - p1;
            return p1p2.X * p1p3.Y - p1p2.Y * p1p3.X;
        }

        static float Dot(System.Numerics.Vector2 a, System.Numerics.Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        static float CosAngle(System.Numerics.Vector2 a, System.Numerics.Vector2 b)
        {
            var p = Dot(a, b);
            var angle = Dot(a, b) / Math.Sqrt(Dot(a, a)) / Math.Sqrt(Dot(b, b));
            return (float)angle;
        }

        class GramScanComparer : IComparer<System.Numerics.Vector2>
        {
            Dictionary<System.Numerics.Vector2, float> angleMap = new Dictionary<System.Numerics.Vector2, float>();
            System.Numerics.Vector2 p0;

            public GramScanComparer(IEnumerable<System.Numerics.Vector2> points, System.Numerics.Vector2 p0)
            {
                foreach (var p in points)
                    angleMap[p] = CosAngle(p0, p);
                this.p0 = p0;
            }
            public int Compare(System.Numerics.Vector2 a, System.Numerics.Vector2 b)
            {
                var ret = angleMap[a].CompareTo(angleMap[b]);
                if (ret == 0)
                    return Dot(a - p0, a - p0).CompareTo(Dot(b - p0, b - p0));
                return ret;
            }
        }

        public static Stack<System.Numerics.Vector2> GrahamScan(IEnumerable<System.Numerics.Vector2> pointIter)
        {
            // https://en.wikipedia.org/wiki/Graham_scan

            var p0 = new System.Numerics.Vector2(float.PositiveInfinity, float.PositiveInfinity);
            foreach (var point in pointIter)
                if (point.Y < p0.Y || (point.Y == p0.Y && point.X < p0.X))
                    p0 = point;

            var points = pointIter.ToList();
            points.Sort(new GramScanComparer(points, p0));

            var stackWithoutTop = new Stack<System.Numerics.Vector2>(); // We don't include top in the stack, since it's not convenient to peek second element of this stack implementation.
            var top = points.First();

            foreach (var point in points.Skip(1))
            {
                while (stackWithoutTop.Count >= 1 && ccw(stackWithoutTop.Peek(), top, point) <= 0)
                    top = stackWithoutTop.Pop();

                stackWithoutTop.Push(top);
                top = point;
            }

            stackWithoutTop.Push(top); // The stack is with top from now.
            var stack = stackWithoutTop;

            return stack;
        }

        public class RegionConvexHull
        {
            public List<IndexT> boundaries;
            public HashSet<IndexT> boundariesSet;
            Func<IndexT, System.Numerics.Vector2> CenterFor;

            public RegionConvexHull(List<IndexT> boundaries, HashSet<IndexT> boundariesSet, Func<IndexT, System.Numerics.Vector2> CenterFor)
            {
                this.boundaries = boundaries;
                this.boundariesSet = boundariesSet;
                this.CenterFor = CenterFor;
            }

            public bool IsInside(IndexT region) // Need checks
            {
                if (boundaries.Count <= 1)
                    return region.Equals(boundaries[0]);

                var tail = boundaries.First();
                foreach (var head in boundaries.Skip(1))
                    if (ccw(CenterFor(tail), CenterFor(head), CenterFor(region)) <= 0)
                        return false;
                return true;
            }

        }

        public static RegionConvexHull RegionConvexHullFor(IGraph<IndexT> graph, IEnumerable<IndexT> regions, Func<IndexT, System.Numerics.Vector2> CenterFor)
        {
            var center2region = new Dictionary<System.Numerics.Vector2, IndexT>();
            foreach (var region in regions)
                center2region[CenterFor(region)] = region;

            var stack = GrahamScan(center2region.Keys);
            var src = center2region[stack.Pop()];
            var boundariesSet = new HashSet<IndexT>() { src };
            var boundaries = new List<IndexT>() { src };

            while (stack.Count > 0)
            {
                var dst = center2region[stack.Pop()];
                foreach (var region in AStar(graph, src, dst))
                    if (!boundariesSet.Contains(region))
                    {
                        boundariesSet.Add(region);
                        boundaries.Add(region);
                    }

                src = dst;
            }

            boundaries.Reverse();

            return new RegionConvexHull(boundaries, boundariesSet, CenterFor);
        }


        public static List<IndexT> RegionConvexHullWrapper(IGraph<IndexT> graph, IEnumerable<IndexT> regions, Func<IndexT, System.Numerics.Vector2> CenterFor)
        {
            var hull = RegionConvexHullFor(graph, regions, CenterFor);
            var set = new HashSet<IndexT>();
            foreach (var b in hull.boundaries)
                foreach (var region in graph.Neighbors(b))
                    if (!hull.boundariesSet.Contains(region) && !hull.IsInside(region))
                        set.Add(region);

            /*
            if(set.Count == 0)
                System.Console.WriteLine("WTF");
            */

            return set.ToList();
        }
    }

    /*
    public class GraphSegmentation<IndexT>
    {
        static void Segmentation(IGraphEnumerable<IndexT> graph, float costThreshold)
        {
            var remaining = graph.Nodes().ToHashSet();
            var clusters = new List<HashSet<IndexT>>();
            while(remaining.Count > 0)
            {
                var clusterFirst = remaining.First();
                var cluster = new HashSet<IndexT>(){clusterFirst};
                var openSet = new HashSet<IndexT>(){clusterFirst};
                var closedSet = new HashSet<IndexT>();
                while(openSet.Count > 0)
                {
                    var node = openSet.First();
                    openSet.Remove(node);
                    foreach(var neiNode in graph.Neighbors(node))
                    {
                        if(cluster.Contains(neiNode))
                            continue;
                        foreach(var testNode in cluster)
                        {
                            graph.Mo
                        }
                    }
                }
            }
        }
        
    }
    */
}
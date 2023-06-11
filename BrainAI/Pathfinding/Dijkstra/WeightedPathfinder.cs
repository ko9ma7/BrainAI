﻿namespace BrainAI.Pathfinding
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// calculates paths given an IWeightedGraph and start/goal positions
    /// </summary>
    public class WeightedPathfinder<T> : IPathfinder<T>, ICoveragePathfinder<T>
    {
        public Dictionary<T, T> VisitedNodes { get; } = new Dictionary<T, T>();

        private readonly HashSet<T> tmpGoals = new HashSet<T>();

        private T searchStart;

        private readonly IWeightedGraph<T> graph;

        private readonly List<T> resultPath = new List<T>();

        private readonly Dictionary<T, int> costSoFar = new Dictionary<T, int>();

        private readonly List<ValueTuple<int, T>> frontier = new List<ValueTuple<int, T>>();

        private static readonly Comparison<(int, T)> Comparison = (x, y) => x.Item1 - y.Item1;

        public WeightedPathfinder(IWeightedGraph<T> graph)
        {
            this.graph = graph;
        }

        public List<T> Search(T start, T goal)
        {
            this.PrepareSearch();
            this.StartNewSearch(start);

            tmpGoals.Add(goal);

            return ContinueSearch();
        }

        public List<T> Search(T start, HashSet<T> goals)
        {
            this.PrepareSearch();
            this.StartNewSearch(start);

            foreach (var goal in goals)
            {
                this.tmpGoals.Add(goal);
            }

            return ContinueSearch();
        }

        public void Search(T start, int maxPathWeight)
        {
            this.PrepareSearch();
            this.StartNewSearch(start);

            InternalSearch(maxPathWeight);
        }

        public List<T> Search(T start, HashSet<T> goals, int maxPathWeight)
        {
            this.PrepareSearch();
            this.StartNewSearch(start);

            foreach (var goal in goals)
            {
                this.tmpGoals.Add(goal);
            }

            return ContinueSearch(maxPathWeight);
        }

        public List<T> ContinueSearch()
        {
            if (tmpGoals.Count == 0)
            {
                return null;
            }
            
            return ContinueSearch(int.MaxValue);
        }

        public List<T> ContinueSearch(int maxPathWeight)
        {
            var (target, result) = InternalSearch(maxPathWeight);
            return this.BuildPath(target, result);
        }

        private ValueTuple<T, bool> InternalSearch(int additionalDepth)
        {
            if (frontier.Count > 0 && additionalDepth < int.MaxValue - frontier[0].Item1)
            {
                additionalDepth += frontier[0].Item1;
            }

            while (frontier.Count > 0)
            {
                var current = frontier[0];

                if (current.Item1 >= additionalDepth)
                {
                    break;
                }

                if (tmpGoals.Contains(current.Item2))
                {
                    tmpGoals.Remove(current.Item2);
                    return (current.Item2, true);
                }

                frontier.RemoveAt(0);

                foreach (var next in graph.GetNeighbors(current.Item2))
                {
                    var newCost = costSoFar[current.Item2] + graph.Cost(current.Item2, next);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost;
                        frontier.Add(new ValueTuple<int, T>(priority, next));
                        VisitedNodes[next] = current.Item2;
                    }
                }

                frontier.Sort(Comparison);
            }

            return (default(T), false);
        }

        private void PrepareSearch()
        {
            this.frontier.Clear();
            this.VisitedNodes.Clear();
            this.tmpGoals.Clear();
            this.costSoFar.Clear();
        }

        private void StartNewSearch(T start)
        {
            this.searchStart = start;
            this.frontier.Add(new ValueTuple<int, T>(0, start));
            this.VisitedNodes.Add(start, start);
            this.costSoFar[start] = 0;
        }

        private List<T> BuildPath(T target, bool result)
        {
            if (!result)
            {
                return null;
            }

            PathConstructor.RecontructPath(VisitedNodes, searchStart, target, resultPath);
            return resultPath;
        }
    }
}

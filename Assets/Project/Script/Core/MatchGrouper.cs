using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public static class MatchGrouper
    {
        public static IReadOnlyList<MatchGroup> Group(IReadOnlyList<Match> matches)
        {
            if (matches == null)
            {
                throw new ArgumentNullException(nameof(matches));
            }

            List<MatchGroup> groups = new();
            bool[] groupedMatches = new bool[matches.Count];

            for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
            {
                if (groupedMatches[matchIndex])
                {
                    continue;
                }

                List<Match> groupMatches = new();
                Queue<int> matchesToVisit = new();
                groupedMatches[matchIndex] = true;
                matchesToVisit.Enqueue(matchIndex);

                while (matchesToVisit.Count > 0)
                {
                    int currentIndex = matchesToVisit.Dequeue();
                    Match currentMatch = matches[currentIndex];
                    groupMatches.Add(currentMatch);

                    for (int candidateIndex = 0; candidateIndex < matches.Count; candidateIndex++)
                    {
                        if (groupedMatches[candidateIndex])
                        {
                            continue;
                        }

                        Match candidateMatch = matches[candidateIndex];
                        if (candidateMatch.TileType == currentMatch.TileType &&
                            Intersects(currentMatch, candidateMatch))
                        {
                            groupedMatches[candidateIndex] = true;
                            matchesToVisit.Enqueue(candidateIndex);
                        }
                    }
                }

                groups.Add(new MatchGroup(groupMatches, Classify(groupMatches)));
            }

            return groups;
        }

        private static MatchShape Classify(IReadOnlyList<Match> matches)
        {
            if (matches.Count == 1)
            {
                return MatchShape.Straight;
            }

            if (matches.Count != 2 || matches[0].Orientation == matches[1].Orientation)
            {
                return MatchShape.Complex;
            }

            if (!TryGetSingleIntersection(matches[0], matches[1], out Vector2Int intersection))
            {
                return MatchShape.Complex;
            }

            bool firstIntersectsAtEndpoint = IsEndpoint(matches[0], intersection);
            bool secondIntersectsAtEndpoint = IsEndpoint(matches[1], intersection);

            if (firstIntersectsAtEndpoint && secondIntersectsAtEndpoint)
            {
                return MatchShape.L;
            }

            if (firstIntersectsAtEndpoint || secondIntersectsAtEndpoint)
            {
                return MatchShape.T;
            }

            return MatchShape.Cross;
        }

        private static bool Intersects(Match first, Match second)
        {
            for (int firstIndex = 0; firstIndex < first.Cells.Count; firstIndex++)
            {
                for (int secondIndex = 0; secondIndex < second.Cells.Count; secondIndex++)
                {
                    if (first.Cells[firstIndex] == second.Cells[secondIndex])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryGetSingleIntersection(
            Match first,
            Match second,
            out Vector2Int intersection)
        {
            intersection = default;
            int intersectionCount = 0;

            for (int firstIndex = 0; firstIndex < first.Cells.Count; firstIndex++)
            {
                for (int secondIndex = 0; secondIndex < second.Cells.Count; secondIndex++)
                {
                    if (first.Cells[firstIndex] != second.Cells[secondIndex])
                    {
                        continue;
                    }

                    intersection = first.Cells[firstIndex];
                    intersectionCount++;
                }
            }

            return intersectionCount == 1;
        }

        private static bool IsEndpoint(Match match, Vector2Int cell)
        {
            return match.Cells[0] == cell || match.Cells[match.Cells.Count - 1] == cell;
        }
    }
}

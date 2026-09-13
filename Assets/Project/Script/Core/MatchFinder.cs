using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public static class MatchFinder
    {
        public static IReadOnlyList<MatchPattern> FindMatches(Board board)
        {
            List<MatchRun> runs = FindRuns(board);
            return GroupRuns(runs);
        }

        public static MatchResult FindSwapMatch(Board board, Vector2Int from, Vector2Int to)
        {
            IReadOnlyList<MatchPattern> patterns = FindMatches(board);
            if (patterns.Count > 0)
            {
                return new MatchResult(patterns, null);
            }

            Tile firstTile = board[from.x, from.y];
            Tile secondTile = board[to.x, to.y];
            bool isColorBombNormal =
                (firstTile.Special == SpecialType.ColorBomb && IsNormalColorTile(secondTile)) ||
                (secondTile.Special == SpecialType.ColorBomb && IsNormalColorTile(firstTile));
            return new MatchResult(patterns,
                isColorBombNormal ? new SpecialMatch(from, to) : null);
        }

        private static bool IsNormalColorTile(Tile tile)
        {
            return !tile.IsEmpty && tile.Special == SpecialType.None && tile.Color >= 0;
        }

        private static List<MatchRun> FindRuns(Board board)
        {
            List<MatchRun> runs = new();

            for (int y = 0; y < board.Height; y++)
            {
                int x = 0;
                while (x < board.Width)
                {
                    if (board[x, y].IsEmpty)
                    {
                        x++;
                        continue;
                    }

                    int runStart = x;
                    int color = board[x, y].Color;

                    x++;
                    while (x < board.Width &&
                           !board[x, y].IsEmpty &&
                           board[x, y].Color == color)
                    {
                        x++;
                    }

                    int runLength = x - runStart;
                    if (runLength >= 3)
                    {
                        List<Vector2Int> cells = new(runLength);
                        for (int runX = runStart; runX < x; runX++)
                        {
                            cells.Add(new Vector2Int(runX, y));
                        }

                        runs.Add(new MatchRun(color, RunOrientation.Horizontal, cells));
                    }
                }
            }

            for (int x = 0; x < board.Width; x++)
            {
                int y = 0;
                while (y < board.Height)
                {
                    if (board[x, y].IsEmpty)
                    {
                        y++;
                        continue;
                    }

                    int runStart = y;
                    int color = board[x, y].Color;

                    y++;
                    while (y < board.Height &&
                           !board[x, y].IsEmpty &&
                           board[x, y].Color == color)
                    {
                        y++;
                    }

                    int runLength = y - runStart;
                    if (runLength >= 3)
                    {
                        List<Vector2Int> cells = new(runLength);
                        for (int runY = runStart; runY < y; runY++)
                        {
                            cells.Add(new Vector2Int(x, runY));
                        }

                        runs.Add(new MatchRun(color, RunOrientation.Vertical, cells));
                    }
                }
            }

            return runs;
        }

        private static IReadOnlyList<MatchPattern> GroupRuns(IReadOnlyList<MatchRun> runs)
        {
            List<MatchPattern> patterns = new();
            bool[] groupedRuns = new bool[runs.Count];

            for (int runIndex = 0; runIndex < runs.Count; runIndex++)
            {
                if (groupedRuns[runIndex])
                {
                    continue;
                }

                List<MatchRun> patternRuns = new();
                Queue<int> runsToVisit = new();
                groupedRuns[runIndex] = true;
                runsToVisit.Enqueue(runIndex);

                while (runsToVisit.Count > 0)
                {
                    int currentIndex = runsToVisit.Dequeue();
                    MatchRun currentRun = runs[currentIndex];
                    patternRuns.Add(currentRun);

                    for (int candidateIndex = 0; candidateIndex < runs.Count; candidateIndex++)
                    {
                        if (groupedRuns[candidateIndex])
                        {
                            continue;
                        }

                        MatchRun candidateRun = runs[candidateIndex];
                        if (candidateRun.Color == currentRun.Color &&
                            Intersects(currentRun, candidateRun))
                        {
                            groupedRuns[candidateIndex] = true;
                            runsToVisit.Enqueue(candidateIndex);
                        }
                    }
                }

                List<Vector2Int> patternCells = new();
                for (int patternRunIndex = 0; patternRunIndex < patternRuns.Count; patternRunIndex++)
                {
                    patternCells.AddRange(patternRuns[patternRunIndex].Cells);
                }

                patterns.Add(new MatchPattern(
                    patternRuns[0].Color,
                    Classify(patternRuns),
                    patternCells));
            }

            return patterns;
        }

        private static MatchShape Classify(IReadOnlyList<MatchRun> runs)
        {
            if (runs.Count == 1)
            {
                return MatchShape.Straight;
            }

            if (runs.Count != 2 || runs[0].Orientation == runs[1].Orientation)
            {
                return MatchShape.Complex;
            }

            if (!TryGetSingleIntersection(runs[0], runs[1], out Vector2Int intersection))
            {
                return MatchShape.Complex;
            }

            bool firstIntersectsAtEndpoint = IsEndpoint(runs[0], intersection);
            bool secondIntersectsAtEndpoint = IsEndpoint(runs[1], intersection);

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

        private static bool Intersects(MatchRun first, MatchRun second)
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
            MatchRun first,
            MatchRun second,
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

        private static bool IsEndpoint(MatchRun run, Vector2Int cell)
        {
            return run.Cells[0] == cell || run.Cells[run.Cells.Count - 1] == cell;
        }

        private enum RunOrientation
        {
            Horizontal,
            Vertical
        }

        private sealed class MatchRun
        {
            internal int Color { get; }
            internal RunOrientation Orientation { get; }
            internal IReadOnlyList<Vector2Int> Cells { get; }

            internal MatchRun(
                int color,
                RunOrientation orientation,
                IReadOnlyList<Vector2Int> cells)
            {
                Color = color;
                Orientation = orientation;
                Cells = cells;
            }
        }
    }
}

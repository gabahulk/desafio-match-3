using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public static class MatchFinder
    {
        public static IReadOnlyList<Match> FindMatches(Board board)
        {
            List<Match> matches = new();

            for (int y = 0; y < board.Height; y++)
            {
                int x = 0;
                while (x < board.Width)
                {
                    int runStart = x;
                    int tileType = board[x, y].Type;

                    x++;
                    while (x < board.Width && board[x, y].Type == tileType)
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

                        matches.Add(new Match(tileType, MatchOrientation.Horizontal, cells));
                    }
                }
            }

            for (int x = 0; x < board.Width; x++)
            {
                int y = 0;
                while (y < board.Height)
                {
                    int runStart = y;
                    int tileType = board[x, y].Type;

                    y++;
                    while (y < board.Height && board[x, y].Type == tileType)
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

                        matches.Add(new Match(tileType, MatchOrientation.Vertical, cells));
                    }
                }
            }

            return matches;
        }
    }
}

using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;

namespace Gazeus.DesafioMatch3.Core
{
    internal static class MatchFinder
    {
        internal static List<List<bool>> FindMatches(Board board)
        {
            List<List<bool>> matchedTiles = new();
            for (int y = 0; y < board.Height; y++)
            {
                matchedTiles.Add(new List<bool>(board.Width));
                for (int x = 0; x < board.Width; x++)
                {
                    matchedTiles[y].Add(false);
                }
            }

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    if (x > 1 &&
                        board[x, y].Type == board[x - 1, y].Type &&
                        board[x - 1, y].Type == board[x - 2, y].Type)
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y][x - 1] = true;
                        matchedTiles[y][x - 2] = true;
                    }

                    if (y > 1 &&
                        board[x, y].Type == board[x, y - 1].Type &&
                        board[x, y - 1].Type == board[x, y - 2].Type)
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y - 1][x] = true;
                        matchedTiles[y - 2][x] = true;
                    }
                }
            }

            return matchedTiles;
        }

        internal static bool HasMatch(List<List<bool>> matchedTiles)
        {
            for (int y = 0; y < matchedTiles.Count; y++)
            {
                for (int x = 0; x < matchedTiles[y].Count; x++)
                {
                    if (matchedTiles[y][x])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

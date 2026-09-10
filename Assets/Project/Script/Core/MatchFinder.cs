using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;

namespace Gazeus.DesafioMatch3.Core
{
    internal static class MatchFinder
    {
        internal static List<List<bool>> FindMatches(List<List<Tile>> board)
        {
            List<List<bool>> matchedTiles = new();
            for (int y = 0; y < board.Count; y++)
            {
                matchedTiles.Add(new List<bool>(board[y].Count));
                for (int x = 0; x < board[y].Count; x++)
                {
                    matchedTiles[y].Add(false);
                }
            }

            for (int y = 0; y < board.Count; y++)
            {
                for (int x = 0; x < board[y].Count; x++)
                {
                    if (x > 1 &&
                        board[y][x].Type == board[y][x - 1].Type &&
                        board[y][x - 1].Type == board[y][x - 2].Type)
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y][x - 1] = true;
                        matchedTiles[y][x - 2] = true;
                    }

                    if (y > 1 &&
                        board[y][x].Type == board[y - 1][x].Type &&
                        board[y - 1][x].Type == board[y - 2][x].Type)
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

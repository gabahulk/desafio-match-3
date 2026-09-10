using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;

namespace Gazeus.DesafioMatch3.Tests.EditMode.Fixtures
{
    internal static class BoardFixture
    {
        public static void Apply(List<List<Tile>> board, params string[] rows)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            if (rows.Length != board.Count)
            {
                throw new ArgumentException(
                    $"Expected {board.Count} rows, but received {rows.Length}.",
                    nameof(rows));
            }

            int[][] tileTypes = new int[rows.Length][];
            for (int y = 0; y < rows.Length; y++)
            {
                if (rows[y] == null || rows[y].Length != board[y].Count)
                {
                    throw new ArgumentException(
                        $"Row {y} must contain exactly {board[y].Count} symbols.",
                        nameof(rows));
                }

                tileTypes[y] = new int[rows[y].Length];
                for (int x = 0; x < rows[y].Length; x++)
                {
                    tileTypes[y][x] = GetTileType(rows[y][x], x, y);
                }
            }

            for (int y = 0; y < tileTypes.Length; y++)
            {
                for (int x = 0; x < tileTypes[y].Length; x++)
                {
                    board[y][x].Type = tileTypes[y][x];
                }
            }
        }

        private static int GetTileType(char symbol, int x, int y)
        {
            switch (symbol)
            {
                case 'R':
                    return 0;
                case 'G':
                    return 1;
                case 'B':
                    return 2;
                case 'Y':
                    return 3;
                default:
                    throw new ArgumentException(
                        $"Unsupported board symbol '{symbol}' at ({x}, {y}).",
                        nameof(symbol));
            }
        }
    }
}

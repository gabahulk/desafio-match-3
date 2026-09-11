using System;
using Gazeus.DesafioMatch3.Models;

namespace Gazeus.DesafioMatch3.Tests.EditMode.Fixtures
{
    internal static class BoardFixture
    {
        public static Board Create(params string[] rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            if (rows.Length == 0)
            {
                throw new ArgumentException("At least one row is required.", nameof(rows));
            }

            if (rows[0] == null)
            {
                throw new ArgumentException("Row 0 cannot be null.", nameof(rows));
            }

            int width = rows[0].Length;
            int[][] tileTypes = new int[rows.Length][];
            for (int y = 0; y < rows.Length; y++)
            {
                if (rows[y] == null || rows[y].Length != width)
                {
                    throw new ArgumentException(
                        $"Row {y} must contain exactly {width} symbols.",
                        nameof(rows));
                }

                tileTypes[y] = new int[width];
                for (int x = 0; x < width; x++)
                {
                    tileTypes[y][x] = GetTileType(rows[y][x], x, y);
                }
            }

            Board board = new(width, rows.Length);
            int tileId = 0;
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board[x, y].Id = tileId++;
                    board[x, y].Type = tileTypes[y][x];
                }
            }

            return board;
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

using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal static class BoardGenerator
    {
        internal static Board Create(
            int width,
            int height,
            IReadOnlyList<int> colors)
        {
            Board board = new(width, height);

            int tileId = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> availableColors = new(colors.Count);
                    for (int i = 0; i < colors.Count; i++)
                    {
                        availableColors.Add(colors[i]);
                    }

                    if (x > 1 &&
                        board[x - 1, y].Color == board[x - 2, y].Color)
                    {
                        availableColors.Remove(board[x - 1, y].Color);
                    }

                    if (y > 1 &&
                        board[x, y - 1].Color == board[x, y - 2].Color)
                    {
                        availableColors.Remove(board[x, y - 1].Color);
                    }

                    board[x, y].Id = tileId++;
                    board[x, y].Color = availableColors[Random.Range(0, availableColors.Count)];
                    board[x, y].Special = SpecialType.None;
                }
            }

            return board;
        }
    }
}

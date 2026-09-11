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
            IReadOnlyList<int> tileTypes)
        {
            Board board = new(width, height);

            int tileId = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<int> noMatchTypes = new(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        noMatchTypes.Add(tileTypes[i]);
                    }

                    if (x > 1 &&
                        board[x - 1, y].Type == board[x - 2, y].Type)
                    {
                        noMatchTypes.Remove(board[x - 1, y].Type);
                    }

                    if (y > 1 &&
                        board[x, y - 1].Type == board[x, y - 2].Type)
                    {
                        noMatchTypes.Remove(board[x, y - 1].Type);
                    }

                    board[x, y].Id = tileId++;
                    board[x, y].Type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
                }
            }

            return board;
        }
    }
}

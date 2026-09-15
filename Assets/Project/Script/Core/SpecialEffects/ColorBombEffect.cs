using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class ColorBombEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext context) =>
            !context.CombinedWith.HasValue && context.Special == SpecialType.ColorBomb;

        public SpecialActivationResult Activate(Board board, SpecialActivationContext context)
        {
            List<Vector2Int> affectedCells = new() { context.Position };
            context.TargetColor ??= FindMostCommonColor(board);

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Tile tile = board[x, y];
                    if (!tile.IsEmpty && tile.Color >= 0 &&
                        context.TargetColor.HasValue && tile.Color == context.TargetColor.Value)
                    {
                        Vector2Int position = new(x, y);
                        if (position != context.Position)
                        {
                            affectedCells.Add(position);
                        }
                    }
                }
            }

            return new SpecialActivationResult(affectedCells, false, null);
        }

        private static int? FindMostCommonColor(Board board)
        {
            Dictionary<int, int> counts = new();
            int? mostCommonColor = null;
            int highestCount = 0;

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Tile tile = board[x, y];
                    if (tile.IsEmpty || tile.Color < 0)
                    {
                        continue;
                    }

                    counts.TryGetValue(tile.Color, out int count);
                    count++;
                    counts[tile.Color] = count;
                    if (count > highestCount ||
                        count == highestCount &&
                        (!mostCommonColor.HasValue || tile.Color < mostCommonColor.Value))
                    {
                        highestCount = count;
                        mostCommonColor = tile.Color;
                    }
                }
            }

            return mostCommonColor;
        }
    }
}

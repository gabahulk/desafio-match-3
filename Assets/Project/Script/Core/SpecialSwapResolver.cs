using System.Collections.Generic;
using Gazeus.DesafioMatch3.Core.SpecialEffects;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal static class SpecialSwapResolver
    {
        internal static bool TryResolve(Board board, Vector2Int first, Vector2Int second,
            out SpecialSwapResolution resolution)
        {
            Tile firstTile = board[first.x, first.y];
            Tile secondTile = board[second.x, second.y];
            Vector2Int bombPosition;
            Tile targetTile;

            if (firstTile.Special == SpecialType.ColorBomb && IsNormalColorTile(secondTile))
            {
                bombPosition = first;
                targetTile = secondTile;
            }
            else if (secondTile.Special == SpecialType.ColorBomb && IsNormalColorTile(firstTile))
            {
                bombPosition = second;
                targetTile = firstTile;
            }
            else
            {
                resolution = null;
                return false;
            }

            HashSet<Vector2Int> cells = new() { bombPosition };
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Tile tile = board[x, y];
                    if (!tile.IsEmpty && tile.Color == targetTile.Color)
                    {
                        cells.Add(new Vector2Int(x, y));
                    }
                }
            }

            resolution = new SpecialSwapResolution(cells, new SpecialActivationInfo
            {
                Special = SpecialType.ColorBomb,
                Position = bombPosition,
                Phase = SpecialActivationPhase.First,
                TargetColor = targetTile.Color
            });
            return true;
        }

        private static bool IsNormalColorTile(Tile tile)
        {
            return !tile.IsEmpty && tile.Special == SpecialType.None && tile.Color >= 0;
        }
    }
}

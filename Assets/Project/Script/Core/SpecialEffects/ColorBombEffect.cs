using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class ColorBombEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialType special) => special == SpecialType.ColorBomb;

        public SpecialActivationResult Activate(Board board, SpecialActivationContext context)
        {
            List<Vector2Int> affectedCells = new() { context.Position };
            if (!context.TargetColor.HasValue)
            {
                return new SpecialActivationResult(affectedCells, false, null);
            }

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Tile tile = board[x, y];
                    if (!tile.IsEmpty && tile.Color == context.TargetColor.Value)
                    {
                        affectedCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            return new SpecialActivationResult(affectedCells, false, null);
        }
    }
}

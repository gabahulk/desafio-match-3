using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class StripedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialType special)
        {
            return special == SpecialType.HorizontalStriped ||
                   special == SpecialType.VerticalStriped;
        }

        public SpecialActivationResult Activate(
            Board board,
            SpecialActivationContext context)
        {
            List<Vector2Int> affectedCells = new();
            Vector2Int position = context.Position;
            SpecialType special = board[position.x, position.y].Special;
            if (special == SpecialType.HorizontalStriped)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    affectedCells.Add(new Vector2Int(x, position.y));
                }
            }
            else if (special == SpecialType.VerticalStriped)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    affectedCells.Add(new Vector2Int(position.x, y));
                }
            }

            return new SpecialActivationResult(affectedCells, false, null);
        }
    }
}

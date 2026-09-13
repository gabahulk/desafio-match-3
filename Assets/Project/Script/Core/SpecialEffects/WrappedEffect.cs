using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core.SpecialEffects
{
    internal sealed class WrappedEffect : ISpecialEffect
    {
        public bool CanHandle(SpecialActivationContext context)
        {
            return !context.CombinedWith.HasValue && context.Special == SpecialType.Wrapped;
        }

        public SpecialActivationResult Activate(
            Board board,
            SpecialActivationContext context)
        {
            Vector2Int position = context.Position;
            List<Vector2Int> affectedCells = new();
            int minimumX = Mathf.Max(0, position.x - 1);
            int maximumX = Mathf.Min(board.Width - 1, position.x + 1);
            int minimumY = Mathf.Max(0, position.y - 1);
            int maximumY = Mathf.Min(board.Height - 1, position.y + 1);

            for (int y = minimumY; y <= maximumY; y++)
            {
                for (int x = minimumX; x <= maximumX; x++)
                {
                    affectedCells.Add(new Vector2Int(x, y));
                }
            }

            bool isFirstActivation = context.Phase == SpecialActivationPhase.First;
            PendingSpecialActivation pendingActivation = isFirstActivation
                ? new PendingSpecialActivation(
                    board[position.x, position.y].Id,
                    SpecialType.Wrapped,
                    SpecialActivationPhase.Second)
                : null;

            return new SpecialActivationResult(
                affectedCells,
                isFirstActivation ? new[] { position } : System.Array.Empty<Vector2Int>(),
                pendingActivation == null ? System.Array.Empty<PendingSpecialActivation>() : new[] { pendingActivation },
                System.Array.Empty<SpecialActivationContext>(),
                System.Array.Empty<SpecialTransformation>());
        }
    }
}

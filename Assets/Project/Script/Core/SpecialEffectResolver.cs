using System.Collections.Generic;
using Gazeus.DesafioMatch3.Core.SpecialEffects;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal static class SpecialEffectResolver
    {
        private static readonly ISpecialEffect[] Effects =
        {
            new StripedEffect(),
            new WrappedEffect(),
            new ColorBombEffect()
        };

        internal static SpecialEffectResolution Expand(
            Board board,
            HashSet<Vector2Int> initialCells,
            IReadOnlyCollection<Vector2Int> protectedCells,
            IReadOnlyList<SpecialActivationInfo> initialActivations = null)
        {
            HashSet<Vector2Int> destructionCells = new(initialCells);
            Queue<SpecialActivationContext> specialsToActivate = new();
            HashSet<Vector2Int> queuedSpecials = new();

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int position = new(x, y);
                    if (destructionCells.Contains(position))
                    {
                        QueueSpecialIfHandled(board, position, SpecialActivationPhase.First,
                            specialsToActivate, queuedSpecials);
                    }
                }
            }

            return ExpandQueued(board, destructionCells, protectedCells,
                specialsToActivate, queuedSpecials, initialActivations);
        }

        internal static SpecialEffectResolution ResolvePending(
            Board board,
            IReadOnlyList<PendingSpecialActivation> pendingActivations)
        {
            HashSet<Vector2Int> destructionCells = new();
            Queue<SpecialActivationContext> specialsToActivate = new();
            HashSet<Vector2Int> queuedSpecials = new();

            // Resolve each stable tile Id after gravity while retaining activation order.
            // A missing or previously destroyed tile is safely skipped.
            for (int index = 0; index < pendingActivations.Count; index++)
            {
                PendingSpecialActivation pending = pendingActivations[index];
                if (TryFindTile(board, pending.TileId, pending.Special, out Vector2Int position) &&
                    queuedSpecials.Add(position))
                {
                    specialsToActivate.Enqueue(new SpecialActivationContext(position, pending.Phase));
                }
            }

            return ExpandQueued(board, destructionCells, new HashSet<Vector2Int>(),
                specialsToActivate, queuedSpecials, null);
        }

        internal static IReadOnlyList<SpecialActivationContext> CreateSpecialMatchActivations(
            Board board,
            SpecialMatch specialMatch)
        {
            Tile first = board[specialMatch.FirstPosition.x, specialMatch.FirstPosition.y];
            Tile second = board[specialMatch.SecondPosition.x, specialMatch.SecondPosition.y];
            List<SpecialActivationContext> contexts = new();

            if (first.Special == SpecialType.ColorBomb && second.Special == SpecialType.ColorBomb)
            {
                contexts.Add(new SpecialActivationContext(specialMatch.FirstPosition, SpecialActivationPhase.First));
                contexts.Add(new SpecialActivationContext(specialMatch.SecondPosition, SpecialActivationPhase.First));
                return contexts;
            }

            if (first.Special == SpecialType.ColorBomb || second.Special == SpecialType.ColorBomb)
            {
                Tile other = first.Special == SpecialType.ColorBomb ? second : first;
                Vector2Int bombPosition = first.Special == SpecialType.ColorBomb
                    ? specialMatch.FirstPosition
                    : specialMatch.SecondPosition;
                contexts.Add(new SpecialActivationContext(bombPosition, SpecialActivationPhase.First, other.Color));
                if (other.Special != SpecialType.None)
                {
                    Vector2Int otherPosition = first.Special == SpecialType.ColorBomb
                        ? specialMatch.SecondPosition
                        : specialMatch.FirstPosition;
                    contexts.Add(new SpecialActivationContext(otherPosition, SpecialActivationPhase.First));
                }

                return contexts;
            }

            contexts.Add(new SpecialActivationContext(specialMatch.FirstPosition, SpecialActivationPhase.First));
            contexts.Add(new SpecialActivationContext(specialMatch.SecondPosition, SpecialActivationPhase.First));
            return contexts;
        }

        internal static SpecialEffectResolution ResolveActivations(
            Board board,
            IReadOnlyList<SpecialActivationContext> contexts)
        {
            Queue<SpecialActivationContext> specialsToActivate = new();
            HashSet<Vector2Int> queuedSpecials = new();
            for (int index = 0; index < contexts.Count; index++)
            {
                SpecialActivationContext context = contexts[index];
                if (queuedSpecials.Add(context.Position))
                {
                    specialsToActivate.Enqueue(context);
                }
            }

            return ExpandQueued(board, new HashSet<Vector2Int>(), new HashSet<Vector2Int>(),
                specialsToActivate, queuedSpecials, null);
        }

        private static SpecialEffectResolution ExpandQueued(
            Board board,
            HashSet<Vector2Int> destructionCells,
            IReadOnlyCollection<Vector2Int> protectedCells,
            Queue<SpecialActivationContext> specialsToActivate,
            HashSet<Vector2Int> queuedSpecials,
            IReadOnlyList<SpecialActivationInfo> initialActivations)
        {
            List<PendingSpecialActivation> pendingActivations = new();
            List<SpecialActivationInfo> activations = initialActivations != null
                ? new List<SpecialActivationInfo>(initialActivations)
                : new List<SpecialActivationInfo>();
            HashSet<Vector2Int> preservedSourceCells = new();
            HashSet<Vector2Int> activatedSpecials = new();

            while (specialsToActivate.Count > 0)
            {
                SpecialActivationContext request = specialsToActivate.Dequeue();
                if (!activatedSpecials.Add(request.Position))
                {
                    continue;
                }

                SpecialType special = board[request.Position.x, request.Position.y].Special;
                ISpecialEffect effect = FindEffect(special);
                if (effect == null)
                {
                    continue;
                }

                activations.Add(new SpecialActivationInfo
                {
                    Special = special,
                    Position = request.Position,
                    Phase = request.Phase,
                    TargetColor = request.TargetColor
                });

                SpecialActivationResult activation = effect.Activate(
                    board, request);
                if (activation.PreserveSource)
                {
                    preservedSourceCells.Add(request.Position);
                    destructionCells.Remove(request.Position);
                }

                if (activation.PendingActivation != null)
                {
                    pendingActivations.Add(activation.PendingActivation);
                }

                for (int index = 0; index < activation.AffectedCells.Count; index++)
                {
                    AddDestructionCell(board, activation.AffectedCells[index], protectedCells,
                        preservedSourceCells, destructionCells, specialsToActivate, queuedSpecials);
                }
            }

            return new SpecialEffectResolution(destructionCells, pendingActivations, activations);
        }

        private static ISpecialEffect FindEffect(SpecialType special)
        {
            for (int index = 0; index < Effects.Length; index++)
            {
                if (Effects[index].CanHandle(special))
                {
                    return Effects[index];
                }
            }

            return null;
        }

        private static void AddDestructionCell(
            Board board,
            Vector2Int position,
            IReadOnlyCollection<Vector2Int> protectedCells,
            IReadOnlyCollection<Vector2Int> preservedSourceCells,
            HashSet<Vector2Int> destructionCells,
            Queue<SpecialActivationContext> specialsToActivate,
            HashSet<Vector2Int> queuedSpecials)
        {
            if (Contains(protectedCells, position) || Contains(preservedSourceCells, position))
            {
                return;
            }

            destructionCells.Add(position);
            QueueSpecialIfHandled(board, position, SpecialActivationPhase.First,
                specialsToActivate, queuedSpecials);
        }

        private static void QueueSpecialIfHandled(
            Board board,
            Vector2Int position,
            SpecialActivationPhase phase,
            Queue<SpecialActivationContext> specialsToActivate,
            HashSet<Vector2Int> queuedSpecials)
        {
            SpecialType special = board[position.x, position.y].Special;
            if (special != SpecialType.ColorBomb && FindEffect(special) != null && queuedSpecials.Add(position))
            {
                specialsToActivate.Enqueue(new SpecialActivationContext(position, phase));
            }
        }

        private static bool TryFindTile(
            Board board,
            int tileId,
            SpecialType special,
            out Vector2Int position)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Tile tile = board[x, y];
                    if (tile.Id == tileId && tile.Special == special)
                    {
                        position = new Vector2Int(x, y);
                        return true;
                    }
                }
            }

            position = default;
            return false;
        }

        private static bool Contains(IEnumerable<Vector2Int> positions, Vector2Int position)
        {
            foreach (Vector2Int candidate in positions)
            {
                if (candidate == position)
                {
                    return true;
                }
            }

            return false;
        }

    }
}

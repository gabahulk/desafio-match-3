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
            new WrappedEffect()
        };

        internal static SpecialEffectResolution Expand(
            Board board,
            HashSet<Vector2Int> initialCells,
            IReadOnlyCollection<Vector2Int> protectedCells)
        {
            HashSet<Vector2Int> destructionCells = new(initialCells);
            Queue<ActivationRequest> specialsToActivate = new();
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
                specialsToActivate, queuedSpecials);
        }

        internal static SpecialEffectResolution ResolvePending(
            Board board,
            IReadOnlyList<PendingSpecialActivation> pendingActivations)
        {
            HashSet<Vector2Int> destructionCells = new();
            Queue<ActivationRequest> specialsToActivate = new();
            HashSet<Vector2Int> queuedSpecials = new();

            // Resolve each stable tile Id after gravity while retaining activation order.
            // A missing or previously destroyed tile is safely skipped.
            for (int index = 0; index < pendingActivations.Count; index++)
            {
                PendingSpecialActivation pending = pendingActivations[index];
                if (TryFindTile(board, pending.TileId, pending.Special, out Vector2Int position) &&
                    queuedSpecials.Add(position))
                {
                    specialsToActivate.Enqueue(new ActivationRequest(position, pending.Phase));
                }
            }

            return ExpandQueued(board, destructionCells, new HashSet<Vector2Int>(),
                specialsToActivate, queuedSpecials);
        }

        private static SpecialEffectResolution ExpandQueued(
            Board board,
            HashSet<Vector2Int> destructionCells,
            IReadOnlyCollection<Vector2Int> protectedCells,
            Queue<ActivationRequest> specialsToActivate,
            HashSet<Vector2Int> queuedSpecials)
        {
            List<PendingSpecialActivation> pendingActivations = new();
            HashSet<Vector2Int> preservedSourceCells = new();
            HashSet<Vector2Int> activatedSpecials = new();

            while (specialsToActivate.Count > 0)
            {
                ActivationRequest request = specialsToActivate.Dequeue();
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

                SpecialActivationResult activation = effect.Activate(
                    board, request.Position, request.Phase);
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

            return new SpecialEffectResolution(destructionCells, pendingActivations);
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
            Queue<ActivationRequest> specialsToActivate,
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
            Queue<ActivationRequest> specialsToActivate,
            HashSet<Vector2Int> queuedSpecials)
        {
            SpecialType special = board[position.x, position.y].Special;
            if (FindEffect(special) != null && queuedSpecials.Add(position))
            {
                specialsToActivate.Enqueue(new ActivationRequest(position, phase));
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

        private readonly struct ActivationRequest
        {
            internal Vector2Int Position { get; }
            internal SpecialActivationPhase Phase { get; }

            internal ActivationRequest(Vector2Int position, SpecialActivationPhase phase)
            {
                Position = position;
                Phase = phase;
            }
        }
    }
}

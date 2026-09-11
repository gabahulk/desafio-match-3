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
            new StripedEffect()
        };

        internal static HashSet<Vector2Int> Expand(
            Board board,
            HashSet<Vector2Int> initialCells,
            IReadOnlyCollection<Vector2Int> protectedCells)
        {
            HashSet<Vector2Int> destructionCells = new(initialCells);
            Queue<Vector2Int> specialsToActivate = new();
            HashSet<Vector2Int> queuedSpecials = new();

            foreach (Vector2Int position in destructionCells)
            {
                QueueSpecialIfHandled(board, position, specialsToActivate, queuedSpecials);
            }

            List<Vector2Int> affectedCells = new();
            HashSet<Vector2Int> activatedSpecials = new();
            while (specialsToActivate.Count > 0)
            {
                Vector2Int specialPosition = specialsToActivate.Dequeue();
                if (!activatedSpecials.Add(specialPosition))
                {
                    continue;
                }

                SpecialType special = board[specialPosition.x, specialPosition.y].Special;
                ISpecialEffect effect = FindEffect(special);
                if (effect == null)
                {
                    continue;
                }

                affectedCells.Clear();
                effect.Expand(board, specialPosition, affectedCells);
                for (int index = 0; index < affectedCells.Count; index++)
                {
                    AddDestructionCell(
                        board,
                        affectedCells[index],
                        protectedCells,
                        destructionCells,
                        specialsToActivate,
                        queuedSpecials);
                }
            }

            return destructionCells;
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
            HashSet<Vector2Int> destructionCells,
            Queue<Vector2Int> specialsToActivate,
            HashSet<Vector2Int> queuedSpecials)
        {
            if (Contains(protectedCells, position))
            {
                return;
            }

            destructionCells.Add(position);
            QueueSpecialIfHandled(board, position, specialsToActivate, queuedSpecials);
        }

        private static void QueueSpecialIfHandled(
            Board board,
            Vector2Int position,
            Queue<Vector2Int> specialsToActivate,
            HashSet<Vector2Int> queuedSpecials)
        {
            SpecialType special = board[position.x, position.y].Special;
            if (FindEffect(special) != null && queuedSpecials.Add(position))
            {
                specialsToActivate.Enqueue(position);
            }
        }

        private static bool Contains(
            IEnumerable<Vector2Int> positions,
            Vector2Int position)
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

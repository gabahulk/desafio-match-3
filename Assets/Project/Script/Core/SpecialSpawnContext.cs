using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal sealed class SpecialSpawnContext
    {
        internal bool IsPlayerSwap { get; }
        internal Vector2Int SwapFrom { get; }
        internal Vector2Int SwapTo { get; }
        internal IReadOnlyCollection<Vector2Int> MovedDestinations { get; }
        internal IReadOnlyCollection<Vector2Int> AddedPositions { get; }

        private SpecialSpawnContext(
            bool isPlayerSwap,
            Vector2Int swapFrom,
            Vector2Int swapTo,
            IReadOnlyCollection<Vector2Int> movedDestinations,
            IReadOnlyCollection<Vector2Int> addedPositions)
        {
            IsPlayerSwap = isPlayerSwap;
            SwapFrom = swapFrom;
            SwapTo = swapTo;
            MovedDestinations = movedDestinations;
            AddedPositions = addedPositions;
        }

        internal static SpecialSpawnContext FromSwap(Vector2Int from, Vector2Int to)
        {
            return new SpecialSpawnContext(
                true,
                from,
                to,
                new HashSet<Vector2Int>(),
                new HashSet<Vector2Int>());
        }

        internal static SpecialSpawnContext FromCascade(
            IReadOnlyList<MovedTileInfo> movedTiles,
            IReadOnlyList<AddedTileInfo> addedTiles)
        {
            HashSet<Vector2Int> movedDestinations = new();
            for (int index = 0; index < movedTiles.Count; index++)
            {
                movedDestinations.Add(movedTiles[index].To);
            }

            HashSet<Vector2Int> addedPositions = new();
            for (int index = 0; index < addedTiles.Count; index++)
            {
                addedPositions.Add(addedTiles[index].Position);
            }

            return new SpecialSpawnContext(
                false,
                default,
                default,
                movedDestinations,
                addedPositions);
        }
    }
}

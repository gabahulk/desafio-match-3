using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal static class SpecialTileRules
    {
        internal static SpecialCreationResult CreateSpecials(
            Board board,
            IReadOnlyList<MatchPattern> patterns,
            SpecialSpawnContext context)
        {
            List<SpecialTileInfo> createdSpecialTiles = new();
            HashSet<Vector2Int> protectedCells = new();

            for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
            {
                MatchPattern pattern = patterns[patternIndex];
                if (pattern.Shape != MatchShape.Straight || pattern.Size != 4)
                {
                    continue;
                }

                if (!TrySelectSpawnPosition(
                        board,
                        pattern,
                        context,
                        protectedCells,
                        out Vector2Int spawnPosition))
                {
                    continue;
                }

                SpecialType special = context.IsPlayerSwap
                    ? GetSwapStripedType(context.SwapFrom, context.SwapTo)
                    : GetPatternStripedType(pattern);
                Tile spawnTile = board[spawnPosition.x, spawnPosition.y];
                spawnTile.Special = special;
                protectedCells.Add(spawnPosition);
                createdSpecialTiles.Add(new SpecialTileInfo
                {
                    Position = spawnPosition,
                    Color = spawnTile.Color,
                    Special = special
                });
            }

            return new SpecialCreationResult(createdSpecialTiles, protectedCells);
        }

        internal static HashSet<Vector2Int> ExpandDestruction(
            Board board,
            HashSet<Vector2Int> initialCells,
            HashSet<Vector2Int> protectedCells)
        {
            HashSet<Vector2Int> destructionCells = new(initialCells);
            Queue<Vector2Int> specialsToActivate = new();
            HashSet<Vector2Int> queuedSpecials = new();

            foreach (Vector2Int position in destructionCells)
            {
                QueueSpecialIfNeeded(board, position, specialsToActivate, queuedSpecials);
            }

            HashSet<Vector2Int> activatedSpecials = new();
            while (specialsToActivate.Count > 0)
            {
                Vector2Int specialPosition = specialsToActivate.Dequeue();
                if (!activatedSpecials.Add(specialPosition))
                {
                    continue;
                }

                Tile tile = board[specialPosition.x, specialPosition.y];
                if (tile.Special == SpecialType.HorizontalStriped)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        AddDestructionCell(
                            board,
                            new Vector2Int(x, specialPosition.y),
                            protectedCells,
                            destructionCells,
                            specialsToActivate,
                            queuedSpecials);
                    }
                }
                else if (tile.Special == SpecialType.VerticalStriped)
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        AddDestructionCell(
                            board,
                            new Vector2Int(specialPosition.x, y),
                            protectedCells,
                            destructionCells,
                            specialsToActivate,
                            queuedSpecials);
                    }
                }
            }

            return destructionCells;
        }

        private static bool TrySelectSpawnPosition(
            Board board,
            MatchPattern pattern,
            SpecialSpawnContext context,
            HashSet<Vector2Int> protectedCells,
            out Vector2Int spawnPosition)
        {
            if (context.IsPlayerSwap)
            {
                if (IsEligibleSpawn(board, pattern, context.SwapTo, protectedCells))
                {
                    spawnPosition = context.SwapTo;
                    return true;
                }

                if (IsEligibleSpawn(board, pattern, context.SwapFrom, protectedCells))
                {
                    spawnPosition = context.SwapFrom;
                    return true;
                }
            }
            else
            {
                if (TrySelectContextPosition(
                        board,
                        pattern,
                        context.MovedDestinations,
                        protectedCells,
                        out spawnPosition))
                {
                    return true;
                }

                if (TrySelectContextPosition(
                        board,
                        pattern,
                        context.AddedPositions,
                        protectedCells,
                        out spawnPosition))
                {
                    return true;
                }
            }

            return TrySelectRowMajorPosition(
                board,
                pattern,
                null,
                protectedCells,
                out spawnPosition);
        }

        private static bool TrySelectContextPosition(
            Board board,
            MatchPattern pattern,
            HashSet<Vector2Int> contextPositions,
            HashSet<Vector2Int> protectedCells,
            out Vector2Int spawnPosition)
        {
            return TrySelectRowMajorPosition(
                board,
                pattern,
                contextPositions,
                protectedCells,
                out spawnPosition);
        }

        private static bool TrySelectRowMajorPosition(
            Board board,
            MatchPattern pattern,
            HashSet<Vector2Int> requiredPositions,
            HashSet<Vector2Int> protectedCells,
            out Vector2Int spawnPosition)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int candidate = new(x, y);
                    if ((requiredPositions == null || requiredPositions.Contains(candidate)) &&
                        IsEligibleSpawn(board, pattern, candidate, protectedCells))
                    {
                        spawnPosition = candidate;
                        return true;
                    }
                }
            }

            spawnPosition = default;
            return false;
        }

        private static bool IsEligibleSpawn(
            Board board,
            MatchPattern pattern,
            Vector2Int position,
            HashSet<Vector2Int> protectedCells)
        {
            if (protectedCells.Contains(position) || !Contains(pattern.Cells, position))
            {
                return false;
            }

            Tile tile = board[position.x, position.y];
            return !tile.IsEmpty && tile.Special == SpecialType.None;
        }

        private static bool Contains(IReadOnlyList<Vector2Int> positions, Vector2Int position)
        {
            for (int index = 0; index < positions.Count; index++)
            {
                if (positions[index] == position)
                {
                    return true;
                }
            }

            return false;
        }

        private static SpecialType GetSwapStripedType(Vector2Int from, Vector2Int to)
        {
            return from.y == to.y
                ? SpecialType.HorizontalStriped
                : SpecialType.VerticalStriped;
        }

        private static SpecialType GetPatternStripedType(MatchPattern pattern)
        {
            int firstY = pattern.Cells[0].y;
            for (int index = 1; index < pattern.Cells.Count; index++)
            {
                if (pattern.Cells[index].y != firstY)
                {
                    return SpecialType.VerticalStriped;
                }
            }

            return SpecialType.HorizontalStriped;
        }

        private static void AddDestructionCell(
            Board board,
            Vector2Int position,
            HashSet<Vector2Int> protectedCells,
            HashSet<Vector2Int> destructionCells,
            Queue<Vector2Int> specialsToActivate,
            HashSet<Vector2Int> queuedSpecials)
        {
            if (protectedCells.Contains(position))
            {
                return;
            }

            destructionCells.Add(position);
            QueueSpecialIfNeeded(board, position, specialsToActivate, queuedSpecials);
        }

        private static void QueueSpecialIfNeeded(
            Board board,
            Vector2Int position,
            Queue<Vector2Int> specialsToActivate,
            HashSet<Vector2Int> queuedSpecials)
        {
            SpecialType special = board[position.x, position.y].Special;
            if ((special == SpecialType.HorizontalStriped ||
                 special == SpecialType.VerticalStriped) &&
                queuedSpecials.Add(position))
            {
                specialsToActivate.Enqueue(position);
            }
        }
    }

    internal sealed class SpecialCreationResult
    {
        internal List<SpecialTileInfo> CreatedSpecialTiles { get; }
        internal HashSet<Vector2Int> ProtectedCells { get; }

        internal SpecialCreationResult(
            List<SpecialTileInfo> createdSpecialTiles,
            HashSet<Vector2Int> protectedCells)
        {
            CreatedSpecialTiles = createdSpecialTiles;
            ProtectedCells = protectedCells;
        }
    }

    internal sealed class SpecialSpawnContext
    {
        internal bool IsPlayerSwap { get; }
        internal Vector2Int SwapFrom { get; }
        internal Vector2Int SwapTo { get; }
        internal HashSet<Vector2Int> MovedDestinations { get; }
        internal HashSet<Vector2Int> AddedPositions { get; }

        private SpecialSpawnContext(
            bool isPlayerSwap,
            Vector2Int swapFrom,
            Vector2Int swapTo,
            HashSet<Vector2Int> movedDestinations,
            HashSet<Vector2Int> addedPositions)
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

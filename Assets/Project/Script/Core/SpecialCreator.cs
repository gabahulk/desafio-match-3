using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    internal static class SpecialCreator
    {
        internal static SpecialCreationResult Create(
            Board board,
            MatchPattern pattern,
            SpecialSpawnContext context,
            IReadOnlyCollection<Vector2Int> alreadyProtectedCells)
        {
            // Other semantically eligible shapes are reserved for specials whose gameplay
            // is not implemented yet. Currently only a Straight-4 maps to a concrete special.
            if (!TryGetSpecialType(pattern, context, out SpecialType special))
            {
                return new SpecialCreationResult(
                    new List<SpecialTileInfo>(),
                    new HashSet<Vector2Int>());
            }

            if (!TrySelectSpawnPosition(
                    board,
                    pattern,
                    context,
                    alreadyProtectedCells,
                    out Vector2Int spawnPosition))
            {
                return new SpecialCreationResult(
                    new List<SpecialTileInfo>(),
                    new HashSet<Vector2Int>());
            }

            Tile spawnTile = board[spawnPosition.x, spawnPosition.y];
            spawnTile.Special = special;

            return new SpecialCreationResult(
                new List<SpecialTileInfo>
                {
                    new()
                    {
                        Position = spawnPosition,
                        Color = spawnTile.Color,
                        Special = special
                    }
                },
                new HashSet<Vector2Int> { spawnPosition });
        }

        private static bool TryGetSpecialType(
            MatchPattern pattern,
            SpecialSpawnContext context,
            out SpecialType special)
        {
            if (pattern.Shape == MatchShape.Straight && pattern.Size == 4)
            {
                special = context.IsPlayerSwap
                    ? GetSwapStripedType(context.SwapFrom, context.SwapTo)
                    : GetPatternStripedType(pattern);
                return true;
            }

            special = SpecialType.None;
            return false;
        }

        private static bool TrySelectSpawnPosition(
            Board board,
            MatchPattern pattern,
            SpecialSpawnContext context,
            IReadOnlyCollection<Vector2Int> protectedCells,
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
            IReadOnlyCollection<Vector2Int> contextPositions,
            IReadOnlyCollection<Vector2Int> protectedCells,
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
            IReadOnlyCollection<Vector2Int> requiredPositions,
            IReadOnlyCollection<Vector2Int> protectedCells,
            out Vector2Int spawnPosition)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    Vector2Int candidate = new(x, y);
                    if ((requiredPositions == null || Contains(requiredPositions, candidate)) &&
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
            IReadOnlyCollection<Vector2Int> protectedCells)
        {
            if (Contains(protectedCells, position) || !Contains(pattern.Cells, position))
            {
                return false;
            }

            Tile tile = board[position.x, position.y];
            return !tile.IsEmpty && tile.Special == SpecialType.None;
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
    }
}

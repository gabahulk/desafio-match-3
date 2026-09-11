using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Core
{
    public class GameService
    {
        private Board _board;
        private List<int> _colors;
        private int _tileCount;

        public GameService()
        {
        }

        public GameService(Board board, IReadOnlyList<int> colors)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _colors = colors != null
                ? new List<int>(colors)
                : throw new ArgumentNullException(nameof(colors));
            _tileCount = GetNextTileId(board);
        }

        public Board StartGame(int boardWidth, int boardHeight)
        {
            _colors = new List<int> { 0, 1, 2, 3 };
            _board = BoardGenerator.Create(boardWidth, boardHeight, _colors);
            _tileCount = boardWidth * boardHeight;

            return _board;
        }

        public MoveResult TrySwap(int fromX, int fromY, int toX, int toY)
        {
            Board candidateBoard = _board.Clone();

            (candidateBoard[toX, toY], candidateBoard[fromX, fromY]) =
                (candidateBoard[fromX, fromY], candidateBoard[toX, toY]);

            IReadOnlyList<MatchPattern> patterns = MatchFinder.FindMatches(candidateBoard);
            if (patterns.Count == 0)
            {
                return new MoveResult(false, Array.Empty<BoardSequence>());
            }

            ResolutionStepContext context = ResolutionStepContext.FromSwap(
                new Vector2Int(fromX, fromY),
                new Vector2Int(toX, toY));
            List<BoardSequence> boardSequences = Resolve(candidateBoard, patterns, context);
            _board = candidateBoard;

            return new MoveResult(true, boardSequences);
        }

        private List<BoardSequence> Resolve(
            Board board,
            IReadOnlyList<MatchPattern> patterns,
            ResolutionStepContext context)
        {
            List<BoardSequence> boardSequences = new();

            while (patterns.Count > 0)
            {
                HashSet<Vector2Int> protectedCells = new();
                List<SpecialTileInfo> createdSpecialTiles = CreateStripedTiles(
                    board,
                    patterns,
                    context,
                    protectedCells);
                HashSet<Vector2Int> destructionCells = BuildDestructionSet(
                    board,
                    patterns,
                    protectedCells);

                List<Vector2Int> matchedPosition = new(destructionCells.Count);
                for (int y = 0; y < board.Height; y++)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        Vector2Int position = new(x, y);
                        if (destructionCells.Contains(position))
                        {
                            matchedPosition.Add(position);
                            board[x, y] = new Tile
                            {
                                Id = -1,
                                Color = -1,
                                Special = SpecialType.None
                            };
                        }
                    }
                }

                // Dropping the tiles
                Dictionary<int, MovedTileInfo> movedTiles = new();
                List<MovedTileInfo> movedTilesList = new();
                for (int i = 0; i < matchedPosition.Count; i++)
                {
                    int x = matchedPosition[i].x;
                    int y = matchedPosition[i].y;
                    if (y > 0)
                    {
                        for (int j = y; j > 0; j--)
                        {
                            Tile movedTile = board[x, j - 1];
                            board[x, j] = movedTile;
                            if (!movedTile.IsEmpty)
                            {
                                if (movedTiles.ContainsKey(movedTile.Id))
                                {
                                    movedTiles[movedTile.Id].To = new Vector2Int(x, j);
                                }
                                else
                                {
                                    MovedTileInfo movedTileInfo = new()
                                    {
                                        From = new Vector2Int(x, j - 1),
                                        To = new Vector2Int(x, j)
                                    };
                                    movedTiles.Add(movedTile.Id, movedTileInfo);
                                    movedTilesList.Add(movedTileInfo);
                                }
                            }
                        }

                        board[x, 0] = new Tile
                        {
                            Id = -1,
                            Color = -1,
                            Special = SpecialType.None
                        };
                    }
                }

                // Filling the board
                List<AddedTileInfo> addedTiles = new();
                for (int y = board.Height - 1; y > -1; y--)
                {
                    for (int x = board.Width - 1; x > -1; x--)
                    {
                        if (board[x, y].IsEmpty)
                        {
                            int colorIndex = Random.Range(0, _colors.Count);
                            Tile tile = board[x, y];
                            tile.Id = _tileCount++;
                            tile.Color = _colors[colorIndex];
                            tile.Special = SpecialType.None;
                            addedTiles.Add(new AddedTileInfo
                            {
                                Position = new Vector2Int(x, y),
                                Color = tile.Color
                            });
                        }
                    }
                }

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition,
                    MovedTiles = movedTilesList,
                    AddedTiles = addedTiles,
                    CreatedSpecialTiles = createdSpecialTiles
                };
                boardSequences.Add(sequence);
                context = ResolutionStepContext.FromCascade(movedTilesList, addedTiles);
                patterns = MatchFinder.FindMatches(board);
            }

            return boardSequences;
        }

        private static List<SpecialTileInfo> CreateStripedTiles(
            Board board,
            IReadOnlyList<MatchPattern> patterns,
            ResolutionStepContext context,
            HashSet<Vector2Int> protectedCells)
        {
            List<SpecialTileInfo> createdSpecialTiles = new();

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

            return createdSpecialTiles;
        }

        private static bool TrySelectSpawnPosition(
            Board board,
            MatchPattern pattern,
            ResolutionStepContext context,
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

        private static HashSet<Vector2Int> BuildDestructionSet(
            Board board,
            IReadOnlyList<MatchPattern> patterns,
            HashSet<Vector2Int> protectedCells)
        {
            HashSet<Vector2Int> destructionCells = new();
            Queue<Vector2Int> specialsToActivate = new();
            HashSet<Vector2Int> queuedSpecials = new();

            for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
            {
                MatchPattern pattern = patterns[patternIndex];
                for (int cellIndex = 0; cellIndex < pattern.Cells.Count; cellIndex++)
                {
                    AddDestructionCell(
                        board,
                        pattern.Cells[cellIndex],
                        protectedCells,
                        destructionCells,
                        specialsToActivate,
                        queuedSpecials);
                }
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
            SpecialType special = board[position.x, position.y].Special;
            if ((special == SpecialType.HorizontalStriped ||
                 special == SpecialType.VerticalStriped) &&
                queuedSpecials.Add(position))
            {
                specialsToActivate.Enqueue(position);
            }
        }

        private static int GetNextTileId(Board board)
        {
            int highestTileId = -1;
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    int tileId = board[x, y].Id;
                    if (tileId > highestTileId)
                    {
                        highestTileId = tileId;
                    }
                }
            }

            return highestTileId + 1;
        }

        private sealed class ResolutionStepContext
        {
            internal bool IsPlayerSwap { get; }
            internal Vector2Int SwapFrom { get; }
            internal Vector2Int SwapTo { get; }
            internal HashSet<Vector2Int> MovedDestinations { get; }
            internal HashSet<Vector2Int> AddedPositions { get; }

            private ResolutionStepContext(
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

            internal static ResolutionStepContext FromSwap(Vector2Int from, Vector2Int to)
            {
                return new ResolutionStepContext(
                    true,
                    from,
                    to,
                    new HashSet<Vector2Int>(),
                    new HashSet<Vector2Int>());
            }

            internal static ResolutionStepContext FromCascade(
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

                return new ResolutionStepContext(
                    false,
                    default,
                    default,
                    movedDestinations,
                    addedPositions);
            }
        }

    }
}

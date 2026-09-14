using System;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Core.SpecialEffects;
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
            StartGame(board, colors);
        }

        public Board StartGame(int boardWidth, int boardHeight)
        {
            _colors = new List<int> { 0, 1, 2, 3 };
            _board = BoardGenerator.Create(boardWidth, boardHeight, _colors);
            _tileCount = boardWidth * boardHeight;

            return _board;
        }

        public Board StartGame(Board board, IReadOnlyList<int> colors)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _colors = colors != null
                ? new List<int>(colors)
                : throw new ArgumentNullException(nameof(colors));
            _tileCount = GetNextTileId(board);

            return _board;
        }

        public MoveResult TrySwap(int fromX, int fromY, int toX, int toY)
        {
            Board candidateBoard = _board.Clone();

            (candidateBoard[toX, toY], candidateBoard[fromX, fromY]) =
                (candidateBoard[fromX, fromY], candidateBoard[toX, toY]);

            Vector2Int from = new(fromX, fromY);
            Vector2Int to = new(toX, toY);
            MatchResult matchResult = MatchFinder.FindSwapMatch(candidateBoard, from, to);
            if (!matchResult.IsValid)
            {
                return new MoveResult(false, Array.Empty<BoardSequence>());
            }

            SpecialSpawnContext context = SpecialSpawnContext.FromSwap(
                from, to);
            List<BoardSequence> boardSequences = Resolve(candidateBoard, matchResult.StandardPatterns, context,
                matchResult.SpecialMatch == null
                    ? null
                    : SpecialEffectResolver.CreateSpecialMatchActivations(candidateBoard, matchResult.SpecialMatch));
            _board = candidateBoard;

            return new MoveResult(true, boardSequences);
        }

        private List<BoardSequence> Resolve(
            Board board,
            IReadOnlyList<MatchPattern> patterns,
            SpecialSpawnContext context,
            IReadOnlyList<SpecialActivationContext> initialActivations = null)
        {
            List<BoardSequence> boardSequences = new();
            IReadOnlyList<PendingSpecialActivation> pendingActivations =
                Array.Empty<PendingSpecialActivation>();

            while (patterns.Count > 0 || pendingActivations.Count > 0 || initialActivations != null)
            {
                List<SpecialTileInfo> createdSpecialTiles = new();
                HashSet<Vector2Int> protectedCells = new();
                if (pendingActivations.Count == 0 && initialActivations == null)
                {
                    for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
                    {
                        MatchPattern pattern = patterns[patternIndex];
                        if (!pattern.CreatesSpecial)
                        {
                            continue;
                        }

                        SpecialCreationResult creation = SpecialCreator.Create(
                            board,
                            pattern,
                            context,
                            protectedCells);
                        createdSpecialTiles.AddRange(creation.CreatedSpecialTiles);
                        protectedCells.UnionWith(creation.ProtectedCells);
                    }
                }

                SpecialEffectResolution effectResolution;
                if (pendingActivations.Count > 0)
                {
                    effectResolution = SpecialEffectResolver.ResolvePending(
                        board,
                        pendingActivations);
                }
                else if (initialActivations != null)
                {
                    effectResolution = SpecialEffectResolver.ResolveActivations(board, initialActivations);
                }
                else
                {
                    effectResolution = SpecialEffectResolver.Expand(board,
                        BuildMatchedCells(patterns, protectedCells), protectedCells);
                }

                initialActivations = null;

                HashSet<Vector2Int> destructionCells = effectResolution.DestructionCells;
                pendingActivations = effectResolution.PendingActivations;
                if (destructionCells.Count == 0)
                {
                    patterns = MatchFinder.FindMatches(board);
                    continue;
                }

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
                                Id = tile.Id,
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
                    CreatedSpecialTiles = createdSpecialTiles,
                    SpecialActivations = new List<SpecialActivationInfo>(effectResolution.Activations)
                };
                boardSequences.Add(sequence);
                context = SpecialSpawnContext.FromCascade(movedTilesList, addedTiles);
                patterns = pendingActivations.Count > 0
                    ? Array.Empty<MatchPattern>()
                    : MatchFinder.FindMatches(board);
            }

            return boardSequences;
        }

        private static HashSet<Vector2Int> BuildMatchedCells(
            IReadOnlyList<MatchPattern> patterns,
            HashSet<Vector2Int> protectedCells)
        {
            HashSet<Vector2Int> matchedCells = new();
            for (int patternIndex = 0; patternIndex < patterns.Count; patternIndex++)
            {
                MatchPattern pattern = patterns[patternIndex];
                for (int cellIndex = 0; cellIndex < pattern.Cells.Count; cellIndex++)
                {
                    Vector2Int position = pattern.Cells[cellIndex];
                    if (!protectedCells.Contains(position))
                    {
                        matchedCells.Add(position);
                    }
                }
            }

            return matchedCells;
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

    }
}

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
        private List<int> _tilesTypes;
        private int _tileCount;

        public GameService()
        {
        }

        public GameService(Board board, IReadOnlyList<int> tileTypes)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _tilesTypes = tileTypes != null
                ? new List<int>(tileTypes)
                : throw new ArgumentNullException(nameof(tileTypes));
            _tileCount = GetNextTileId(board);
        }

        public Board StartGame(int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<int> { 0, 1, 2, 3 };
            _board = BoardGenerator.Create(boardWidth, boardHeight, _tilesTypes);
            _tileCount = boardWidth * boardHeight;

            return _board;
        }

        public MoveResult TrySwap(int fromX, int fromY, int toX, int toY)
        {
            Board candidateBoard = _board.Clone();

            (candidateBoard[toX, toY], candidateBoard[fromX, fromY]) =
                (candidateBoard[fromX, fromY], candidateBoard[toX, toY]);

            List<List<bool>> matchedTiles = MatchFinder.FindMatches(candidateBoard);
            if (!MatchFinder.HasMatch(matchedTiles))
            {
                return new MoveResult(false, Array.Empty<BoardSequence>());
            }

            List<BoardSequence> boardSequences = Resolve(candidateBoard, matchedTiles);
            _board = candidateBoard;

            return new MoveResult(true, boardSequences);
        }

        private List<BoardSequence> Resolve(Board board, List<List<bool>> matchedTiles)
        {
            List<BoardSequence> boardSequences = new();

            while (MatchFinder.HasMatch(matchedTiles))
            {
                //Cleaning the matched tiles
                List<Vector2Int> matchedPosition = new();
                for (int y = 0; y < board.Height; y++)
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        if (matchedTiles[y][x])
                        {
                            matchedPosition.Add(new Vector2Int(x, y));
                            board[x, y] = new Tile { Id = -1, Type = -1 };
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
                            if (movedTile.Type > -1)
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
                            Type = -1
                        };
                    }
                }

                // Filling the board
                List<AddedTileInfo> addedTiles = new();
                for (int y = board.Height - 1; y > -1; y--)
                {
                    for (int x = board.Width - 1; x > -1; x--)
                    {
                        if (board[x, y].Type == -1)
                        {
                            int tileType = Random.Range(0, _tilesTypes.Count);
                            Tile tile = board[x, y];
                            tile.Id = _tileCount++;
                            tile.Type = _tilesTypes[tileType];
                            addedTiles.Add(new AddedTileInfo
                            {
                                Position = new Vector2Int(x, y),
                                Type = tile.Type
                            });
                        }
                    }
                }

                BoardSequence sequence = new()
                {
                    MatchedPosition = matchedPosition,
                    MovedTiles = movedTilesList,
                    AddedTiles = addedTiles
                };
                boardSequences.Add(sequence);
                matchedTiles = MatchFinder.FindMatches(board);
            }

            return boardSequences;
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

using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private int _boardHeight = 10;
        [SerializeField] private int _boardWidth = 10;

        private GameService _gameService;
        private bool _isAnimating;
        private bool _hasStarted;
        private int _selectedX = -1;
        private int _selectedY = -1;

        public Board Board => _gameService?.Board;
        public bool IsAnimating => _isAnimating;

        #region Unity
        private void Awake()
        {
            _gameService ??= new GameService();
            _boardView.TileClicked += OnTileClick;
        }

        private void OnDestroy()
        {
            _boardView.TileClicked -= OnTileClick;
        }

        private void Start()
        {
            if (_hasStarted)
            {
                return;
            }

            CreateBoard(_gameService.StartGame(_boardWidth, _boardHeight));
        }
        #endregion

        public void StartGame(Board board, IReadOnlyList<int> colors)
        {
            if (_hasStarted)
            {
                throw new InvalidOperationException("The game has already started.");
            }

            _gameService ??= new GameService();
            CreateBoard(_gameService.StartGame(board, colors));
        }

        private void CreateBoard(Board board)
        {
            _hasStarted = true;
            _boardView.CreateBoard(board);
        }

        private void AnimateBoard(IReadOnlyList<BoardSequence> boardSequences, int index, Action onComplete)
        {
            BoardSequence boardSequence = boardSequences[index];

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_boardView.PlaySpecialActivations(boardSequence.SpecialActivations));
            sequence.Append(_boardView.DestroyTiles(boardSequence.MatchedPosition));
            _boardView.ApplyCreatedSpecials(boardSequence.CreatedSpecialTiles);
            sequence.Append(_boardView.MoveTiles(boardSequence.MovedTiles));
            sequence.Append(_boardView.CreateTile(boardSequence.AddedTiles));

            index += 1;
            if (index < boardSequences.Count)
            {
                sequence.onComplete += () => AnimateBoard(boardSequences, index, onComplete);
            }
            else
            {
                sequence.onComplete += () => onComplete();
            }
        }

        private void OnTileClick(int x, int y)
        {
            if (_isAnimating) return;

            if (_selectedX > -1 && _selectedY > -1)
            {
                if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) > 1)
                {
                    _selectedX = -1;
                    _selectedY = -1;
                }
                else
                {
                    _isAnimating = true;
                    _boardView.SwapTiles(_selectedX, _selectedY, x, y).onComplete += () =>
                    {
                        MoveResult result = _gameService.TrySwap(_selectedX, _selectedY, x, y);
                        if (result.IsValid)
                        {
                            AnimateBoard(result.BoardSequences, 0, () => _isAnimating = false);
                        }
                        else
                        {
                            _boardView.SwapTiles(x, y, _selectedX, _selectedY).onComplete += () => _isAnimating = false;
                        }
                        _selectedX = -1;
                        _selectedY = -1;
                    };
                }
            }
            else
            {
                _selectedX = x;
                _selectedY = y;
            }
        }
    }
}

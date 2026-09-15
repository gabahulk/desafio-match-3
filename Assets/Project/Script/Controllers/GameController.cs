using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using UnityEngine;
using UnityEngine.UI;

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
        private Text _scoreText;

        public Board Board => _gameService?.Board;
        public bool IsAnimating => _isAnimating;
        public int Score => _gameService?.Score ?? 0;

        #region Unity
        private void Awake()
        {
            _gameService ??= new GameService();
            _boardView.TileClicked += OnTileClick;
            CreateScoreDisplay();
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
            UpdateScore(0);
            _boardView.CreateBoard(board);
        }

        private void CreateScoreDisplay()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            GameObject scoreObject = new("Score", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            scoreObject.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = scoreObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1.0f);
            rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
            rectTransform.pivot = new Vector2(0.5f, 1.0f);
            rectTransform.anchoredPosition = new Vector2(0.0f, -8.0f);
            rectTransform.sizeDelta = new Vector2(240.0f, 64.0f);

            _scoreText = scoreObject.GetComponent<Text>();
            _scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _scoreText.fontSize = 24;
            _scoreText.alignment = TextAnchor.MiddleCenter;
            _scoreText.color = Color.white;
            _scoreText.raycastTarget = false;
            UpdateScore(0);
        }

        private void UpdateScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"SCORE\n{score}";
            }
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

            int nextIndex = index + 1;
            sequence.onComplete += () =>
            {
                UpdateScore(boardSequence.TotalScore);
                if (nextIndex < boardSequences.Count)
                {
                    AnimateBoard(boardSequences, nextIndex, onComplete);
                }
                else
                {
                    onComplete();
                }
            };
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

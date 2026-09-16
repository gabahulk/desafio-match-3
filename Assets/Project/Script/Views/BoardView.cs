using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public class BoardView : MonoBehaviour
    {
        public event Action<int, int> TileClicked;

        [SerializeField] private GridLayoutGroup _boardContainer;
        [SerializeField] private BoardResponsiveController _responsiveController;
        [SerializeField] private TileVisualRepository _tileVisualRepository;
        [SerializeField] private TileSpotView _tileSpotPrefab;

        private GameObject[][] _tiles;
        private TileSpotView[][] _tileSpots;

        public void CreateBoard(Board board)
        {
            _responsiveController.SetBoardSize(board.Width, board.Height);
            _tiles = new GameObject[board.Height][];
            _tileSpots = new TileSpotView[board.Height][];

            for (int y = 0; y < board.Height; y++)
            {
                _tiles[y] = new GameObject[board.Width];
                _tileSpots[y] = new TileSpotView[board.Width];

                for (int x = 0; x < board.Width; x++)
                {
                    TileSpotView tileSpot = Instantiate(_tileSpotPrefab);
                    tileSpot.transform.SetParent(_boardContainer.transform, false);
                    tileSpot.SetPosition(x, y);
                    tileSpot.Clicked += TileSpot_Clicked;

                    _tileSpots[y][x] = tileSpot;

                    if (!board[x, y].IsEmpty)
                    {
                        Tile tileData = board[x, y];
                        GameObject tile = Instantiate(_tileVisualRepository.TilePrefab);
                        tileSpot.SetTile(tile);
                        ApplyTileVisual(tile, tileData.Color, tileData.Special);

                        _tiles[y][x] = tile;
                    }
                }
            }
        }

        public Tween CreateTile(List<AddedTileInfo> addedTiles)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < addedTiles.Count; i++)
            {
                AddedTileInfo addedTileInfo = addedTiles[i];
                Vector2Int position = addedTileInfo.Position;

                TileSpotView tileSpot = _tileSpots[position.y][position.x];

                GameObject tile = Instantiate(_tileVisualRepository.TilePrefab);
                tileSpot.SetTile(tile);
                ApplyTileVisual(tile, addedTileInfo.Color, SpecialType.None);

                _tiles[position.y][position.x] = tile;

                tile.transform.DOKill();
                tile.transform.localScale = Vector3.one * 0.75f;
                sequence.Join(tile.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
            }

            return sequence;
        }

        public void ApplyCreatedSpecials(List<SpecialTileInfo> createdSpecialTiles)
        {
            ApplySpecialVisuals(createdSpecialTiles);
        }

        public void ApplySpecialTransformations(List<SpecialTileInfo> transformedSpecialTiles)
        {
            ApplySpecialVisuals(transformedSpecialTiles);
        }

        private void ApplySpecialVisuals(IReadOnlyList<SpecialTileInfo> specialTiles)
        {
            for (int index = 0; index < specialTiles.Count; index++)
            {
                SpecialTileInfo specialTile = specialTiles[index];
                GameObject tile = _tiles[specialTile.Position.y][specialTile.Position.x];
                ApplyTileVisual(tile, specialTile.Color, specialTile.Special);

                if (tile != null)
                {
                    tile.transform.DOKill();
                    tile.transform.DOPunchScale(Vector3.one * 0.14f, 0.18f, 4);
                }
            }
        }

        public Tween DestroyTiles(List<Vector2Int> matchedPositions)
        {
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < matchedPositions.Count; i++)
            {
                Vector2Int position = matchedPositions[i];
                GameObject tile = _tiles[position.y][position.x];
                if (tile != null)
                {
                    tile.transform.DOKill();
                    tile.transform.localScale = Vector3.one;

                    Sequence destruction = DOTween.Sequence()
                        .Append(tile.transform.DOScale(Vector3.one * 1.12f, 0.08f).SetEase(Ease.OutQuad))
                        .Append(tile.transform.DOScale(Vector3.zero, 0.12f).SetEase(Ease.InBack))
                        .OnComplete(() => Destroy(tile));
                    sequence.Join(destruction);
                }

                _tiles[position.y][position.x] = null;
            }

            return sequence;
        }

        public Tween PlaySpecialActivations(List<SpecialActivationInfo> activations)
        {
            Sequence sequence = DOTween.Sequence();
            for (int index = 0; index < activations.Count; index++)
            {
                SpecialActivationInfo activation = activations[index];
                if (activation.Special != SpecialType.ColorBomb)
                {
                    continue;
                }

                GameObject tile = _tiles[activation.Position.y][activation.Position.x];
                if (tile != null)
                {
                    tile.transform.DOKill();
                    sequence.Join(tile.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 4));
                }
            }

            return sequence;
        }

        public Tween MoveTiles(List<MovedTileInfo> movedTiles)
        {
            GameObject[][] tiles = new GameObject[_tiles.Length][];
            for (int y = 0; y < _tiles.Length; y++)
            {
                tiles[y] = new GameObject[_tiles[y].Length];
                for (int x = 0; x < _tiles[y].Length; x++)
                {
                    tiles[y][x] = _tiles[y][x];
                }
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < movedTiles.Count; i++)
            {
                MovedTileInfo movedTileInfo = movedTiles[i];

                Vector2Int from = movedTileInfo.From;
                Vector2Int to = movedTileInfo.To;

                sequence.Join(_tileSpots[to.y][to.x].AnimatedSetTile(_tiles[from.y][from.x]));

                tiles[to.y][to.x] = _tiles[from.y][from.x];
            }

            _tiles = tiles;

            return sequence;
        }

        public Tween SwapTiles(int fromX, int fromY, int toX, int toY)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_tileSpots[fromY][fromX].AnimatedSetTile(_tiles[toY][toX]));
            sequence.Join(_tileSpots[toY][toX].AnimatedSetTile(_tiles[fromY][fromX]));

            (_tiles[toY][toX], _tiles[fromY][fromX]) = (_tiles[fromY][fromX], _tiles[toY][toX]);

            return sequence;
        }

        private void ApplyTileVisual(GameObject tile, int color, SpecialType special)
        {
            if (tile == null)
            {
                return;
            }

            Image image = tile.GetComponent<Image>();
            image.sprite = _tileVisualRepository.GetSprite(color, special);
        }

        #region Events
        private void TileSpot_Clicked(int x, int y)
        {
            TileClicked(x, y);
        }
        #endregion
    }
}

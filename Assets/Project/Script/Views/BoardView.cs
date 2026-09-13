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
        [SerializeField] private TilePrefabRepository _tilePrefabRepository;
        [SerializeField] private TileSpotView _tileSpotPrefab;

        private GameObject[][] _tiles;
        private TileSpotView[][] _tileSpots;

        public void CreateBoard(Board board)
        {
            _boardContainer.constraintCount = board.Width;
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
                        GameObject tilePrefab = GetTilePrefab(board[x, y]);
                        GameObject tile = Instantiate(tilePrefab);
                        tileSpot.SetTile(tile);
                        ApplySpecialVisual(tile, board[x, y].Special);

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

                GameObject tilePrefab = _tilePrefabRepository.ColorPrefabList[addedTileInfo.Color];
                GameObject tile = Instantiate(tilePrefab);
                tileSpot.SetTile(tile);

                _tiles[position.y][position.x] = tile;

                tile.transform.localScale = Vector2.zero;
                sequence.Join(tile.transform.DOScale(1.0f, 0.2f));
            }

            return sequence;
        }

        public void ApplyCreatedSpecials(List<SpecialTileInfo> createdSpecialTiles)
        {
            for (int index = 0; index < createdSpecialTiles.Count; index++)
            {
                SpecialTileInfo specialTile = createdSpecialTiles[index];
                GameObject tile = _tiles[specialTile.Position.y][specialTile.Position.x];
                ApplySpecialVisual(tile, specialTile.Special);
            }
        }

        public Tween DestroyTiles(List<Vector2Int> matchedPosition)
        {
            for (int i = 0; i < matchedPosition.Count; i++)
            {
                Vector2Int position = matchedPosition[i];
                Destroy(_tiles[position.y][position.x]);
                _tiles[position.y][position.x] = null;
            }

            return DOVirtual.DelayedCall(0.2f, () => { });
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

        private static void ApplySpecialVisual(GameObject tile, SpecialType special)
        {
            if (tile == null || special == SpecialType.None)
            {
                return;
            }

            string overlayName = $"{special} Overlay";
            if (tile.transform.Find(overlayName) != null)
            {
                return;
            }

            if (special == SpecialType.Wrapped)
            {
                CreateWrappedOverlay(tile, overlayName);
                return;
            }

            if (special == SpecialType.ColorBomb)
            {
                CreateColorBombOverlay(tile, overlayName);
                return;
            }

            if (special != SpecialType.HorizontalStriped &&
                special != SpecialType.VerticalStriped)
            {
                return;
            }

            GameObject overlay = new(overlayName, typeof(RectTransform), typeof(Image));
            RectTransform rectTransform = (RectTransform)overlay.transform;
            rectTransform.SetParent(tile.transform, false);

            if (special == SpecialType.HorizontalStriped)
            {
                rectTransform.anchorMin = new Vector2(0.1f, 0.42f);
                rectTransform.anchorMax = new Vector2(0.9f, 0.58f);
            }
            else
            {
                rectTransform.anchorMin = new Vector2(0.42f, 0.1f);
                rectTransform.anchorMax = new Vector2(0.58f, 0.9f);
            }

            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Image stripe = overlay.GetComponent<Image>();
            stripe.color = new Color(1.0f, 1.0f, 1.0f, 0.85f);
            stripe.raycastTarget = false;
        }

        private GameObject GetTilePrefab(Tile tile)
        {
            int colorIndex = tile.Special == SpecialType.ColorBomb ? 0 : tile.Color;
            return _tilePrefabRepository.ColorPrefabList[colorIndex];
        }

        private static void CreateColorBombOverlay(GameObject tile, string overlayName)
        {
            GameObject overlay = new(overlayName, typeof(RectTransform), typeof(Image));
            RectTransform rect = (RectTransform)overlay.transform;
            rect.SetParent(tile.transform, false);
            rect.anchorMin = new Vector2(0.18f, 0.18f);
            rect.anchorMax = new Vector2(0.82f, 0.82f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = overlay.GetComponent<Image>();
            image.color = new Color(0.18f, 0.08f, 0.35f, 1.0f);
            image.raycastTarget = false;
        }

        private static void CreateWrappedOverlay(GameObject tile, string overlayName)
        {
            GameObject overlay = new(overlayName, typeof(RectTransform));
            RectTransform overlayRect = (RectTransform)overlay.transform;
            overlayRect.SetParent(tile.transform, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            CreateWrappedEdge(overlayRect, "Top", new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.92f));
            CreateWrappedEdge(overlayRect, "Bottom", new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.22f));
            CreateWrappedEdge(overlayRect, "Left", new Vector2(0.08f, 0.22f), new Vector2(0.22f, 0.78f));
            CreateWrappedEdge(overlayRect, "Right", new Vector2(0.78f, 0.22f), new Vector2(0.92f, 0.78f));
        }

        private static void CreateWrappedEdge(
            RectTransform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject edge = new(name, typeof(RectTransform), typeof(Image));
            RectTransform edgeRect = (RectTransform)edge.transform;
            edgeRect.SetParent(parent, false);
            edgeRect.anchorMin = anchorMin;
            edgeRect.anchorMax = anchorMax;
            edgeRect.offsetMin = Vector2.zero;
            edgeRect.offsetMax = Vector2.zero;

            Image image = edge.GetComponent<Image>();
            image.color = new Color(1.0f, 0.72f, 0.15f, 0.95f);
            image.raycastTarget = false;
        }

        #region Events
        private void TileSpot_Clicked(int x, int y)
        {
            TileClicked(x, y);
        }
        #endregion
    }
}

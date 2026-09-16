using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    [RequireComponent(typeof(RectTransform), typeof(AspectRatioFitter), typeof(GridLayoutGroup))]
    public sealed class BoardResponsiveController : MonoBehaviour
    {
        [SerializeField] private RectTransform _boardContainer;
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;

        private int _boardWidth;
        private int _boardHeight;

        public void SetBoardSize(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            _boardWidth = width;
            _boardHeight = height;

            _aspectRatioFitter.aspectRatio = (float)width / height;
            _gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _gridLayoutGroup.constraintCount = width;

            RectTransform layoutRoot = _boardContainer.parent as RectTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot ?? _boardContainer);
            RecalculateCellSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            RecalculateCellSize();
        }

        private void RecalculateCellSize()
        {
            if (_boardWidth <= 0 || _boardHeight <= 0 ||
                _boardContainer == null || _gridLayoutGroup == null)
            {
                return;
            }

            float cellWidth = _boardContainer.rect.width / _boardWidth;
            float cellHeight = _boardContainer.rect.height / _boardHeight;
            float cellSize = Mathf.Min(cellWidth, cellHeight);
            _gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
        }
    }
}

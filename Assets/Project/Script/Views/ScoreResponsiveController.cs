using UnityEngine;

namespace Gazeus.DesafioMatch3.Views
{
    public sealed class ScoreResponsiveController : MonoBehaviour
    {
        [SerializeField] private RectTransform _canvasRect;
        [SerializeField] private RectTransform _boardFrame;
        [SerializeField] private RectTransform _scoreRect;
        [SerializeField] private float _portraitWidth = 360.0f;
        [SerializeField] private float _landscapeWidth = 300.0f;
        [SerializeField] private float _plaqueAspectRatio = 3.0f;
        [SerializeField] private float _verticalGap = 16.0f;
        [SerializeField] private float _topInset = 20.0f;

        private Vector2 _lastCanvasSize;
        private Rect _lastFrameRect;

        private void OnEnable()
        {
            ApplyLayout();
        }

        private void LateUpdate()
        {
            if (_canvasRect == null || _boardFrame == null || _scoreRect == null)
            {
                return;
            }

            if (_lastCanvasSize != _canvasRect.rect.size || _lastFrameRect != _boardFrame.rect)
            {
                ApplyLayout();
            }
        }

        private void ApplyLayout()
        {
            if (_canvasRect == null || _boardFrame == null || _scoreRect == null || _plaqueAspectRatio <= 0.0f)
            {
                return;
            }

            bool isPortrait = _canvasRect.rect.height > _canvasRect.rect.width;
            float width = isPortrait ? _portraitWidth : _landscapeWidth;
            _scoreRect.sizeDelta = new Vector2(width, width / _plaqueAspectRatio);

            Vector3 frameTopInCanvas = _canvasRect.InverseTransformPoint(
                _boardFrame.TransformPoint(new Vector3(0.0f, _boardFrame.rect.yMax, 0.0f)));
            float scoreHalfHeight = _scoreRect.rect.height * 0.5f;
            float desiredY = frameTopInCanvas.y + _verticalGap + scoreHalfHeight;
            float maximumY = _canvasRect.rect.yMax - _topInset - scoreHalfHeight;
            _scoreRect.anchoredPosition = new Vector2(0.0f, Mathf.Min(desiredY, maximumY));

            _lastCanvasSize = _canvasRect.rect.size;
            _lastFrameRect = _boardFrame.rect;
        }
    }
}

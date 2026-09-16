using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public sealed class BoardFrameResponsiveController : MonoBehaviour
    {
        [Serializable]
        private struct LayoutProfile
        {
            [SerializeField] private Vector2 _anchorMin;
            [SerializeField] private Vector2 _anchorMax;
            [SerializeField] private Sprite _background;

            public Vector2 AnchorMin => _anchorMin;
            public Vector2 AnchorMax => _anchorMax;
            public Sprite Background => _background;
        }

        [SerializeField] private RectTransform _canvasRect;
        [SerializeField] private RectTransform _boardFrame;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private LayoutProfile _portraitLayout;
        [SerializeField] private LayoutProfile _landscapeLayout;

        private bool? _isPortrait;

        private void OnEnable()
        {
            ApplyCurrentProfile(true);
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyCurrentProfile(false);
        }

        private void ApplyCurrentProfile(bool force)
        {
            if (_canvasRect == null || _boardFrame == null || _backgroundImage == null)
            {
                return;
            }

            bool isPortrait = _canvasRect.rect.height > _canvasRect.rect.width;
            if (!force && _isPortrait == isPortrait)
            {
                return;
            }

            _isPortrait = isPortrait;
            LayoutProfile profile = isPortrait ? _portraitLayout : _landscapeLayout;
            _boardFrame.anchorMin = profile.AnchorMin;
            _boardFrame.anchorMax = profile.AnchorMax;
            _boardFrame.offsetMin = Vector2.zero;
            _boardFrame.offsetMax = Vector2.zero;
            _backgroundImage.sprite = profile.Background;
        }
    }
}

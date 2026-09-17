using System;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Controllers
{
    public sealed class MainMenuResponsiveController : MonoBehaviour
    {
        [Serializable]
        private struct LayoutProfile
        {
            [SerializeField] private Vector2 _titlePosition;
            [SerializeField] private Vector2 _suitGroupPosition;
            [SerializeField] private Vector2 _jokerPosition;
            [SerializeField] private Vector2 _playPosition;
            [SerializeField] private float _titleScale;
            [SerializeField] private float _suitGroupScale;
            [SerializeField] private float _jokerScale;
            [SerializeField] private float _playScale;
            [SerializeField] private Vector2[] _suitPositions;

            public Vector2 TitlePosition => _titlePosition;
            public Vector2 SuitGroupPosition => _suitGroupPosition;
            public Vector2 JokerPosition => _jokerPosition;
            public Vector2 PlayPosition => _playPosition;
            public float TitleScale => _titleScale;
            public float SuitGroupScale => _suitGroupScale;
            public float JokerScale => _jokerScale;
            public float PlayScale => _playScale;
            public Vector2[] SuitPositions => _suitPositions;
        }

        [SerializeField] private RectTransform _canvas;
        [SerializeField] private RectTransform _title;
        [SerializeField] private RectTransform _suitGroup;
        [SerializeField] private RectTransform[] _suits;
        [SerializeField] private RectTransform _joker;
        [SerializeField] private RectTransform _playButton;
        [SerializeField] private LayoutProfile _portraitLayout;
        [SerializeField] private LayoutProfile _landscapeLayout;

        private bool? _isPortrait;

        public event Action LayoutChanged;

        private void OnEnable()
        {
            ApplyCurrentProfile(true);
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyCurrentProfile(false);
        }

        public void ApplyCurrentProfile()
        {
            ApplyCurrentProfile(true);
        }

        private void ApplyCurrentProfile(bool force)
        {
            if (_canvas == null || _title == null || _suitGroup == null || _joker == null || _playButton == null)
            {
                return;
            }

            bool isPortrait = _canvas.rect.height > _canvas.rect.width;
            if (!force && _isPortrait == isPortrait)
            {
                return;
            }

            _isPortrait = isPortrait;
            LayoutProfile profile = isPortrait ? _portraitLayout : _landscapeLayout;
            _title.anchoredPosition = profile.TitlePosition;
            _title.localScale = Vector3.one * profile.TitleScale;
            _suitGroup.anchoredPosition = profile.SuitGroupPosition;
            _suitGroup.localScale = Vector3.one * profile.SuitGroupScale;
            _joker.anchoredPosition = profile.JokerPosition;
            _joker.localScale = Vector3.one * profile.JokerScale;
            _playButton.anchoredPosition = profile.PlayPosition;
            _playButton.localScale = Vector3.one * profile.PlayScale;

            Vector2[] suitPositions = profile.SuitPositions;
            for (int index = 0; index < _suits.Length && index < suitPositions.Length; index++)
            {
                _suits[index].anchoredPosition = suitPositions[index];
            }

            LayoutChanged?.Invoke();
        }
    }
}

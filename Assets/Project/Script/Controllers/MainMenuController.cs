using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Controllers
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private RectTransform[] _suits;
        [SerializeField] private RectTransform _joker;
        [SerializeField] private RectTransform _title;
        [SerializeField] private CanvasGroup _titleCanvasGroup;
        [SerializeField] private RectTransform _playButtonTransform;
        [SerializeField] private CanvasGroup _playButtonCanvasGroup;
        [SerializeField] private Button _playButton;
        [SerializeField] private MainMenuResponsiveController _responsiveController;

        private readonly Vector2[] _suitEntryDirections =
        {
            new Vector2(-1.0f, 1.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(-1.0f, -1.0f),
            new Vector2(1.0f, -1.0f)
        };

        private bool _introComplete;
        private bool _isTransitioning;
        private bool _hasStarted;
        private Sequence _introSequence;

        private void Awake()
        {
            _playButton.interactable = false;
            _playButton.onClick.AddListener(Play);
            _responsiveController.LayoutChanged += OnLayoutChanged;
        }

        private void Start()
        {
            _responsiveController.ApplyCurrentProfile();
            _hasStarted = true;
            PlayIntro();
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(Play);
            _responsiveController.LayoutChanged -= OnLayoutChanged;
        }

        public void Play()
        {
            if (!_introComplete || _isTransitioning)
            {
                return;
            }

            _isTransitioning = true;
            _playButton.interactable = false;
            _introSequence?.Kill();
            _joker.DOKill();
            _playButtonTransform.DOKill();
            _title.DOKill();
            AudioManager.Instance?.PlayButtonClick();

            MenuGameplayTransition.Begin(
                (RectTransform)transform,
                _suits,
                _joker,
                _title,
                _titleCanvasGroup,
                _playButtonTransform,
                _playButtonCanvasGroup);
        }

        private void PlayIntro()
        {
            RectTransform canvas = (RectTransform)transform;
            Vector2 canvasSize = canvas.rect.size;
            Color backgroundColor = _background.color;
            backgroundColor.a = 0.45f;
            _background.color = backgroundColor;

            Vector2[] suitFinalPositions = new Vector2[_suits.Length];
            for (int index = 0; index < _suits.Length; index++)
            {
                RectTransform suit = _suits[index];
                suitFinalPositions[index] = suit.anchoredPosition;
                Vector2 direction = _suitEntryDirections[index];
                suit.anchoredPosition += Vector2.Scale(direction, canvasSize * 0.5f);
                suit.localScale = Vector3.one * 0.9f;
                suit.localRotation = Quaternion.Euler(0.0f, 0.0f, direction.x * 14.0f);
            }

            Vector2 jokerFinalPosition = _joker.anchoredPosition;
            Vector3 jokerFinalScale = _joker.localScale;
            _joker.anchoredPosition += Vector2.down * canvasSize.y * 0.12f;
            _joker.localScale = jokerFinalScale * 0.7f;
            _joker.localRotation = Quaternion.Euler(0.0f, 0.0f, -10.0f);

            Vector3 titleFinalScale = _title.localScale;
            _title.localScale = titleFinalScale * 0.9f;
            _titleCanvasGroup.alpha = 0.0f;

            Vector2 playFinalPosition = _playButtonTransform.anchoredPosition;
            Vector3 playFinalScale = _playButtonTransform.localScale;
            _playButtonTransform.anchoredPosition += Vector2.down * canvasSize.y * 0.16f;
            _playButtonTransform.localScale = playFinalScale * 0.9f;
            _playButtonCanvasGroup.alpha = 0.0f;

            _introSequence = DOTween.Sequence();
            Color visibleBackground = _background.color;
            visibleBackground.a = 1.0f;
            _introSequence.Join(DOTween.To(() => _background.color, color => _background.color = color,
                visibleBackground, 0.2f));

            for (int index = 0; index < _suits.Length; index++)
            {
                RectTransform suit = _suits[index];
                float delay = index * 0.08f;
                _introSequence.Insert(delay, DOTween.To(() => suit.anchoredPosition,
                    position => suit.anchoredPosition = position, suitFinalPositions[index], 0.32f)
                    .SetEase(Ease.OutBack));
                _introSequence.Insert(delay, suit.DOScale(Vector3.one, 0.32f).SetEase(Ease.OutBack));
                _introSequence.Insert(delay, suit.DORotate(Vector3.zero, 0.32f, RotateMode.Fast).SetEase(Ease.OutQuad));
            }

            _introSequence.Insert(0.3f, DOTween.To(() => _joker.anchoredPosition,
                position => _joker.anchoredPosition = position, jokerFinalPosition, 0.35f).SetEase(Ease.OutBack));
            _introSequence.Insert(0.3f, _joker.DOScale(jokerFinalScale, 0.35f).SetEase(Ease.OutBack));
            _introSequence.Insert(0.3f, _joker.DORotate(Vector3.zero, 0.35f, RotateMode.Fast).SetEase(Ease.OutQuad));
            _introSequence.Insert(0.58f, _joker.DOPunchScale(jokerFinalScale * 0.1f, 0.16f, 4));

            _introSequence.Insert(0.36f, _title.DOScale(titleFinalScale, 0.24f).SetEase(Ease.OutQuad));
            _introSequence.Insert(0.36f, Fade(_titleCanvasGroup, 1.0f, 0.24f));

            _introSequence.Insert(0.6f, DOTween.To(() => _playButtonTransform.anchoredPosition,
                position => _playButtonTransform.anchoredPosition = position, playFinalPosition, 0.3f)
                .SetEase(Ease.OutBack));
            _introSequence.Insert(0.6f, _playButtonTransform.DOScale(playFinalScale, 0.3f).SetEase(Ease.OutBack));
            _introSequence.Insert(0.6f, Fade(_playButtonCanvasGroup, 1.0f, 0.2f));
            _introSequence.OnComplete(() => BeginIdle(playFinalPosition, jokerFinalPosition));
        }

        private void BeginIdle(Vector2 playPosition, Vector2 jokerPosition)
        {
            _introComplete = true;
            _playButton.interactable = true;
            DOTween.To(() => _joker.anchoredPosition,
                    position => _joker.anchoredPosition = position,
                    jokerPosition + Vector2.up * 8.0f,
                    1.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
            DOTween.To(() => _playButtonTransform.anchoredPosition,
                    position => _playButtonTransform.anchoredPosition = position,
                    playPosition + Vector2.up * 3.0f,
                    1.4f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnLayoutChanged()
        {
            if (!_hasStarted || _isTransitioning)
            {
                return;
            }

            _introSequence?.Kill();
            _joker.DOKill();
            _playButtonTransform.DOKill();
            _title.DOKill();
            _titleCanvasGroup.alpha = 1.0f;
            _playButtonCanvasGroup.alpha = 1.0f;
            BeginIdle(_playButtonTransform.anchoredPosition, _joker.anchoredPosition);
        }

        private static Tween Fade(CanvasGroup canvasGroup, float endValue, float duration)
        {
            return DOTween.To(() => canvasGroup.alpha, value => canvasGroup.alpha = value, endValue, duration);
        }
    }
}

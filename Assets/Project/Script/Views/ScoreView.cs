using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _valueText;

        private Image _plaque;
        private Tween _scoreTween;
        private int _displayedScore;

        private void Awake()
        {
            _plaque = transform.Find("Plaque").GetComponent<Image>();
        }

        public void UpdateScore(int score)
        {
            _scoreTween?.Kill();
            _scoreTween = null;
            SetDisplayedScore(score);
        }

        public void ShowScoreFeedback(int scoreGained, int totalScore, Vector3 matchCenter)
        {
            AnimateTotalScore(totalScore);
            CreateScorePopup(scoreGained, matchCenter);
        }

        private void AnimateTotalScore(int totalScore)
        {
            _scoreTween?.Kill();

            int startScore = _displayedScore;
            _scoreTween = DOTween.To(
                    () => startScore,
                    SetDisplayedScore,
                    totalScore,
                    0.3f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => _scoreTween = null);

            _valueText.transform.DOKill();
            _plaque.transform.DOKill();
            _valueText.transform.DOPunchScale(Vector3.one * 0.08f, 0.16f, 4);
            _plaque.transform.DOPunchScale(Vector3.one * 0.04f, 0.16f, 4);
        }

        private void CreateScorePopup(int scoreGained, Vector3 matchCenter)
        {
            if (scoreGained <= 0)
            {
                return;
            }

            GameObject popupObject = new("Score Popup", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            RectTransform popupTransform = popupObject.GetComponent<RectTransform>();
            popupTransform.SetParent(transform.parent, false);
            popupTransform.anchorMin = new Vector2(0.5f, 0.5f);
            popupTransform.anchorMax = new Vector2(0.5f, 0.5f);
            popupTransform.pivot = new Vector2(0.5f, 0.5f);
            popupTransform.sizeDelta = new Vector2(180.0f, 60.0f);
            popupTransform.anchoredPosition = GetCanvasPosition(matchCenter);
            popupTransform.localScale = Vector3.one * 0.75f;

            TextMeshProUGUI popupText = popupObject.GetComponent<TextMeshProUGUI>();
            popupText.font = _valueText.font;
            popupText.fontSize = 34.0f;
            popupText.alignment = TextAlignmentOptions.Center;
            popupText.color = new Color(1.0f, 0.79f, 0.43f, 1.0f);
            popupText.text = $"+{scoreGained.ToString("N0", CultureInfo.InvariantCulture)}";
            popupText.raycastTarget = false;

            Vector2 popupDestination = popupTransform.anchoredPosition + Vector2.up * 60.0f;
            Color transparentPopupColor = popupText.color;
            transparentPopupColor.a = 0.0f;
            Sequence popupSequence = DOTween.Sequence()
                .Append(popupTransform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack))
                .Join(DOTween.To(
                        () => popupTransform.anchoredPosition,
                        position => popupTransform.anchoredPosition = position,
                        popupDestination,
                        0.6f)
                    .SetEase(Ease.OutQuad))
                .Join(DOTween.To(() => popupText.color, color => popupText.color = color,
                    transparentPopupColor, 0.6f))
                .OnComplete(() => Destroy(popupObject));
        }

        private Vector2 GetCanvasPosition(Vector3 worldPosition)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            RectTransform canvasTransform = (RectTransform)canvas.transform;
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasTransform, screenPosition, camera,
                out Vector2 canvasPosition);
            return canvasPosition;
        }

        private void SetDisplayedScore(int score)
        {
            _displayedScore = score;
            _valueText.text = score.ToString("N0", CultureInfo.InvariantCulture);
        }
    }
}

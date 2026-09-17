using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Controllers
{
    /// <summary>
    /// A short, single-purpose visual bridge from the menu to the gameplay scene.
    /// The overlay remains alive through the scene load so input stays covered until reveal.
    /// </summary>
    public sealed class MenuGameplayTransition : MonoBehaviour
    {
        private const string GameplaySceneName = "Gameplay";

        private CanvasGroup _canvasGroup;
        private Image _overlay;
        private RectTransform _joker;

        public static void Begin(
            RectTransform menuCanvas,
            RectTransform[] suits,
            RectTransform menuJoker,
            RectTransform title,
            CanvasGroup titleCanvasGroup,
            RectTransform playButton,
            CanvasGroup playCanvasGroup)
        {
            GameObject transitionObject = new("Menu Gameplay Transition", typeof(RectTransform), typeof(MenuGameplayTransition));
            MenuGameplayTransition transition = transitionObject.GetComponent<MenuGameplayTransition>();
            transition.StartTransition(menuCanvas, suits, menuJoker, title, titleCanvasGroup, playButton, playCanvasGroup);
        }

        private void StartTransition(
            RectTransform menuCanvas,
            RectTransform[] suits,
            RectTransform menuJoker,
            RectTransform title,
            CanvasGroup titleCanvasGroup,
            RectTransform playButton,
            CanvasGroup playCanvasGroup)
        {
            DontDestroyOnLoad(gameObject);
            CreateOverlay(menuCanvas, menuJoker);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(playButton.DOScale(playButton.localScale * 0.92f, 0.05f));
            sequence.Append(playButton.DOScale(playButton.localScale / 0.92f, 0.05f));

            for (int index = 0; index < suits.Length; index++)
            {
                RectTransform suit = suits[index];
                float direction = index is 0 or 2 ? -1.0f : 1.0f;
                Vector2 targetPosition = suit.anchoredPosition + Vector2.right * direction * menuCanvas.rect.width * 0.55f;
                sequence.Insert(0.05f, TweenAnchorPosition(suit, targetPosition, 0.24f).SetEase(Ease.InQuad));
                sequence.Insert(0.05f, suit.DOScale(suit.localScale * 0.9f, 0.24f));
            }

            sequence.Insert(0.05f, TweenAnchorPosition(title, title.anchoredPosition + Vector2.up * 70.0f, 0.22f)
                .SetEase(Ease.OutQuad));
            sequence.Insert(0.05f, Fade(titleCanvasGroup, 0.0f, 0.2f));
            sequence.Insert(0.05f, TweenAnchorPosition(playButton, playButton.anchoredPosition + Vector2.down * 90.0f, 0.22f)
                .SetEase(Ease.InQuad));
            sequence.Insert(0.05f, Fade(playCanvasGroup, 0.0f, 0.18f));

            sequence.Insert(0.16f, _joker.DOPunchScale(_joker.localScale * 0.15f, 0.12f, 4));
            sequence.Insert(0.18f, _joker.DORotate(new Vector3(0.0f, 0.0f, 12.0f), 0.14f, RotateMode.Fast));
            sequence.Insert(0.27f, _joker.DOScale(GetTakeoverScale(menuCanvas, _joker), 0.3f).SetEase(Ease.InCubic));
            sequence.Insert(0.28f, Fade(_overlay, 1.0f, 0.28f));
            sequence.OnComplete(LoadGameplay);
        }

        private void CreateOverlay(RectTransform menuCanvas, RectTransform menuJoker)
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080.0f, 1920.0f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            RectTransform root = (RectTransform)transform;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            _overlay = CreateImage("Overlay", root);
            Stretch(_overlay.rectTransform);
            _overlay.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            _overlay.color = new Color(0.07f, 0.03f, 0.16f, 0.0f);
            _overlay.raycastTarget = true;

            Image menuJokerImage = menuJoker.GetComponent<Image>();
            _joker = CreateImage("Joker", root).rectTransform;
            Image jokerImage = _joker.GetComponent<Image>();
            jokerImage.sprite = menuJokerImage.sprite;
            jokerImage.preserveAspect = true;
            jokerImage.raycastTarget = false;
            _joker.sizeDelta = menuJoker.rect.size;
            _joker.localScale = menuJoker.localScale;
            _joker.localRotation = menuJoker.localRotation;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                root,
                RectTransformUtility.WorldToScreenPoint(null, menuJoker.position),
                null,
                out Vector2 localPosition);
            _joker.anchoredPosition = localPosition;
            menuJokerImage.enabled = false;
        }

        private static Image CreateImage(string objectName, RectTransform parent)
        {
            GameObject child = new(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child.transform.SetParent(parent, false);
            return child.GetComponent<Image>();
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static Vector3 GetTakeoverScale(RectTransform menuCanvas, RectTransform joker)
        {
            float jokerSize = Mathf.Max(1.0f, Mathf.Min(joker.rect.width, joker.rect.height));
            float canvasDiagonal = menuCanvas.rect.size.magnitude;
            float multiplier = canvasDiagonal / jokerSize * 1.2f;
            return joker.localScale * multiplier;
        }

        private void LoadGameplay()
        {
            SceneManager.sceneLoaded += RevealGameplay;
            SceneManager.LoadSceneAsync(GameplaySceneName);
        }

        private void RevealGameplay(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= RevealGameplay;
            GameObject boardFrame = GameObject.Find("BoardFrame");
            if (boardFrame != null)
            {
                boardFrame.transform.localScale = Vector3.one * 0.95f;
                boardFrame.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }

            DOTween.To(() => _canvasGroup.alpha, value => _canvasGroup.alpha = value, 0.0f, 0.3f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => Destroy(gameObject));
        }

        private static Tween Fade(Graphic graphic, float alpha, float duration)
        {
            Color targetColor = graphic.color;
            targetColor.a = alpha;
            return DOTween.To(() => graphic.color, value => graphic.color = value, targetColor, duration);
        }

        private static Tween Fade(CanvasGroup canvasGroup, float alpha, float duration)
        {
            return DOTween.To(() => canvasGroup.alpha, value => canvasGroup.alpha = value, alpha, duration);
        }

        private static Tween TweenAnchorPosition(RectTransform transform, Vector2 targetPosition, float duration)
        {
            return DOTween.To(() => transform.anchoredPosition, value => transform.anchoredPosition = value,
                targetPosition, duration);
        }
    }
}

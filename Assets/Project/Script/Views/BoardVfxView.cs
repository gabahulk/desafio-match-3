using DG.Tweening;
using Gazeus.DesafioMatch3.Core.SpecialEffects;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public sealed class BoardVfxView : MonoBehaviour
    {
        private static readonly Color[] TileColors =
        {
            new Color(0.55f, 0.32f, 0.82f),
            new Color(0.35f, 0.72f, 0.38f),
            new Color(1.0f, 0.66f, 0.2f),
            new Color(0.88f, 0.26f, 0.3f)
        };

        private static readonly Vector2[] BurstDirections =
        {
            Vector2.up,
            new Vector2(1.0f, 1.0f).normalized,
            Vector2.right,
            new Vector2(1.0f, -1.0f).normalized,
            Vector2.down,
            new Vector2(-1.0f, -1.0f).normalized,
            Vector2.left,
            new Vector2(-1.0f, 1.0f).normalized
        };

        public Tween Play(SpecialActivationInfo activation, Vector3 sourcePosition, float tileSize,
            GameObject sourceTile)
        {
            Vector2 source = GetOverlayPosition(sourcePosition);
            return activation.Special switch
            {
                SpecialType.HorizontalStriped => PlayStriped(source, tileSize, true, sourceTile),
                SpecialType.VerticalStriped => PlayStriped(source, tileSize, false, sourceTile),
                SpecialType.Wrapped => PlayWrapped(source, tileSize, activation.Phase, sourceTile),
                SpecialType.ColorBomb => PlayColorBomb(source, tileSize, activation.TargetColor, sourceTile),
                _ => DOTween.Sequence()
            };
        }

        private Tween PlayStriped(Vector2 source, float tileSize, bool horizontal, GameObject sourceTile)
        {
            Image line = CreateImage("Striped Beam", new Color(1.0f, 0.82f, 0.38f, 0.95f));
            RectTransform lineTransform = line.rectTransform;
            lineTransform.anchoredPosition = source;
            lineTransform.sizeDelta = horizontal
                ? new Vector2(((RectTransform)transform).rect.width, tileSize * 0.16f)
                : new Vector2(tileSize * 0.16f, ((RectTransform)transform).rect.height);
            lineTransform.localScale = horizontal ? new Vector3(0.01f, 1.0f, 1.0f) : new Vector3(1.0f, 0.01f, 1.0f);

            Sequence sequence = DOTween.Sequence();
            AddTilePunch(sequence, sourceTile, 0.12f);
            sequence.Join(horizontal
                ? lineTransform.DOScaleX(1.0f, 0.12f).SetEase(Ease.OutQuad)
                : lineTransform.DOScaleY(1.0f, 0.12f).SetEase(Ease.OutQuad));
            sequence.Join(Fade(line, 0.25f));
            sequence.OnComplete(() => Destroy(line.gameObject));
            return sequence;
        }

        private Tween PlayWrapped(Vector2 source, float tileSize, SpecialActivationPhase phase,
            GameObject sourceTile)
        {
            float intensity = phase == SpecialActivationPhase.Second ? 1.2f : 1.0f;
            Image flash = CreateImage("Wrapped Flash", new Color(1.0f, 0.84f, 0.47f, 0.85f));
            flash.rectTransform.anchoredPosition = source;
            flash.rectTransform.sizeDelta = Vector2.one * tileSize * 1.45f * intensity;
            flash.rectTransform.localScale = Vector3.zero;

            Sequence sequence = DOTween.Sequence();
            AddTilePunch(sequence, sourceTile, 0.18f);
            sequence.Join(flash.rectTransform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
            sequence.Join(Fade(flash, 0.35f));
            sequence.OnComplete(() => Destroy(flash.gameObject));

            for (int index = 0; index < BurstDirections.Length; index++)
            {
                Image fragment = CreateImage("Wrapped Fragment", index % 2 == 0
                    ? new Color(1.0f, 0.8f, 0.35f, 1.0f)
                    : new Color(1.0f, 0.94f, 0.76f, 1.0f));
                RectTransform fragmentTransform = fragment.rectTransform;
                fragmentTransform.anchoredPosition = source;
                fragmentTransform.sizeDelta = Vector2.one * tileSize * 0.22f;
                fragmentTransform.localScale = Vector3.one * 0.35f;

                Vector2 destination = source + BurstDirections[index] * tileSize * 1.25f * intensity;
                sequence.Join(DOTween.To(
                        () => fragmentTransform.anchoredPosition,
                        position => fragmentTransform.anchoredPosition = position,
                        destination,
                        0.35f)
                    .SetEase(Ease.OutQuad));
                sequence.Join(fragmentTransform.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutBack));
                sequence.Join(fragmentTransform.DORotate(new Vector3(0.0f, 0.0f, 120.0f), 0.35f,
                    RotateMode.FastBeyond360));
                sequence.Join(Fade(fragment, 0.35f));
                sequence.OnComplete(() => Destroy(fragment.gameObject));
            }

            return sequence;
        }

        private Tween PlayColorBomb(Vector2 source, float tileSize, int? targetColor, GameObject sourceTile)
        {
            Image flash = CreateImage("Color Bomb Flash", new Color(1.0f, 0.96f, 0.72f, 0.9f));
            flash.rectTransform.anchoredPosition = source;
            flash.rectTransform.sizeDelta = Vector2.one * tileSize * 2.0f;
            flash.rectTransform.localScale = Vector3.zero;

            Image boardFlash = CreateImage("Color Bomb Board Flash", new Color(1.0f, 1.0f, 1.0f, 0.16f));
            boardFlash.rectTransform.anchorMin = Vector2.zero;
            boardFlash.rectTransform.anchorMax = Vector2.one;
            boardFlash.rectTransform.sizeDelta = Vector2.zero;
            boardFlash.rectTransform.anchoredPosition = Vector2.zero;

            Sequence sequence = DOTween.Sequence();
            AddTilePunch(sequence, sourceTile, 0.25f);
            sequence.Join(flash.rectTransform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
            sequence.Join(Fade(flash, 0.4f));
            sequence.Join(Fade(boardFlash, 0.28f));
            sequence.OnComplete(() =>
            {
                Destroy(flash.gameObject);
                Destroy(boardFlash.gameObject);
            });

            for (int index = 0; index < 10; index++)
            {
                float angle = index * 36.0f;
                Image ray = CreateImage("Color Bomb Ray", GetTileColor(targetColor ?? index));
                RectTransform rayTransform = ray.rectTransform;
                rayTransform.anchoredPosition = source;
                rayTransform.pivot = new Vector2(0.0f, 0.5f);
                rayTransform.sizeDelta = new Vector2(Mathf.Max(((RectTransform)transform).rect.width,
                    ((RectTransform)transform).rect.height) * 0.7f, tileSize * 0.1f);
                rayTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, angle);
                rayTransform.localScale = new Vector3(0.01f, 1.0f, 1.0f);

                sequence.Join(rayTransform.DOScaleX(1.0f, 0.18f).SetEase(Ease.OutQuad));
                sequence.Join(Fade(ray, 0.4f));
                sequence.OnComplete(() => Destroy(ray.gameObject));
            }

            return sequence;
        }

        private Image CreateImage(string name, Color color)
        {
            GameObject imageObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform imageTransform = imageObject.GetComponent<RectTransform>();
            imageTransform.SetParent(transform, false);
            imageTransform.anchorMin = new Vector2(0.5f, 0.5f);
            imageTransform.anchorMax = new Vector2(0.5f, 0.5f);
            imageTransform.pivot = new Vector2(0.5f, 0.5f);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Vector2 GetOverlayPosition(Vector3 worldPosition)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(camera, worldPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screenPosition, camera,
                out Vector2 overlayPosition);
            return overlayPosition;
        }

        private static Tween Fade(Graphic graphic, float duration)
        {
            Color transparent = graphic.color;
            transparent.a = 0.0f;
            return DOTween.To(() => graphic.color, color => graphic.color = color, transparent, duration);
        }

        private static Color GetTileColor(int colorIndex)
        {
            return TileColors[Mathf.Abs(colorIndex) % TileColors.Length];
        }

        private static void AddTilePunch(Sequence sequence, GameObject sourceTile, float strength)
        {
            if (sourceTile == null)
            {
                return;
            }

            sourceTile.transform.DOKill();
            sequence.Join(sourceTile.transform.DOPunchScale(Vector3.one * strength, 0.18f, 4));
        }
    }
}

using UnityEngine;

namespace Gazeus.DesafioMatch3.Views
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class BoardContentInsetController : MonoBehaviour
    {
        [SerializeField] private float _left = 80.0f;
        [SerializeField] private float _right = 80.0f;
        [SerializeField] private float _top = 80.0f;
        [SerializeField] private float _bottom = 80.0f;

        private RectTransform _rectTransform;

        private void OnEnable()
        {
            ApplyInsets();
        }

        private void OnValidate()
        {
            ApplyInsets();
        }

        private void ApplyInsets()
        {
            _rectTransform ??= GetComponent<RectTransform>();
            _rectTransform.offsetMin = new Vector2(_left, _bottom);
            _rectTransform.offsetMax = new Vector2(-_right, -_top);
        }
    }
}

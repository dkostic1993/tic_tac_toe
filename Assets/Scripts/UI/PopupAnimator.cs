using System.Collections;
using UnityEngine;

namespace TicTacToe.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class PopupAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private float duration = 0.18f;
        [SerializeField] private float startScale = 0.92f;

        private CanvasGroup _group;
        private Coroutine _routine;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            if (_routine != null) StopCoroutine(_routine);
            gameObject.SetActive(true);
            _routine = StartCoroutine(Animate(visible: true));
        }

        public void Hide()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Animate(visible: false));
        }

        public void HideImmediate()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            Apply(visible: false, t01: 1f);
            gameObject.SetActive(false);
        }

        private IEnumerator Animate(bool visible)
        {
            Apply(visible, t01: 0f);
            var t = 0f;
            var d = Mathf.Max(0.01f, duration);

            while (t < d)
            {
                t += Time.unscaledDeltaTime;
                var k = Mathf.Clamp01(t / d);
                k = k * k * (3f - 2f * k); // smoothstep
                Apply(visible, k);
                yield return null;
            }

            Apply(visible, 1f);
            if (!visible)
                gameObject.SetActive(false);
            _routine = null;
        }

        private void Apply(bool visible, float t01)
        {
            if (_group == null) return;

            var alpha = visible ? t01 : (1f - t01);
            _group.alpha = alpha;
            _group.blocksRaycasts = visible;
            _group.interactable = visible;

            if (panel != null)
            {
                var scale = Mathf.Lerp(startScale, 1f, alpha);
                panel.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }
}


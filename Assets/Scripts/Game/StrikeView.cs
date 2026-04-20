using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TicTacToe.Game
{
    public sealed class StrikeView : MonoBehaviour
    {
        [SerializeField] private Image strikeImage;
        [SerializeField] private RectTransform strikeRect;
        [SerializeField] private float revealDuration = 0.18f;

        private Coroutine _routine;
        private float _targetLength;

        public void ApplyTheme(ThemeDefinition theme)
        {
            if (strikeImage != null)
            {
                strikeImage.sprite = theme != null ? theme.strikeSprite : null;
                if (theme != null)
                    strikeImage.color = new Color(theme.strikeColor.r, theme.strikeColor.g, theme.strikeColor.b, strikeImage.color.a);
            }
        }

        public void Hide()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            gameObject.SetActive(false);
        }

        public void ShowForWin(WinLine line, RectTransform[] cellRects)
        {
            if (cellRects == null || cellRects.Length < BoardState.CellCount)
            {
                gameObject.SetActive(true);
                return;
            }

            var a = cellRects[line.a];
            var c = cellRects[line.c];
            if (a == null || c == null || strikeRect == null)
            {
                gameObject.SetActive(true);
                return;
            }

            var start = a.TransformPoint(a.rect.center);
            var end = c.TransformPoint(c.rect.center);

            var parent = strikeRect.parent as RectTransform;
            if (parent == null)
            {
                gameObject.SetActive(true);
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, RectTransformUtility.WorldToScreenPoint(null, start), null, out var localStart);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, RectTransformUtility.WorldToScreenPoint(null, end), null, out var localEnd);

            var mid = (localStart + localEnd) * 0.5f;
            var dir = (localEnd - localStart);
            var length = dir.magnitude;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            strikeRect.anchoredPosition = mid;
            _targetLength = length;
            strikeRect.localRotation = Quaternion.Euler(0f, 0f, angle);

            gameObject.SetActive(true);

            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Reveal());
        }

        private IEnumerator Reveal()
        {
            if (strikeImage != null)
                strikeImage.color = new Color(strikeImage.color.r, strikeImage.color.g, strikeImage.color.b, 0f);

            var baseHeight = strikeRect != null ? strikeRect.sizeDelta.y : 20f;
            var d = Mathf.Max(0.05f, revealDuration);
            var t = 0f;
            while (t < d)
            {
                t += Time.deltaTime;
                var k = Mathf.Clamp01(t / d);
                k = k * k * (3f - 2f * k);

                if (strikeRect != null)
                    strikeRect.sizeDelta = new Vector2(_targetLength * k, baseHeight);

                if (strikeImage != null)
                    strikeImage.color = new Color(strikeImage.color.r, strikeImage.color.g, strikeImage.color.b, k);

                yield return null;
            }

            if (strikeRect != null)
                strikeRect.sizeDelta = new Vector2(_targetLength, baseHeight);
            if (strikeImage != null)
                strikeImage.color = new Color(strikeImage.color.r, strikeImage.color.g, strikeImage.color.b, 1f);
            _routine = null;
        }
    }
}


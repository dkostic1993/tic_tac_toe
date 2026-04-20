using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace TicTacToe.Game
{
    public sealed class BoardCellView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image cellBackground;
        [SerializeField] private Image markImage;
        [SerializeField] private TMP_Text markText;

        public int Index { get; private set; }
        public RectTransform RectTransform { get; private set; }

        private System.Action<int> _onClicked;

        public void Init(int index, System.Action<int> onClicked)
        {
            Index = index;
            _onClicked = onClicked;
            RectTransform = GetComponent<RectTransform>();
        }

        public void SetTheme(ThemeDefinition theme)
        {
            if (cellBackground != null)
            {
                cellBackground.sprite = theme != null ? theme.cellSprite : null;
                cellBackground.color = theme != null ? theme.cellColor : new Color(1f, 1f, 1f, 0.08f);
            }
        }

        public void SetMark(ThemeDefinition theme, PlayerMark mark)
        {
            var sprite = (theme == null || mark == PlayerMark.None)
                ? null
                : (mark == PlayerMark.X ? theme.xSprite : theme.oSprite);

            if (markImage != null)
            {
                markImage.sprite = sprite;
                markImage.enabled = sprite != null;
                if (theme != null)
                    markImage.color = mark == PlayerMark.X ? theme.xColor : theme.oColor;
            }

            if (markText != null)
            {
                if (mark == PlayerMark.None)
                {
                    markText.text = string.Empty;
                    markText.enabled = false;
                }
                else
                {
                    // Fallback when sprites aren't assigned yet.
                    markText.text = mark == PlayerMark.X ? "X" : "O";
                    markText.enabled = sprite == null;
                    if (theme != null)
                        markText.color = mark == PlayerMark.X ? theme.xColor : theme.oColor;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClicked?.Invoke(Index);
        }
    }
}


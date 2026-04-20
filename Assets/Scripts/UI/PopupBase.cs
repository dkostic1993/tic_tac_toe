using TicTacToe.Audio;
using UnityEngine;

namespace TicTacToe.UI
{
    public abstract class PopupBase : MonoBehaviour
    {
        [SerializeField] protected GameObject root;
        [SerializeField] private PopupAnimator animator;

        public virtual void Show()
        {
            if (animator != null) animator.Show();
            else if (root != null) root.SetActive(true);
            AudioManager.Instance?.Play(AudioEvent.PopupOpen);
        }

        public virtual void Hide()
        {
            if (animator != null) animator.Hide();
            else if (root != null) root.SetActive(false);
            AudioManager.Instance?.Play(AudioEvent.PopupClose);
        }

        public void HideImmediate()
        {
            if (animator != null) animator.HideImmediate();
            else if (root != null) root.SetActive(false);
        }
    }
}


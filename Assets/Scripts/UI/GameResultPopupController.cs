using TicTacToe.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.Game
{
    public sealed class GameResultPopupController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TicTacToe.UI.PopupAnimator animator;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text durationText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button exitButton;

        [SerializeField] private GameController gameController;

        private void Awake()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetry);
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExit);
        }

        public void HideImmediate()
        {
            if (animator != null) animator.HideImmediate();
            else if (root != null) root.SetActive(false);
        }

        public void Show(GameResult result, float durationSeconds)
        {
            if (animator != null) animator.Show();
            else if (root != null) root.SetActive(true);
            AudioManager.Instance?.Play(AudioEvent.PopupOpen);
            if (titleText != null) titleText.text = ResultToTitle(result);
            if (durationText != null) durationText.text = $"Duration: {FormatDuration(durationSeconds)}";
        }

        private void OnRetry()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            gameController?.NewMatch();
        }

        private void OnExit()
        {
            AudioManager.Instance?.Play(AudioEvent.ButtonClick);
            gameController?.ExitToMenu();
        }

        private static string ResultToTitle(GameResult result)
        {
            switch (result)
            {
                case GameResult.Player1Wins: return "Player 1 Wins";
                case GameResult.Player2Wins: return "Player 2 Wins";
                case GameResult.Draw: return "Draw";
                default: return "Game Over";
            }
        }

        private static string FormatDuration(float seconds)
        {
            var total = Mathf.FloorToInt(Mathf.Max(0f, seconds));
            var min = total / 60;
            var sec = total % 60;
            return $"{min:00}:{sec:00}";
        }
    }
}


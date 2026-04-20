using TicTacToe.Audio;
using TicTacToe.Stats;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TicTacToe.Game
{
    public sealed class GameController : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] private ThemeRegistry themeRegistry;

        [Header("Board")]
        [SerializeField] private BoardCellView[] cells = new BoardCellView[BoardState.CellCount];
        [SerializeField] private StrikeView strikeView;

        [Header("HUD")]
        [SerializeField] private HUDController hud;
        [SerializeField] private GameResultPopupController resultPopup;

        private readonly ThemeService _themeService = new ThemeService();
        private readonly StatsService _statsService = new StatsService();

        private ThemeDefinition _theme;
        private readonly GameSession _session = new GameSession();
        private StatsModel _stats;

        private void Awake()
        {
            _session.StateChanged += Render;
        }

        private void Start()
        {
            _stats = _statsService.Load();
            var themeName = _themeService.LoadThemeName();
            _theme = themeRegistry != null ? themeRegistry.GetByNameOrDefault(themeName) : null;

            for (var i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                    cells[i].Init(i, OnCellClicked);
            }

            NewMatch();
        }

        private void Update()
        {
            _session.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _session.StateChanged -= Render;
        }

        public void NewMatch()
        {
            _session.Reset();
            if (strikeView != null)
            {
                strikeView.ApplyTheme(_theme);
                strikeView.Hide();
            }
            if (resultPopup != null)
                resultPopup.HideImmediate();

            Render();
        }

        public void ExitToMenu()
        {
            SceneManager.LoadScene("Play");
        }

        private void OnCellClicked(int index)
        {
            if (_session.IsOver)
                return;

            if (!_session.TryPlayAt(index))
                return;

            AudioManager.Instance?.Play(AudioEvent.PlaceMark);

            if (_session.IsOver)
            {
                OnMatchCompleted();
            }
        }

        private void OnMatchCompleted()
        {
            _stats.totalGames += 1;
            _stats.totalDurationSeconds += _session.DurationSeconds;
            switch (_session.Result)
            {
                case GameResult.Player1Wins:
                    _stats.player1Wins += 1;
                    break;
                case GameResult.Player2Wins:
                    _stats.player2Wins += 1;
                    break;
                case GameResult.Draw:
                    _stats.draws += 1;
                    break;
            }
            _statsService.Save(_stats);

            if (strikeView != null && _session.WinningLine.HasValue)
            {
                var rects = new RectTransform[cells.Length];
                for (var i = 0; i < cells.Length; i++)
                    rects[i] = cells[i] != null ? cells[i].RectTransform : null;
                strikeView.ShowForWin(_session.WinningLine.Value, rects);
            }

            AudioManager.Instance?.Play(AudioEvent.StrikeWin);

            if (resultPopup != null)
                resultPopup.Show(_session.Result, _session.DurationSeconds);
        }

        private void Render()
        {
            for (var i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                if (cell == null)
                    continue;
                cell.SetTheme(_theme);
                cell.SetMark(_theme, _session.Board.Get(i));
            }

            if (hud != null)
            {
                hud.SetMoveCounts(_session.Player1Moves, _session.Player2Moves);
                hud.SetDurationSeconds(_session.DurationSeconds);
            }
        }
    }
}


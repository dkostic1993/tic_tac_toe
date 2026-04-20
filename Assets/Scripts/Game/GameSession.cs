using System;

namespace TicTacToe.Game
{
    public sealed class GameSession
    {
        public BoardState Board { get; } = new BoardState();

        public PlayerMark CurrentTurn { get; private set; } = PlayerMark.X;
        public int Player1Moves { get; private set; }
        public int Player2Moves { get; private set; }
        public float DurationSeconds { get; private set; }
        public bool IsOver { get; private set; }
        public GameResult Result { get; private set; } = GameResult.InProgress;
        public WinLine? WinningLine { get; private set; }

        public event Action StateChanged;

        public void Reset()
        {
            Board.Reset();
            CurrentTurn = PlayerMark.X;
            Player1Moves = 0;
            Player2Moves = 0;
            DurationSeconds = 0f;
            IsOver = false;
            Result = GameResult.InProgress;
            WinningLine = null;
            StateChanged?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            if (IsOver)
                return;
            DurationSeconds += deltaTime;
            StateChanged?.Invoke();
        }

        public bool TryPlayAt(int index)
        {
            if (IsOver)
                return false;

            if (!Board.TryPlace(index, CurrentTurn))
                return false;

            if (CurrentTurn == PlayerMark.X) Player1Moves++;
            else if (CurrentTurn == PlayerMark.O) Player2Moves++;

            if (Board.TryGetWin(out var winner, out var line))
            {
                IsOver = true;
                WinningLine = line;
                Result = winner == PlayerMark.X ? GameResult.Player1Wins : GameResult.Player2Wins;
            }
            else if (Board.IsFull())
            {
                IsOver = true;
                Result = GameResult.Draw;
            }
            else
            {
                CurrentTurn = CurrentTurn == PlayerMark.X ? PlayerMark.O : PlayerMark.X;
            }

            StateChanged?.Invoke();
            return true;
        }
    }
}


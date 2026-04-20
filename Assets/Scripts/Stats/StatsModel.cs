using System;

namespace TicTacToe.Stats
{
    [Serializable]
    public sealed class StatsModel
    {
        public int totalGames;
        public int player1Wins;
        public int player2Wins;
        public int draws;
        public float totalDurationSeconds;

        public float AverageDurationSeconds => totalGames <= 0 ? 0f : totalDurationSeconds / totalGames;
    }
}


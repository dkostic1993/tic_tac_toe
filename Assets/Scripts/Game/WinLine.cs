namespace TicTacToe.Game
{
    public readonly struct WinLine
    {
        public readonly int a;
        public readonly int b;
        public readonly int c;

        public WinLine(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }
}


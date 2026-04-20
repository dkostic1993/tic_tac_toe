using System;

namespace TicTacToe.Game
{
    public sealed class BoardState
    {
        public const int Size = 3;
        public const int CellCount = Size * Size;

        private readonly PlayerMark[] _cells = new PlayerMark[CellCount];

        public PlayerMark Get(int index) => _cells[index];

        public bool TryPlace(int index, PlayerMark mark)
        {
            if (index < 0 || index >= CellCount)
                return false;
            if (mark == PlayerMark.None)
                return false;
            if (_cells[index] != PlayerMark.None)
                return false;

            _cells[index] = mark;
            return true;
        }

        public void Reset()
        {
            Array.Clear(_cells, 0, _cells.Length);
        }

        public bool IsFull()
        {
            for (var i = 0; i < _cells.Length; i++)
                if (_cells[i] == PlayerMark.None)
                    return false;
            return true;
        }

        public bool TryGetWin(out PlayerMark winner, out WinLine line)
        {
            // Rows
            for (var r = 0; r < Size; r++)
            {
                var i0 = r * Size + 0;
                var i1 = r * Size + 1;
                var i2 = r * Size + 2;
                if (IsSameNonEmpty(i0, i1, i2, out winner))
                {
                    line = new WinLine(i0, i1, i2);
                    return true;
                }
            }

            // Cols
            for (var c = 0; c < Size; c++)
            {
                var i0 = 0 * Size + c;
                var i1 = 1 * Size + c;
                var i2 = 2 * Size + c;
                if (IsSameNonEmpty(i0, i1, i2, out winner))
                {
                    line = new WinLine(i0, i1, i2);
                    return true;
                }
            }

            // Diagonals
            if (IsSameNonEmpty(0, 4, 8, out winner))
            {
                line = new WinLine(0, 4, 8);
                return true;
            }

            if (IsSameNonEmpty(2, 4, 6, out winner))
            {
                line = new WinLine(2, 4, 6);
                return true;
            }

            winner = PlayerMark.None;
            line = default;
            return false;
        }

        private bool IsSameNonEmpty(int a, int b, int c, out PlayerMark mark)
        {
            var m = _cells[a];
            if (m == PlayerMark.None)
            {
                mark = PlayerMark.None;
                return false;
            }

            if (_cells[b] != m || _cells[c] != m)
            {
                mark = PlayerMark.None;
                return false;
            }

            mark = m;
            return true;
        }
    }
}


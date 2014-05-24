using System;

namespace LifeGameWP.Life
{
    public struct Cell : IEquatable<Cell>
    {
        public Cell(float x, float y)
        {
            _x = x;
            _y = y;
        }

        private readonly float _x;

        public float X
        {
            get { return _x; }
        }

        private readonly float _y;

        public float Y
        {
            get { return _y; }
        }

        public bool Equals(Cell other)
        {
            return (X == other.X && Y == other.Y);
        }
    }
}

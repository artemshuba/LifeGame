using System;

namespace LifeGameWP.Life
{
    /// <summary>
    /// Cell
    /// </summary>
    public struct Cell : IEquatable<Cell>
    {
        private readonly float _x;
        private readonly float _y;

        /// <summary>
        /// X
        /// </summary>
        public float X
        {
            get { return _x; }
        }

        /// <summary>
        /// Y
        /// </summary>
        public float Y
        {
            get { return _y; }
        }

        public Cell(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public bool Equals(Cell other)
        {
            return (X == other.X && Y == other.Y);
        }
    }
}

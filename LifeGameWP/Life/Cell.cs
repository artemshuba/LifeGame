using System;

namespace LifeGameWP.Life
{
    /// <summary>
    /// Cell
    /// </summary>
    public struct Cell : IEquatable<Cell>
    {
        private readonly ulong _x;
        private readonly ulong _y;

        /// <summary>
        /// X
        /// </summary>
        public ulong X
        {
            get { return _x; }
        }

        /// <summary>
        /// Y
        /// </summary>
        public ulong Y
        {
            get { return _y; }
        }

        public Cell(ulong x, ulong y)
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

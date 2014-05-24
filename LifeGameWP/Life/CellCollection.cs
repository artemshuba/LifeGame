using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LifeGameWP.Life
{
    public class CellCollection : List<Cell>
    {
        private float _size;
        private static object _syncRoot = new object();

        public CellCollection(IList<Cell> cells, float size)
            : this(size)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Add(cells[i]);
            }
        }

        public CellCollection(float size)
        {
            _size = size;
            Generation = 0;
            _stopwatch = new Stopwatch();
        }

        // Returns the amount of neighboring cells that are alive.
        private byte GetAliveNeighborsCount(float x, float y)
        {
            byte neighbors = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int o = -1; o <= 1; o++)
                {
                    if (i == 0 && o == 0)
                        continue;
                    if (Contains(x + i, y + o))
                        neighbors++;
                }
            }
            return neighbors;
        }

        // Returns all neighboring cells.
        private IEnumerable<Cell> GetNeighbors(float x, float y)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int o = -1; o <= 1; o++)
                {
                    if ((i == 0 && o == 0) || (y + i < 0 || x + o < 0 || y + i > _size || x + o > _size))
                        continue;

                    yield return new Cell(x + o, y + i);
                }
            }
        }

        // Steps forward the specified amount of generations.
        public void Step(uint steps = 1)
        {
            _stopwatch.Restart();

            for (uint step = 0; step < steps; step++)
            {
                Generation++;

                // Variable to act as a snapshot of the current state while we make changes.
                var oldState = new CellCollection(this, _size);

                // Variable to hold the cells that we will check.
                var checkCells = new List<Cell>(oldState);

                // Adds all dead cells neighboring alive cells to the cells that we will check.
                checkCells.AddRange(
                    from cell in oldState
                    from neighbor in GetNeighbors(cell.X, cell.Y)
                    where !checkCells.Contains(neighbor)
                    select neighbor);

                foreach (var cell in checkCells)
                {
                    byte neighbors = oldState.GetAliveNeighborsCount(cell.X, cell.Y);

                    /*
                     * Checks if the current cell is alive or not.
                     * 
                     * If so, if the cell has less than 2, or more than 3 alive neighbors,
                     * the cell will be killed.
                     * 
                     * If not, if the cell has 3 alive neighbors, the cell will be brought to life.
                     */
                    if (oldState.Contains(cell.X, cell.Y))
                    {
                        if (neighbors < 2 || neighbors > 3)
                            Remove(cell);
                    }
                    else
                    {
                        if (neighbors == 3)
                            Add(cell);
                    }
                }
            }

            _stopwatch.Stop();
        }

        public void Add(float x, float y)
        {
            Add(new Cell(x, y));
        }

        public bool Contains(float x, float y)
        {
            return Contains(new Cell(x, y));
        }

        public uint Generation { get; private set; }

        public long MillisecondsToGenerate { get { return _stopwatch.ElapsedMilliseconds; } }

        private readonly Stopwatch _stopwatch;
    }
}

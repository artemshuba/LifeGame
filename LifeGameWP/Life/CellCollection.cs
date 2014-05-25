using System.Collections.Generic;
using System.Linq;

namespace LifeGameWP.Life
{
    /// <summary>
    /// Collection of cells
    /// </summary>
    public class CellCollection : List<Cell>
    {
        private readonly float _universeSize;

        public uint Generation { get; private set; }

        public CellCollection(IList<Cell> cells, float universeSize)
            : this(universeSize)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Add(cells[i]);
            }
        }

        public CellCollection(float universeSize)
        {
            _universeSize = universeSize;
            Generation = 0;
        }

        /// <summary>
        /// Add new cell at specified coords
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Add(float x, float y)
        {
            Add(new Cell(x, y));
        }

        /// <summary>
        /// Checks if collection contains cell at specified coords
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Contains(float x, float y)
        {
            return Contains(new Cell(x, y));
        }

        /// <summary>
        /// Steps forward the specified amount of generations.
        /// </summary>
        /// <param name="steps">Amount of steps</param>
        public void Step(uint steps = 1)
        {
            for (uint step = 0; step < steps; step++)
            {
                Generation++;

                // Variable to act as a snapshot of the current state while we make changes.
                var oldState = new CellCollection(this, _universeSize);

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
        }

        // Returns the amount of neighboring cells that are alive.
        private byte GetAliveNeighborsCount(float x, float y)
        {
            byte neighbors = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    if (Contains(x + i, y + j))
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
                for (int j = -1; j <= 1; j++)
                {
                    if ((i == 0 && j == 0) || (y + i < 0 || x + j < 0 || y + i > _universeSize || x + j > _universeSize))
                        continue;

                    yield return new Cell(x + j, y + i);
                }
            }
        }
    }
}

using System.Collections.Generic;

namespace LifeGameWP.Life
{
    /// <summary>
    /// Collection of cells
    /// </summary>
    public class CellCollection : List<Cell>
    {

        public CellCollection(IList<Cell> cells)
            : this()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Add(cells[i]);
            }
        }

        public CellCollection()
        {

        }

        /// <summary>
        /// Add new cell at specified coords
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Add(ulong x, ulong y)
        {
            Add(new Cell(x, y));
        }

        /// <summary>
        /// Checks if collection contains cell at specified coords
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Contains(ulong x, ulong y)
        {
            return Contains(new Cell(x, y));
        }
    }
}

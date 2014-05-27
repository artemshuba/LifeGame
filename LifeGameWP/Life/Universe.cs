using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace LifeGameWP.Life
{
    public class Universe
    {
        private const float CAMERA_MOVE_TOLERANCE = 0.5f;
        private const int CELL_SIZE = 40;
        private const double MILLISECONDS_PER_FRAME = 50.0f;//update every 50ms
        private const float CAMERA_ACCELERATION = 2.0f;

        private readonly SpriteBatch _spriteBatch;
        private readonly ulong _size; //universe size
        private CellCollection _cellCollection;
        private readonly Camera _camera;
        private int _aliveCellsCount;
        private bool _wasTouchPressed = false;
        private double _timeSinceLastUpdate = 0; //accumulate the elapsed time
        private bool _canStep = true; //can perform update step

        /// <summary>
        /// Is paused
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Camera
        /// </summary>
        public Camera Camera
        {
            get { return _camera; }
        }

        /// <summary>
        /// Amount of alive cells
        /// </summary>
        public int AliveCellsCount
        {
            get { return _aliveCellsCount; }
        }

        /// <summary>
        /// Universe size
        /// </summary>
        public float Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Generation
        /// </summary>
        public ulong Generation { get; set; }

        /// <summary>
        /// Collection of cells
        /// </summary>
        public CellCollection CellCollection
        {
            get { return _cellCollection; }
            set { _cellCollection = value; }
        }

        public Universe(ulong size, uint viewPortWidth, uint viewPortHeight, SpriteBatch spriteBatch)
        {
            _size = size;
            _spriteBatch = spriteBatch;
            _cellCollection = new CellCollection();
            _camera = new Camera()
            {
                Width = viewPortWidth,
                Height = viewPortHeight,
                X = 0,
                Y = 0
            };

            IsPaused = true;
        }

        /// <summary>
        /// Update universe
        /// </summary>
        /// <param name="gameTime"></param>
        public async void Update(GameTime gameTime)
        {
            HandleInput();

            if (!IsPaused && _canStep)
            {
                _timeSinceLastUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_timeSinceLastUpdate >= MILLISECONDS_PER_FRAME)
                {
                    _timeSinceLastUpdate = 0;

                    _canStep = false;

                    await StepAsync();

                    _canStep = true;
                }
            }
        }

        /// <summary>
        /// Draw universe
        /// </summary>
        /// <param name="gameTime"></param>
        public void Draw(GameTime gameTime)
        {
            DrawGrid();
            DrawField();
        }

        /// <summary>
        /// Perform update of cells collection
        /// </summary>
        /// <returns></returns>
        public Task<bool> StepAsync()
        {
            //run in separate task to avoid blocking app ui thread
            return Task.Run(() => Step());
        }

        #region Update universe

        /// <summary>
        /// Steps forward the specified amount of generations.
        /// </summary>
        /// <param name="steps">Amount of steps</param>
        private bool Step(uint steps = 1)
        {
            bool changed = false;

            for (uint step = 0; step < steps; step++)
            {
                Generation++;

                // Variable to act as a snapshot of the current state while we make changes.
                var oldState = new CellCollection(_cellCollection);

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
                    byte neighbors = GetAliveNeighborsCount(oldState, cell.X, cell.Y);

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
                        {
                            _cellCollection.Remove(cell);
                            changed = true;
                        }
                    }
                    else
                    {
                        if (neighbors == 3)
                        {
                            _cellCollection.Add(cell);
                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }


        // Returns the amount of neighboring cells that are alive.
        private byte GetAliveNeighborsCount(CellCollection cells, ulong x, ulong y)
        {
            byte neighbors = 0;

            uint xFrom = 1;
            if (x == 0)
                xFrom = 0;

            uint xTo = 1;
            if (x == _size)
                xTo = 0;

            uint yFrom = 1;
            if (y == 0)
                yFrom = 0;

            uint yTo = 1;
            if (y == _size)
                yTo = 0;

            for (ulong i = x - xFrom; i <= x + xTo; i++)
            {
                for (ulong j = y - yFrom; j <= y + yTo; j++)
                {
                    if (i == x && j == y)
                        continue;

                    if (cells.Contains(i, j))
                        neighbors++;
                }
            }
            return neighbors;
        }

        // Returns all neighboring cells.
        private IEnumerable<Cell> GetNeighbors(ulong x, ulong y)
        {
            uint xFrom = 1;
            if (x == 0)
                xFrom = 0;

            uint xTo = 1;
            if (x == _size - 1)
                xTo = 0;

            uint yFrom = 1;
            if (y == 0)
                yFrom = 0;

            uint yTo = 1;
            if (y == _size - 1)
                yTo = 0;

            for (ulong i = y - yFrom; i <= y + yTo; i++)
            {
                for (ulong j = x - xFrom; j <= x + xTo; j++)
                {
                    //skip current cell
                    if (i == y && x == 0)
                        continue;

                    yield return new Cell(j, i);
                }
            }
        }

        private void HandleInput()
        {
            var touchStateCollection = TouchPanel.GetState();
            foreach (var touchState in touchStateCollection)
            {
                if (touchState.State == TouchLocationState.Moved)
                {
                    TouchLocation prevLocation;
                    touchState.TryGetPreviousLocation(out prevLocation);

                    var delta = touchState.Position - prevLocation.Position;
                    if (delta.LengthSquared() > CAMERA_MOVE_TOLERANCE)
                    {
                        float x = _camera.X - delta.X * 2.0f;
                        float y = _camera.Y - delta.Y * 2.0f;

                        if (x < 0)
                            x = 0;
                        else if (x > _size * CELL_SIZE - _camera.Width)
                            x = _size * CELL_SIZE - _camera.Width;

                        if (y < 0)
                            y = 0;
                        else if (y > _size * CELL_SIZE - _camera.Height)
                            y = _size * CELL_SIZE - _camera.Height;

                        _camera.X = x;
                        _camera.Y = y;

                        _wasTouchPressed = false;
                    }
                }
                else if (touchState.State == TouchLocationState.Pressed)
                {
                    _wasTouchPressed = true;
                }
                else if (touchState.State == TouchLocationState.Released)
                {
                    TouchLocation prevLocation;
                    touchState.TryGetPreviousLocation(out prevLocation);
                    if (_wasTouchPressed)
                    {
                        var point = touchState.Position;
                        var cellPosition = GetCellFromPoint(point.X + _camera.X, point.Y + _camera.Y);
                        var cell = new Cell((ulong)cellPosition.X, (ulong)cellPosition.Y);

                        if (_cellCollection.Contains(cell))
                            _cellCollection.Remove(cell);
                        else
                            _cellCollection.Add(cell);
                    }
                }
            }
        }

        //Return cell coordinates by screen point
        private Vector2 GetCellFromPoint(float x, float y)
        {
            var cellX = (float)Math.Floor(x / CELL_SIZE);
            var cellY = (float)Math.Floor(y / CELL_SIZE);

            return new Vector2(cellX, cellY);
        }

        #endregion

        #region Draw

        //Draw help grid
        private void DrawGrid()
        {
            var xOffset = Math.Abs(_camera.X - Math.Floor(_camera.X / CELL_SIZE) * CELL_SIZE);

            var x = 0 - (float)xOffset;
            for (int i = 0; i < _camera.Width / CELL_SIZE + 1; i++)
            {
                //draw vertical lines
                _spriteBatch.DrawLine(x, 0, x, _camera.Height, Color.Green);
                x += CELL_SIZE;
            }

            var yOffset = Math.Abs(_camera.Y - Math.Floor(_camera.Y / CELL_SIZE) * CELL_SIZE);

            var y = 0 - (float)yOffset;
            for (int i = 0; i < _camera.Height / CELL_SIZE + 1; i++)
            {
                //draw horizontal lines
                _spriteBatch.DrawLine(0, y, _camera.Width, y, Color.Green);
                y += CELL_SIZE;
            }
        }

        //Draw cell by coords
        private void DrawCell(float x, float y)
        {
            var drawX = (int)((x * CELL_SIZE) - _camera.X);
            var drawY = (int)((y * CELL_SIZE) - _camera.Y);

            if (drawX < 0 - CELL_SIZE || drawY < 0 - CELL_SIZE || drawX > _camera.Width + CELL_SIZE ||
                drawY > _camera.Height + CELL_SIZE) //draw only if cell is visible on screen
                return;

            _spriteBatch.FillRectangle(new Rectangle(drawX, drawY, CELL_SIZE, CELL_SIZE), Color.Green);

        }

        //Draw all alive cells
        private void DrawField()
        {
            _aliveCellsCount = 0;
            for (int i = 0; i < _cellCollection.Count; i++)
            {
                var cell = _cellCollection[i];
                DrawCell(cell.X, cell.Y);
                _aliveCellsCount++;
            }
        }

        #endregion
    }
}

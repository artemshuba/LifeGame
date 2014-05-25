using System;
using System.Threading;
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

        private readonly SpriteBatch _spriteBatch;
        private readonly float _size; //universe size
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
        /// Collection of cells
        /// </summary>
        public CellCollection CellCollection
        {
            get { return _cellCollection; }
            set { _cellCollection = value; }
        }

        public Universe(float size, float viewPortWidth, float viewPortHeight, SpriteBatch spriteBatch)
        {
            _size = size;
            _spriteBatch = spriteBatch;
            _cellCollection = new CellCollection(size);
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
        public Task StepAsync()
        {
            //run in separate task to avoid blocking app ui thread
            return Task.Run(() => _cellCollection.Step());
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
                        _camera.X -= delta.X * 2.0f;
                        _camera.Y -= delta.Y * 2.0f;

                        if (_camera.X + _camera.Width > _size)
                            _camera.X = _size - _camera.Width;

                        if (_camera.Y + _camera.Height > _size)
                            _camera.Y = _size - _camera.Height;

                        if (_camera.X < 0)
                            _camera.X = 0;

                        if (_camera.Y < 0)
                            _camera.Y = 0;

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
                        var cellPosition = GetCellFromPoint((int)(point.X + _camera.X), (int)(point.Y + _camera.Y));
                        var cell = new Cell(cellPosition.X, cellPosition.Y);

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

        #region Draw

        //Draw help grid
        private void DrawGrid()
        {
            var x = 0 - _camera.X;
            for (int i = 0; i < _camera.Width; i++)
            {
                //draw vertical lines
                _spriteBatch.DrawLine(x, 0, x, _size * CELL_SIZE, Color.Green);
                x += CELL_SIZE;
            }

            var y = 0 - _camera.Y;
            for (int i = 0; i < _camera.Height; i++)
            {
                //draw horizontal lines
                _spriteBatch.DrawLine(0, y, _size * CELL_SIZE, y, Color.Green);
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

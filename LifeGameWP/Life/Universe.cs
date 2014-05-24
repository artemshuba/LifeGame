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

        private float _size;
        private int _cellSize = 50;
        private SpriteBatch _spriteBatch;
        private double _millisecondsPerFrame = 50.0f; //Update every 1 second
        private double _timeSinceLastUpdate = 0; //Accumulate the elapsed time
        private CellCollection _cellCollection;
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(true);
        private Camera _camera;
        private int _cellsCount;
        private bool _pressed = false;

        public bool IsPaused { get; set; }

        public Camera Camera
        {
            get { return _camera; }
        }

        public int CellsCount
        {
            get { return _cellsCount; }
        }

        public float Size
        {
            get { return _size; }
        }

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

        public async void Update(GameTime gameTime)
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
                        _camera.X -= delta.X * 3.0f;
                        _camera.Y -= delta.Y * 3.0f;

                        if (_camera.X + _camera.Width > _size)
                            _camera.X = _size - _camera.Width;

                        if (_camera.Y + _camera.Height > _size)
                            _camera.Y = _size - _camera.Height;

                        if (_camera.X < 0)
                            _camera.X = 0;

                        if (_camera.Y < 0)
                            _camera.Y = 0;

                        _pressed = false;
                    }
                }
                else if (touchState.State == TouchLocationState.Pressed)
                {
                    _pressed = true;
                }
                else if (touchState.State == TouchLocationState.Released)
                {
                    TouchLocation prevLocation;
                    touchState.TryGetPreviousLocation(out prevLocation);
                    if (_pressed)
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

            if (!IsPaused)
            {
                _timeSinceLastUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_timeSinceLastUpdate >= _millisecondsPerFrame)
                {
                    _timeSinceLastUpdate = 0;

                    await Task.Run(() => _autoResetEvent.WaitOne());

                    await StepAsync();
                    _autoResetEvent.Set();
                    //Step();
                }
            }
        }

        private Task StepAsync()
        {
            return Task.Run(() => _cellCollection.Step());
        }

        #region Draw

        public void Draw(GameTime gameTime)
        {

            DrawGrid();
            DrawField();
        }

        private void DrawGrid()
        {
            var x = 0 - _camera.X;
            for (int i = 0; i < _camera.Width; i++)
            {
                //draw vertical lines
                _spriteBatch.DrawLine(x, 0, x, _size * _cellSize, Color.Green);
                x += _cellSize;
            }

            var y = 0 - _camera.Y;
            for (int i = 0; i < _camera.Height; i++)
            {
                //draw horizontal lines
                _spriteBatch.DrawLine(0, y, _size * _cellSize, y, Color.Green);
                y += _cellSize;
            }
        }

        private void DrawPoint(float x, float y)
        {
            var drawX = (int)((x * _cellSize) - _camera.X);
            var drawY = (int)((y * _cellSize) - _camera.Y);

            if (drawX < 0 - _cellSize || drawY < 0 - _cellSize || drawX > _camera.Width + _cellSize ||
                drawY > _camera.Height + _cellSize) //draw only if point is visible on screen
                return;

            _spriteBatch.FillRectangle(new Rectangle(drawX, drawY, _cellSize, _cellSize), Color.Green);

        }

        private void DrawField()
        {
            _cellsCount = 0;
            for (int i = 0; i < _cellCollection.Count; i++)
            {
                var cell = _cellCollection[i];
                DrawPoint(cell.X, cell.Y);
                _cellsCount++;
            }
        }

        #endregion

        private Vector2 GetCellFromPoint(float x, float y)
        {
            var cellX = (float)Math.Floor(x / _cellSize);
            var cellY = (float)Math.Floor(y / _cellSize);

            return new Vector2(cellX, cellY);
        }
    }
}

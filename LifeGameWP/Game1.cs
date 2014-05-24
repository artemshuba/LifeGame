using System;
using System.IO;
using System.IO.IsolatedStorage;
using LifeGameWP.Life;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LifeGameWP
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private const float UNIVERSE_FIZE = 5000.0f;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Universe _universe;
        private bool _isPaused = true;
        private SpriteFont _debugFont;

        public event EventHandler Initialized;

        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
                _universe.IsPaused = value;
            }
        }

        public Universe Universe
        {
            get { return _universe; }
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            IsFixedTimeStep = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _universe = new Universe(UNIVERSE_FIZE, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, _spriteBatch);

            if (Initialized != null)
                Initialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _debugFont = Content.Load<SpriteFont>("DebugFont");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _universe.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _universe.Draw(gameTime);

            _spriteBatch.DrawString(_debugFont, "Cells count:" + _universe.CellsCount, new Vector2(10, 10), Color.Yellow);

            _spriteBatch.DrawString(_debugFont, string.Format("Camera: X {0} Y {1}", _universe.Camera.X, _universe.Camera.Y), new Vector2(10, 30), Color.Yellow);

            _spriteBatch.DrawString(_debugFont, string.Format("Universe size: {0}x{0}", _universe.Size), new Vector2(10, 50), Color.Yellow);

            _spriteBatch.End();


            base.Draw(gameTime);
        }

        public void SaveData(string fileName)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = store.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new BinaryWriter(fileStream))
                    {
                        writer.Write(_universe.Camera.X);
                        writer.Write(_universe.Camera.Y);

                        for (var i = 0; i < _universe.CellCollection.Count; i++)
                        {
                            var cell = _universe.CellCollection[i];
                            //write pair of x,y
                            writer.Write(cell.X);
                            writer.Write(cell.Y);
                        }

                        writer.Flush();
                        fileStream.Flush();
                    }
                }
            }
        }

        public void LoadData(string fileName)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                    return;

                using (var fileStream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new BinaryReader(fileStream))
                    {
                        var cellCollection = new CellCollection(UNIVERSE_FIZE);

                        var cameraX = reader.ReadSingle();
                        var cameraY = reader.ReadSingle();

                        _universe.Camera.X = cameraX;
                        _universe.Camera.Y = cameraY;

                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            //read pair of x,y
                            var x = reader.ReadSingle();
                            var y = reader.ReadSingle();

                            var cell = new Cell(x, y);
                            cellCollection.Add(cell);
                        }

                        _universe.CellCollection = cellCollection;
                    }
                }
            }
        }
    }
}

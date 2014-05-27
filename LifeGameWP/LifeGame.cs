using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Controls;
using LifeGameWP.Life;
using LifeGameWP.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LifeGameWP
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class LifeGame : Game
    {
        private const float UNIVERSE_FIZE = 5000.0f;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Universe _universe;
        private bool _isPaused = true;
        private SpriteFont _debugFont;
        private AudioService _audioService;

        public event EventHandler Initialized;

        /// <summary>
        /// Is paused
        /// </summary>
        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
                _universe.IsPaused = value;
            }
        }

        /// <summary>
        /// Universe
        /// </summary>
        public Universe Universe
        {
            get { return _universe; }
        }

        /// <summary>
        /// Audio service
        /// </summary>
        public AudioService AudioService { get; set; }

        public LifeGame()
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

            //Draw some debug info
            _spriteBatch.DrawString(_debugFont, "Alive cells count:" + _universe.AliveCellsCount, new Vector2(10, 10), Color.Yellow);

            _spriteBatch.DrawString(_debugFont, string.Format("Generation: {0}", _universe.Generation), new Vector2(10, 30), Color.Yellow);

            _spriteBatch.DrawString(_debugFont, string.Format("Camera: X {0} Y {1}", _universe.Camera.X, _universe.Camera.Y), new Vector2(10, 50), Color.Yellow);

            _spriteBatch.DrawString(_debugFont, string.Format("Universe size: {0}x{0}", _universe.Size), new Vector2(10, 70), Color.Yellow);
            //

            _spriteBatch.End();


            base.Draw(gameTime);
        }

        /// <summary>
        /// Save universe state to file
        /// </summary>
        /// <param name="fileName">File name</param>
        public void SaveData(string fileName)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = store.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new BinaryWriter(fileStream))
                    {
                        //write camera coords
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

        /// <summary>
        /// Load universe state from file
        /// </summary>
        /// <param name="fileName">File name</param>
        public void TryLoadData(string fileName)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName))
                    return;

                using (var fileStream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new BinaryReader(fileStream))
                    {
                        var cellCollection = new CellCollection();

                        //read camera coords
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

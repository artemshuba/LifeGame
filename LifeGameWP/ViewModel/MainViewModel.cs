using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LifeGameWP.Controls.BindableApplicationBar;
using LifeGameWP.Resources.Localization;
using Microsoft.Phone.Shell;

namespace LifeGameWP.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<IApplicationBarMenuItem> _appbarButtons;
        private bool _canSaveLoad = true;
        private bool _canStep = true;

        #region Commands

        /// <summary>
        /// Play/Pause command
        /// </summary>
        public RelayCommand PlayPauseCommand { get; private set; }

        /// <summary>
        /// Save state command
        /// </summary>
        public RelayCommand SaveCommand { get; private set; }

        /// <summary>
        /// Load state command
        /// </summary>
        public RelayCommand LoadCommand { get; private set; }

        /// <summary>
        /// Reset command
        /// </summary>
        public RelayCommand ResetCommand { get; private set; }

        /// <summary>
        /// Perform single step command
        /// </summary>
        public RelayCommand StepCommand { get; private set; }

        #endregion

        /// <summary>
        /// Application bar buttons
        /// </summary>
        public ObservableCollection<IApplicationBarMenuItem> AppbarButtons
        {
            get { return _appbarButtons; }
            set { Set(ref _appbarButtons, value); }
        }

        /// <summary>
        /// Game
        /// </summary>
        public LifeGame Game { get; private set; }

        /// <summary>
        /// Is save/load button enabled
        /// </summary>
        public bool CanSaveLoad
        {
            get { return _canSaveLoad; }
            set { Set(ref _canSaveLoad, value); }
        }

        /// <summary>
        /// Is step button enabled
        /// </summary>
        public bool CanStep
        {
            get { return _canStep; }
            set { Set(ref _canStep, value); }
        }

        public MainViewModel(LifeGame game)
        {
            Game = game;

            Game.Initialized += Game_Initialized;

            InitializeCommands();
            InitializeAppbarButtons();
        }

        private void Game_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void InitializeCommands()
        {
            PlayPauseCommand = new RelayCommand(() =>
            {
                Game.IsPaused = !Game.IsPaused;
                CanSaveLoad = Game.IsPaused;
                CanStep = Game.IsPaused;

                InitializeAppbarButtons();
            });

            SaveCommand = new RelayCommand(() =>
            {
                Game.SaveData("universe.dat");
            });

            LoadCommand = new RelayCommand(() =>
            {
                Game.TryLoadData("universe.dat");
            });

            ResetCommand = new RelayCommand(() =>
            {
                Game.IsPaused = true;
                Game.Universe.CellCollection.Clear();
            });

            StepCommand = new RelayCommand(() =>
            {
                Game.Universe.StepAsync();
            });
        }

        private void InitializeAppbarButtons()
        {
            var appbarButtons = new ObservableCollection<IApplicationBarMenuItem>();

            var playPauseButton = new BindableApplicationBarIconButton();
            if (Game.IsPaused)
            {
                playPauseButton.IconUri = new Uri("/Resources/Images/Appbar/appbar.control.play.png", UriKind.Relative);
                playPauseButton.Text = AppResources.AppbarRun;
            }
            else
            {
                playPauseButton.IconUri = new Uri("/Resources/Images/Appbar/appbar.control.pause.png", UriKind.Relative);
                playPauseButton.Text = AppResources.AppbarPause;
            }
            playPauseButton.Command = PlayPauseCommand;

            appbarButtons.Add(playPauseButton);


            var stepButton = new BindableApplicationBarIconButton();
            stepButton.IconUri = new Uri("/Resources/Images/Appbar/appbar.control.resume.png", UriKind.Relative);
            stepButton.Text = AppResources.AppbarStep;
            stepButton.Command = StepCommand;
            stepButton.SetBinding(BindableApplicationBarIconButton.IsEnabledProperty, new Binding("CanStep"));

            appbarButtons.Add(stepButton);


            var saveButton = new BindableApplicationBarIconButton();
            saveButton.IconUri = new Uri("/Resources/Images/Appbar/appbar.save.png", UriKind.Relative);
            saveButton.Text = AppResources.AppbarSave;
            saveButton.Command = SaveCommand;
            saveButton.SetBinding(BindableApplicationBarIconButton.IsEnabledProperty, new Binding("CanSaveLoad"));

            appbarButtons.Add(saveButton);


            var loadButton = new BindableApplicationBarIconButton();
            loadButton.IconUri = new Uri("/Resources/Images/Appbar/appbar.folder.open.png", UriKind.Relative);
            loadButton.Text = AppResources.AppbarLoad;
            loadButton.Command = LoadCommand;
            loadButton.SetBinding(BindableApplicationBarIconButton.IsEnabledProperty, new Binding("CanSaveLoad"));

            appbarButtons.Add(loadButton);


            var resetButton = new BindableApplicationBarMenuItem();
            resetButton.Text = AppResources.AppbarReset;
            resetButton.Command = ResetCommand;

            appbarButtons.Add(resetButton);

            AppbarButtons = appbarButtons;
        }

        private void Initialize()
        {
            //load saved (or default) universe state
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists("universe.dat"))
                {
                    var strm = Application.GetResourceStream(new Uri("/LifeGameWP;component/Content/universe.dat",
                            UriKind.Relative));

                    var fileStream = store.OpenFile("universe.dat", FileMode.Create, FileAccess.Write);
                    strm.Stream.CopyTo(fileStream);

                    strm.Stream.Dispose();

                    fileStream.Flush();
                    fileStream.Dispose();
                }
            }

            Game.TryLoadData("universe.dat");
        }
    }
}

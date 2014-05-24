using System.ComponentModel;
using System.Windows;
using LifeGameWP.ViewModel;
using Microsoft.Phone.Controls;
using MonoGame.Framework.WindowsPhone;

namespace LifeGameWP
{
    public partial class GamePage : PhoneApplicationPage
    {
        private MainViewModel _mainViewModel;

        // Constructor
        public GamePage()
        {
            InitializeComponent();

            var game = XamlGame<Game1>.Create("", this);

            _mainViewModel = new MainViewModel(game);
            this.DataContext = _mainViewModel;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            _mainViewModel.Game.Exit();

            base.OnBackKeyPress(e);
        }
    }
}
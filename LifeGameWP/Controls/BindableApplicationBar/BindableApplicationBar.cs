using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using LifeGameWP.Extensions;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LifeGameWP.Controls.BindableApplicationBar
{
    [ContentProperty("Buttons")]
    public class BindableApplicationBar : ItemsControl, IApplicationBar
    {
        private readonly ApplicationBar _applicationBar;
        private double _opacityBackup;

        public BindableApplicationBar()
        {
            _applicationBar = new ApplicationBar();
            this.Loaded += BindableApplicationBarLoaded;
            this.StateChanged += BindableApplicationBar_StateChanged;
        }

        private void BindableApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            if (e.IsMenuVisible && RemoveOpacityOnOpen)
            {
                _opacityBackup = _applicationBar.Opacity;
                _applicationBar.Opacity = 0.99;
            }
            else
            {
                _applicationBar.Opacity = _opacityBackup;
            }
        }

        private void BindableApplicationBarLoaded(object sender, RoutedEventArgs e)
        {
            var page =
                this.GetVisualAncestors().FirstOrDefault(c => c is PhoneApplicationPage) as PhoneApplicationPage;
            if (page != null) page.ApplicationBar = _applicationBar;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            _applicationBar.Buttons.Clear();
            _applicationBar.MenuItems.Clear();

            foreach (BindableApplicationBarIconButton button in Items.Where(c => c is BindableApplicationBarIconButton
                //fix VS designer errors
                && (c as BindableApplicationBarIconButton).IconUri != null && !(c as BindableApplicationBarIconButton).IconUri.IsAbsoluteUri))
            {
                _applicationBar.Buttons.Add(button.Button);
            }
            foreach (BindableApplicationBarMenuItem button in Items.Where(c => c is BindableApplicationBarMenuItem))
            {
                _applicationBar.MenuItems.Add(button.MenuItem);
            }
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached("IsVisible", typeof(bool), typeof(BindableApplicationBar),
                                                new PropertyMetadata(true, OnVisibleChanged));

        private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBar)d)._applicationBar.IsVisible = (bool)e.NewValue;
            }
        }

        public static readonly DependencyProperty IsMenuEnabledProperty =
            DependencyProperty.RegisterAttached("IsMenuEnabled", typeof(bool), typeof(BindableApplicationBar),
                                                new PropertyMetadata(true, OnEnabledChanged));

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBar)d)._applicationBar.IsMenuEnabled = (bool)e.NewValue;
            }
        }

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly DependencyProperty BarOpacityProperty =
            DependencyProperty.RegisterAttached("BarOpacity", typeof(double), typeof(BindableApplicationBar),
                                                new PropertyMetadata(1.0, OnBarOpacityChanged));

        private static void OnBarOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBar)d)._applicationBar.Opacity = (double)e.NewValue;
            }
        }

        public double BarOpacity
        {
            get { return (double)GetValue(BarOpacityProperty); }
            set { SetValue(BarOpacityProperty, value); }
        }

        public bool IsMenuEnabled
        {
            get { return (bool)GetValue(IsMenuEnabledProperty); }
            set { SetValue(IsMenuEnabledProperty, value); }
        }

        public Color BackgroundColor
        {
            get { return _applicationBar.BackgroundColor; }
            set { _applicationBar.BackgroundColor = value; }
        }

        public Color ForegroundColor
        {
            get { return _applicationBar.ForegroundColor; }
            set { _applicationBar.ForegroundColor = value; }
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(ApplicationBarMode), typeof(BindableApplicationBar), new PropertyMetadata(ApplicationBarMode.Default, ModeChanged));

        private static void ModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBar)d)._applicationBar.Mode = (ApplicationBarMode)e.NewValue;
            }
        }


        public ApplicationBarMode Mode
        {
            get { return (ApplicationBarMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }


        public double DefaultSize
        {
            get { return _applicationBar.DefaultSize; }
        }

        public double MiniSize
        {
            get { return _applicationBar.MiniSize; }
        }

        public IList Buttons
        {
            get { return this.Items; }

        }

        public IList MenuItems
        {
            get { return this.Items; }
        }

        public event EventHandler<ApplicationBarStateChangedEventArgs> StateChanged
        {
            add { _applicationBar.StateChanged += value; }
            remove { _applicationBar.StateChanged -= value; }
        }

        public bool RemoveOpacityOnOpen { get; set; }

        #region BarBackground

        /// <summary>
        /// BarBackground Dependency Property
        /// </summary>
        public static readonly DependencyProperty BarBackgroundColorProperty =
            DependencyProperty.Register("BarBackgroundColor", typeof(Color), typeof(BindableApplicationBar),
                new PropertyMetadata(Colors.Transparent,
                    new PropertyChangedCallback(OnBarBackgroundChanged)));

        /// <summary>
        /// Gets or sets the BarBackground property. This dependency property 
        /// indicates ....
        /// </summary>
        public Color BarBackgroundColor
        {
            get { return (Color)GetValue(BarBackgroundColorProperty); }
            set { SetValue(BarBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the BarBackground property.
        /// </summary>
        private static void OnBarBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BindableApplicationBar target = (BindableApplicationBar)d;
            Color oldBarBackground = (Color)e.OldValue;
            Color newBarBackground = target.BarBackgroundColor;
            target.OnBarBackgroundChanged(oldBarBackground, newBarBackground);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the BarBackground property.
        /// </summary>
        protected virtual void OnBarBackgroundChanged(Color oldBarBackground, Color newBarBackground)
        {
            this._applicationBar.BackgroundColor = newBarBackground;
        }

        #endregion


    }
}

using System;
using System.Windows.Controls;

namespace LifeGameWP.Services
{
    /// <summary>
    /// Service for playing music
    /// </summary>
    public class AudioService
    {
        private readonly MediaElement _mediaElement;

        public AudioService(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            _mediaElement.MediaEnded += _mediaElement_MediaEnded;
        }

        /// <summary>
        /// Play audio by uri
        /// </summary>
        /// <param name="uri"></param>
        public void Play(Uri uri)
        {
            _mediaElement.Source = uri;
        }

        /// <summary>
        /// Play
        /// </summary>
        public void Play()
        {
            _mediaElement.Play();
        }

        /// <summary>
        /// Pause
        /// </summary>
        public void Pause()
        {
            _mediaElement.Pause();
        }

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            _mediaElement.Stop();
        }

        private void _mediaElement_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            _mediaElement.Play();
        }
    }
}

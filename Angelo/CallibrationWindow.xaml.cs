using Angelo.Bot;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Angelo
{
    /// <summary>
    /// Interaction logic for CallibrationWindow.xaml
    /// </summary>
    public partial class CallibrationWindow : Window
    {
        private static CallibrationWindow? _instance;
        private static readonly object _lock = new();

        private CallibrationWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Get singular instance, creating it if needed. This will always run on the UI thread.
        /// </summary>
        /// <returns></returns>
        public static CallibrationWindow GetInstance()
        {
            if (Application.Current.Dispatcher.Thread != Thread.CurrentThread)
            {
                return Application.Current.Dispatcher.Invoke(() => GetInstance());
            }

            if (_instance == null || !_instance.IsLoaded)
            {
                lock (_lock)
                {
                    if (_instance == null || !_instance.IsLoaded)
                    {
                        _instance = new CallibrationWindow();
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        /// Check if window is active and bobber checkbox is set.
        /// Always runs on UI thread.
        /// </summary>
        /// <returns></returns>
        public bool ShouldShowBobber()
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                return Dispatcher.Invoke(() => ShouldShowBobber());
            }
            return IsVisible && ShowBobber.IsChecked == true;
        }

        /// <summary>
        /// Check if window is active and splash checkbox is set.
        /// Always runs on UI thread.
        /// </summary>
        /// <returns></returns>
        public bool ShouldShowSplash()
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                return Dispatcher.Invoke(() => ShouldShowSplash());
            }
            return IsVisible && ShowSplash.IsChecked == true;
        }

        private void SetInfoText(string text)
        {
            InfoText.Text = text.Trim();
        }

        private void SetImage(Bitmap? bmp)
        {
            if (bmp == null)
            {
                ImageDisplay.Source = null;
                return;
            }

            MemoryStream mstream = new MemoryStream();
            bmp.Save(mstream, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage bmpimg = new();
            bmpimg.BeginInit();
            mstream.Seek(0, SeekOrigin.Begin);
            bmpimg.StreamSource = mstream;
            bmpimg.EndInit();

            ImageDisplay.Source = bmpimg;
        }

        /// <summary>
        /// Display or update splash scan.
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="areas"></param>
        internal void SetBobberResult(Bitmap bmp, List<FloodCountResult> areas, Rectangle displayRegion)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(() => SetBobberResult(bmp, areas, displayRegion));
                return;
            }

            foreach (var area in areas)
            {
                Graphics g = Graphics.FromImage(bmp);
                var c = area.Center;
                Pen pen = new(Color.Red, 2);
                g.DrawLine(pen, c.X - 6, c.Y, c.X + 6, c.Y);
                g.DrawLine(pen, c.X, c.Y - 6, c.X, c.Y + 6);
                g.DrawRectangle(pen, area.BoundingBox);
                g.DrawString(area.ConnectedPixels.ToString(), new Font("Arial", 16.0f), Brushes.Green, new PointF((float)area.BoundingBox.Right + 5, area.BoundingBox.Top));
            }

            SetImage(bmp.Clone(displayRegion, bmp.PixelFormat));
            SetInfoText($"Found possible bobbers: {areas.Count}");
        }

        /// <summary>
        /// Display result for bobber search.
        /// </summary>
        /// <param name="pixelCount"></param>
        /// <param name="needed"></param>
        /// <param name="max"></param>
        public void SetSplashResult(int pixelCount, int needed, int max)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.Invoke(() => SetSplashResult(pixelCount, needed, max));
                return;
            }
            SetInfoText($"Status: {pixelCount} / {needed} ({pixelCount / needed * 100}%) - Max: {max} ({max / needed * 100}%)");
        }
    }
}

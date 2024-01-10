using Angelo.Bot;
using Angelo.Screen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Angelo
{
    /// <summary>
    /// Interaction logic for CallibrationWindow.xaml
    /// </summary>
    public partial class CallibrationWindow : Window
    {
        public static CallibrationWindow? Current { get; private set; } = null;
        private static readonly object _lock = new();

        private readonly Harbormaster _harbormaster;
        private Bitmap? _currentBobberBmp;
        private Rectangle? _currentBobberRegion;

        private CallibrationWindow(Harbormaster harbormaster)
        {
            InitializeComponent();
            _harbormaster = harbormaster;
            _harbormaster.RegisterEvent(Harbormaster_SplashEvent);
            _harbormaster.RegisterEvent(Harbormaster_BobberEvent);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _harbormaster.UnregisterEvent(Harbormaster_SplashEvent);
            _harbormaster.UnregisterEvent(Harbormaster_BobberEvent);
        }

        /// <summary>
        /// Get singular instance, creating it if needed. This will always run on the UI thread.
        /// </summary>
        /// <returns></returns>
        internal static CallibrationWindow GetInstance(Harbormaster harbormaster)
        {
            if (Current == null || !Current.IsLoaded)
            {
                lock (_lock)
                {
                    if (Current == null || !Current.IsLoaded)
                    {
                        Current = new CallibrationWindow(harbormaster);
                    }
                }
            }
            return Current;
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

        private void ImageDisplay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_currentBobberBmp == null || _currentBobberRegion == null)
                return;

            var cs = new CaptureScreen();
            var mousePos = e.GetPosition(ImageDisplay);
            int x = (int)mousePos.X;
            int y = (int)mousePos.Y;

            cs.SetBitmap(_currentBobberBmp, 0, 0);
            PixelColor pc = new(_currentBobberBmp.GetPixel(x, y).ToArgb());
            SetInfoText($"Hue at mouse position: {pc.GetHue()}");
        }

        private void Harbormaster_BobberEvent(Bitmap orig, Rectangle checkRegion, List<FloodCountResult> positions)
        {
            _currentBobberBmp = orig;
            _currentBobberRegion = checkRegion;

            Bitmap copy = orig.Clone(new Rectangle(0, 0, orig.Width, orig.Height), orig.PixelFormat);
            Graphics gfx = Graphics.FromImage(copy);
            Pen pen = new(Color.Red, 2);

            foreach (var area in positions)
            {
                // Offset x and y coordinates so they are relative to region instead of screen.
                var centerPoint = area.Center;
                var bBox = new Rectangle(area.X, area.Y, area.Width, area.Height);
                centerPoint.Offset(-checkRegion.X, -checkRegion.Y);
                bBox.Offset(-checkRegion.X, -checkRegion.Y);

                gfx.DrawLine(pen, centerPoint.X - 6, centerPoint.Y, centerPoint.X + 6, centerPoint.Y);
                gfx.DrawLine(pen, centerPoint.X, centerPoint.Y - 6, centerPoint.X, centerPoint.Y + 6);
                gfx.DrawRectangle(pen, bBox);
                gfx.DrawString(area.ConnectedPixels.ToString(), new Font("Arial", 16.0f), Brushes.Green, new PointF((float)bBox.Right + 5, bBox.Top));
            }

            SetImage(copy);
            SetInfoText($"Found possible bobbers: {positions.Count}");
        }

        private void Harbormaster_SplashEvent(int pixelsFound, int threshold, int maxFound)
        {
            double pctCurrent = Math.Round((double)pixelsFound / threshold * 100);
            double pctMax = Math.Round((double)maxFound / threshold * 100);
            SetInfoText($"Splash detection: {pixelsFound} / {threshold} ({pctCurrent}%) - Max: {maxFound} ({pctMax}%)");
        }
    }
}

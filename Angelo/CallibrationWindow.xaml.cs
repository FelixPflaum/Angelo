using Angelo.Bot.Bobber;
using Angelo.Screen;
using Angelo.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;

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
        private readonly BobberDetectionImg _hueSelImg;


        private CallibrationWindow(Harbormaster harbormaster)
        {
            InitializeComponent();
            _harbormaster = harbormaster;
            _harbormaster.RegisterEvent(Harbormaster_SplashEvent);
            _hueSelImg = new BobberDetectionImg();
            _hueSelImg.PixelSelect += HueSelImg_PixelSelect;
            BobberHelperPanel.Children.Add(_hueSelImg.Image);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _harbormaster.UnregisterEvent(Harbormaster_SplashEvent);
            _harbormaster.UnregisterEvent(Harbormaster_BobberEvent);
        }

        /// <summary>
        /// Get singular instance, creating it if needed.
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

        private void ShowBobber_Click(object sender, RoutedEventArgs e)
        {
            if (ShowBobber.IsChecked == true)
            {
                _harbormaster.RegisterEvent(Harbormaster_BobberEvent);
                BobberHelperPanel.Visibility = Visibility.Visible;
            }
            else
            {
                _harbormaster.UnregisterEvent(Harbormaster_BobberEvent);
                BobberHelperPanel.Visibility = Visibility.Hidden;
            }
        }

        private void HueSelImg_PixelSelect(int lastPixelHue, int avgHue, int minHue, int maxHue, int selectionCount)
        {
            BobberInfoText.Text = $"Hue at mouse position: {lastPixelHue} | Selection hue (min, max, avg): {minHue},{maxHue},{avgHue} (Over {selectionCount} pixels)";
        }

        private void Harbormaster_BobberEvent(Bitmap orig, Rectangle checkRegion, List<FloodCountResult> positions)
        {
            _hueSelImg.SetImg(orig);
            _hueSelImg.DrawBobberPositions(positions, checkRegion.Location);
            BobberInfoText.Text = $"Found {positions.Count} possible bobber(s).";
        }

        private void Harbormaster_SplashEvent(int pixelsFound, int threshold, int maxFound)
        {
            double pctCurrent = Math.Round((double)pixelsFound / threshold * 100);
            double pctMax = Math.Round((double)maxFound / threshold * 100);
            SplashStatusText.Text = $"{pixelsFound} / {threshold} ({pctCurrent}%) - Max: {maxFound} ({pctMax}%)";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var cs = new CaptureScreen();
            int w = 800;
            int h = 400;
            int x = (cs.Screen.Width - w) / 2;
            int y = (cs.Screen.Height - h) / 2;
            cs.Update(x, y, w, h);
            Bitmap bmp = cs.GetBitmapFromBuffer(x, y, w, h);
            _hueSelImg.SetImg(bmp);
        }
    }
}

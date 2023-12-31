using System;
using System.Windows.Media;
using System.Windows.Controls;
using static Angelo.UI.UIHelpers;

namespace Angelo.UI
{
    internal class StartButton : Button
    {
        private readonly Button _button;
        private readonly SolidColorBrush _defaultBrush;
        private readonly SolidColorBrush _redBrush;

        public StartButton(Button button)
        {
            if (button.Background is not SolidColorBrush)
                throw new ArgumentException("Button background is no SolidBrush!");

            _button = button;
            _defaultBrush = (SolidColorBrush)button.Background;
            _redBrush = new SolidColorBrush(GetHueShiftedColor(_defaultBrush.Color, 5.0f));
        }

        /// <summary>
        /// Shift a color to a set hue.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="hue">The hue to shift to.</param>
        /// <returns>New color shifted to hue.</returns>
        private static Color GetHueShiftedColor(Color color, float hue)
        {
            int colorValue = (color.R << 16) + (color.G << 8) + color.B;
            unchecked
            {
                colorValue |= (int)0xFF000000;
            }
            System.Drawing.Color dColor = System.Drawing.Color.FromArgb((int)colorValue);
            ushort h = (ushort)((hue % 360) / 1.5f);
            ushort l = (ushort)(dColor.GetBrightness() * 240);
            ushort s = (ushort)(dColor.GetSaturation() * 240);
            int newValue = ColorHLSToRGB(h, l, s);
            byte b = (byte)(newValue >> 16);
            byte g = (byte)((newValue >> 8) & 0xFF);
            byte r = (byte)(newValue & 0xFF);
            return Color.FromRgb(r, g, b);
        }

        /// <summary>
        /// Set button text to "Stop" and change color to red.
        /// </summary>
        public void SetToStop()
        {
            _button.Content = "Stop";
            _button.Background = _redBrush;
        }

        /// <summary>
        /// Set button text to "Start" and reset color to default.
        /// </summary>
        public void SetToStart()
        {
            _button.Content = "Start";
            _button.Background = _defaultBrush;
        }

        /// <summary>
        /// Set the enabled state of the Button control.
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            _button.IsEnabled = active;
        }
    }
}

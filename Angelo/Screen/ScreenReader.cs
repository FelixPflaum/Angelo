using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Angelo.Screen
{
    internal class ScreenReader
    {

        /// <summary>
        /// Get the resolution of the primary screen.
        /// </summary>
        /// <returns>Size containing the dimensions.</returns>
        public static System.Drawing.Size GetPrimaryScreenRes()
        {
            double w = SystemParameters.PrimaryScreenWidth;
            double h = SystemParameters.PrimaryScreenHeight;

            if (w % 1 != 0 || h % 1 != 0)
            {
                MessageBox.Show(String.Format("Dimensions from API ({0}x{1}) have decimal places. This is not supported.", w, h), "ScreenReader Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }

            return new System.Drawing.Size((int)w, (int)h);
        }
    }
}

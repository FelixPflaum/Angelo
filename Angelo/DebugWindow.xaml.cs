using Angelo.Screen;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Angelo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private static readonly Object _lock = new();
        private static DebugWindow? _instance;

        private DebugWindow()
        {
            InitializeComponent();
        }

        public static DebugWindow GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null || !_instance.IsLoaded)
                {
                    _instance = new DebugWindow();
                }
            }
            return _instance;
        }

        public BitmapImage BitmapToBMPI(Bitmap bmp)
        {
            MemoryStream mstream = new MemoryStream();
            bmp.Save(mstream, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage bmpimg = new();
            bmpimg.BeginInit();
            mstream.Seek(0, SeekOrigin.Begin);
            bmpimg.StreamSource = mstream;
            bmpimg.EndInit();
            return bmpimg;
        }

        private void DInputB1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int x = Int32.Parse(DInput1.Text);
                int y = Int32.Parse(DInput2.Text);
                int w = Int32.Parse(DInput3.Text);
                int h = Int32.Parse(DInput4.Text);
                var capt = new CaptureScreen();
                capt.Update();
                var bmp = capt.GetBitmapFromBuffer(x, y, w, h);
                var bmpi = BitmapToBMPI(bmp);
                ImageDisplay.Source = bmpi;
                DText1.Text = String.Format("W: {0} H: {1}", bmp.Width, bmp.Height);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERRRRRRR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

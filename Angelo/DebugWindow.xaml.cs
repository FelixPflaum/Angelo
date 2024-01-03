using Angelo.Bot;
using Angelo.Screen;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
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

        void SetInfo(string info)
        {
            DText1.Text = info;
        }

        void SetImage(Bitmap bmp)
        {
            var bmpi = BitmapToBMPI(bmp);
            ImageDisplay.Source = bmpi;
        }

        private void DInputB1_Click(object sender, RoutedEventArgs e)
        {
            {
                int repeats = 10;
                int x = 476;
                int y = 650;
                Bitmap bmp = new Bitmap("test3.png");

                {
                    var sw = new Stopwatch();
                    var captureScreen = new CaptureScreen();
                    var ff = new FloodCountHue(captureScreen, 0, 15);
                    double timesSum = 0;

                    Bitmap bmpCloned = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    captureScreen.SetBitmap(bmpCloned, 0, 0);

                    FloodCountResult? res = null;

                    for (int i = 0; i < repeats; i++)
                    {
                        sw.Restart();
                        res = ff.CountFrom(x, y, bmpCloned);
                        sw.Stop();
                        timesSum += sw.Elapsed.TotalMilliseconds;
                        Trace.WriteLine($"Time: {sw.Elapsed.TotalMilliseconds} | Count: {res.Value.ConnectedPixels} | Ctr: {res.Value.Center} | BBox: {res.Value.BoundingBox}");
                    }

                    Trace.WriteLine($"AVG {timesSum / repeats}");

                    if (res != null)
                    {
                        Graphics g = Graphics.FromImage(bmpCloned);
                        var c = res.Value.Center;
                        Pen pen = new Pen(Color.Red, 2);
                        g.DrawLine(pen, c.X - 6, c.Y, c.X + 6, c.Y);
                        g.DrawLine(pen, c.X, c.Y - 6, c.X, c.Y + 6);
                        g.DrawRectangle(pen, res.Value.BoundingBox);

                        SetImage(bmpCloned);
                    }
                }

                return;
            }

            try
            {
                int x = Int32.Parse(DInput1.Text);
                int y = Int32.Parse(DInput2.Text);
                int w = Int32.Parse(DInput3.Text);
                int h = Int32.Parse(DInput4.Text);
                var capt = new CaptureScreen();
                capt.Update();
                var bmp = capt.GetBitmapFromBuffer(x, y, w, h);
                SetImage(bmp);
                SetInfo(String.Format("W: {0} H: {1}", bmp.Width, bmp.Height));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERRRRRRR", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImageDisplay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton != System.Windows.Input.MouseButton.Left
                || e.ButtonState != System.Windows.Input.MouseButtonState.Pressed)
                return;

            var pos = Mouse.GetPosition(ImageDisplay);
            Bitmap bmp = new Bitmap("test3.png");
            var captureScreen = new CaptureScreen();
            captureScreen.SetBitmap(bmp, 0, 0);
            var ff = new FloodCountHue(captureScreen, 0, 15);
            try
            {
                var res = ff.CountFrom((int)pos.X, (int)pos.Y, bmp);

                Graphics g = Graphics.FromImage(bmp);
                var c = res.Center;
                Pen pen = new Pen(Color.Red, 2);
                g.DrawLine(pen, c.X - 6, c.Y, c.X + 6, c.Y);
                g.DrawLine(pen, c.X, c.Y - 6, c.X, c.Y + 6);
                g.DrawRectangle(pen, res.BoundingBox);

                SetImage(bmp);

                SetInfo($"Count: {res.ConnectedPixels} | Ctr: {res.Center} | BBox: {res.BoundingBox}");
            }
            catch
            {
                SetInfo("ERR: Invalid pixel!");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Bitmap bmp = new Bitmap("test3.png");
            SetImage(bmp);
        }
    }
}

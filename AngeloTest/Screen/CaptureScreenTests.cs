using Microsoft.VisualStudio.TestTools.UnitTesting;
using Angelo.Screen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Angelo.Screen.Tests
{
    [TestClass()]
    public class CaptureScreenTests
    {
        [TestMethod()]
        public void GetPixelTest()
        {
            const int COLOR_FILL = 0x00FF00;

            CaptureScreen cap = new();

            // Fill single color and check if pixels changed.
            {
                Bitmap bmp = new((int)cap.Screen.Width, (int)cap.Screen.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.FromArgb(0xFF, Color.FromArgb(COLOR_FILL)));
                cap.SetBitmap(bmp, 0, 0);

                PixelColor pxc = cap.GetPixel(567, 345);
                Assert.IsTrue((bmp.GetPixel(567, 345).ToArgb() & 0xFFFFFF) == COLOR_FILL);
                Assert.IsTrue(pxc.Value == COLOR_FILL, "GetPixel color mismatch after fill!");
                Assert.IsTrue(cap.CheckColorAt(567, 345, pxc), "CheckColorAt failed after fill!");
                Assert.IsTrue(cap.CheckColorAt(567, 345, COLOR_FILL), "CheckColorAt failed after fill!");
            }

            // Update and check if pixels changed.
            {
                cap.Update();
                PixelColor pxc = cap.GetPixel(0, 0);
                PixelColor pxc2 = cap.GetPixel(cap.Screen.Width - 1, cap.Screen.Height - 1);

                Assert.IsFalse(pxc.Value == COLOR_FILL, "GetPixel color did not change after update!");
                Assert.IsFalse(pxc2.Value == COLOR_FILL, "GetPixel color did not change after update!");
            }

            // Fill single color and check if pixels changed.
            {
                Bitmap bmp = new((int)cap.Screen.Width, (int)cap.Screen.Height);
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.FromArgb(0xFF, Color.FromArgb(COLOR_FILL)));
                cap.SetBitmap(bmp, 0, 0);

                PixelColor pxc = cap.GetPixel(567, 345);
                Assert.IsTrue((bmp.GetPixel(567, 345).ToArgb() & 0xFFFFFF) == COLOR_FILL);
                Assert.IsTrue(pxc.Value == COLOR_FILL, "GetPixel color mismatch after fill!");
                Assert.IsTrue(cap.CheckColorAt(567, 345, pxc), "CheckColorAt failed after fill!");
                Assert.IsTrue(cap.CheckColorAt(567, 345, COLOR_FILL), "CheckColorAt failed after fill!");
            }

            //Partial update and check pixels
            {
                cap.Update(200, 300, 100, 100);
                PixelColor pxc = cap.GetPixel(222, 333);
                Assert.IsFalse(pxc.Value == COLOR_FILL, "GetPixel color did not change after update!");
                Assert.IsFalse(cap.CheckColorAt(222, 333, COLOR_FILL), "CheckColorAt did not change after update!");
                Assert.IsFalse(cap.CheckColorAt(222, 333, COLOR_FILL), "CheckColorAt did not change after update!");
            }

            // Update single pixels and check
            {
                cap.Update(444, 444, 1, 1);
                cap.Update(555, 555, 1, 1);

                PixelColor pxc1 = cap.GetPixel(444, 444);
                PixelColor pxc2 = cap.GetPixel(555, 555);

                Assert.IsFalse(pxc1.Value == COLOR_FILL, "GetPixel color did not change after update!");
                Assert.IsFalse(pxc2.Value == COLOR_FILL, "GetPixel color did not change after update!");
            }

            // Set single pixels to color and check
            {
                const int COLOR_PIXEL = 0xFFFFFF;
                Bitmap bmp = new(1, 1);
                Graphics g = Graphics.FromImage(bmp);
                g.Clear(Color.FromArgb(0xFF, Color.FromArgb(COLOR_PIXEL)));
                cap.SetBitmap(bmp, 555, 666);
                cap.SetBitmap(bmp, 777, 888);

                PixelColor pxc1 = cap.GetPixel(555, 666);
                PixelColor pxc2 = cap.GetPixel(777, 888);
                Assert.IsTrue(pxc1.Value == COLOR_PIXEL, "GetPixel color mismatch after pixel set!");
                Assert.IsTrue(pxc2.Value == COLOR_PIXEL, "GetPixel color mismatch after pixel set!");
            }
        }

        [TestMethod()]
        public void FindPixelTest()
        {
            const int COLOR_PIXEL = 0xFFFFFF;
            CaptureScreen cap = new();
            Bitmap bmp = new(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(0xFF, Color.FromArgb(COLOR_PIXEL)));
            cap.SetBitmap(bmp, 555, 666);
            cap.SetBitmap(bmp, 777, 888);

            PixelColor pxc1 = cap.GetPixel(555, 666);
            Assert.IsTrue(pxc1.Value == COLOR_PIXEL, "GetPixel color mismatch after pixel set!");

            Point? p1 = cap.FindPixel(COLOR_PIXEL);
            Assert.IsNotNull(p1);
            Point op = p1.Value;
            Point? p2again1 = cap.FindPixel(pxc1, op);
            op.Offset(1, 0);
            Point? p2not1 = cap.FindPixel(pxc1, op);

            Assert.IsNotNull(p2again1);
            Assert.IsNotNull(p2not1);
            Assert.AreNotEqual(p1, p2not1);
            Assert.AreEqual(p1, p2again1);
        }

        [TestMethod()]
        public void GetBitmapFromBufferTest()
        {
            CaptureScreen cap = new();
            cap.Update();
            var bmp = cap.GetBitmapFromBuffer(111,222,100,100);
            PixelColor pctl = cap.GetPixel(111, 222);
            PixelColor pxbl = cap.GetPixel(210, 321);
            var bmpFullScale2 = cap.GetBitmapFromBuffer(2);

            Assert.IsTrue(bmp.Width == 100 && bmp.Height == 100);
            Assert.IsTrue(bmpFullScale2.Width == cap.Screen.Width / 2);
            Assert.IsTrue(bmpFullScale2.Height == cap.Screen.Height / 2);
            var tl = bmp.GetPixel(0, 0).ToArgb() & 0xFFFFFF;
            var bl = bmp.GetPixel(99, 99).ToArgb() & 0xFFFFFF;
            Assert.IsTrue(pctl.Value == tl, "Color mismatch TL!");
            Assert.IsTrue(pxbl.Value == bl, "Color mismatch BL!");
        }
    }
}

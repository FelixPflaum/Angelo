using Angelo.Screen;

namespace AngeloTest
{
    [TestClass]
    public class ScreenTests
    {
        [TestMethod]
        public void TestPixelColor()
        {
            {
                byte r = 0x9C;
                byte g = 0x37;
                byte b = 0x7A;
                uint expectedValue = 0x9C377A;
                var pc = new PixelColor(r, g, b);
                Assert.AreEqual(pc.Value, expectedValue, "PixelColor sets wrong value!");
            }

            {
                uint value = 0x14A1F9;
                byte expectedR = 0x14;
                byte expectedG = 0xA1;
                byte expectedB = 0xF9;
                var pc = new PixelColor(value);
                Assert.AreEqual(pc.R, expectedR, "PixelColor sets wrong R!");
                Assert.AreEqual(pc.G, expectedG, "PixelColor sets wrong R!");
                Assert.AreEqual(pc.B, expectedB, "PixelColor sets wrong R!");
            }

            {
                var pc1 = new PixelColor(0xFFAA11);
                var pc2 = new PixelColor(0x00AA00);
                Assert.IsFalse(pc1.Equals(pc2));
                Assert.IsFalse(pc1 == pc2);
                Assert.IsFalse(pc1.Contains(0xFF));
                Assert.IsTrue(pc1.Contains(0xFF0000));
                Assert.IsTrue(pc1.Contains(pc2));
            }
        }
    }
}

using Angelo.KBM;
using Angelo.Screen;

namespace AngeloTest
{
    [TestClass]
    public class KBMTests
    {
        [TestMethod]
        public void TestMouseMovementAbsolute()
        {
            var bounds = ScreenReader.GetPrimaryScreenRes();
            Random rnd = new();

            for(int i = 0; i < 200; i++)
            {
                int x = rnd.Next(bounds.Width - 1);
                int y = rnd.Next(bounds.Height - 1);
                KBMHelpers.MoveMouseAbsolute(x, y);
                Thread.Sleep(5);

                KBMHelpers.GetCursorPos(out var pos);
                Assert.IsTrue(pos.X == x && pos.Y == y, String.Format("Curser at wrong position! Expected {0} {1} but got {2} {3}", x,y,pos.X,pos.Y));
            }
        }

        [TestMethod]
        public void TestMouseMovementRelative()
        {
            var bounds = ScreenReader.GetPrimaryScreenRes();
            Random rnd = new();

            for (int mi = 0; mi < 10; mi++)
            {
                KBMHelpers.GetCursorPos(out var pos);
                int startX = pos.X;
                int startY = pos.Y;
                int targetX = rnd.Next(bounds.Width - 1);
                int targetY = rnd.Next(bounds.Height - 1);
                int dx = targetX - startX;
                int dy = targetY - startY;

                KBMHelpers.MoveMouseRelative(dx, dy);
                Thread.Sleep(5);

                KBMHelpers.GetCursorPos(out pos);
                Assert.IsTrue(pos.X == targetX && pos.Y == targetY, String.Format("Curser at wrong position! Expected {0} {1} but got {2} {3}", targetX, targetY, pos.X, pos.Y));
            }
        }
    }
}

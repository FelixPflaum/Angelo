namespace Angelo.Screen
{
    internal readonly struct ScreenData
    {
        public readonly int Width;
        public readonly int Height;
        public readonly int RefreshRate;

        public ScreenData(int width, int height, int refreshRate)
        {
            Width = width;
            Height = height;
            RefreshRate = refreshRate;
        }
    }
}

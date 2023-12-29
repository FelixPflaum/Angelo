namespace Angelo.Screen
{
    internal readonly struct ScreenData
    {
        public readonly uint Width;
        public readonly uint Height;
        public readonly uint RefreshRate;

        public ScreenData(uint width, uint height, uint refreshRate)
        {
            Width = width;
            Height = height;
            RefreshRate = refreshRate;
        }
    }
}

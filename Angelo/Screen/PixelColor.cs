namespace Angelo.Screen
{
    internal readonly struct PixelColor
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly uint Value;

        public PixelColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            Value = b;
            Value += (uint)g << 8;
            Value += (uint)r << 16;
        }

        public PixelColor(uint value)
        {
            R = (byte)(value >> 16 & 0xFF);
            G = (byte)(value >> 8 & 0xFF);
            B = (byte)(value & 0xFF);
            Value = value;
        }

        /// <summary>
        /// Check if this color contains <paramref name="value"/>, i.e. the same bits are set.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public readonly bool Contains(uint value)
        {
            return (Value & value) == value;
        }

        /// <summary>
        /// Check if this color contains value of <paramref name="otherColor"/>, i.e. the same bits are set.
        /// </summary>
        /// <param name="otherColor"></param>
        /// <returns></returns>
        public readonly bool Contains(PixelColor otherColor)
        {
            return (Value & otherColor.Value) == otherColor.Value;
        }

        public static bool operator ==(PixelColor a, PixelColor b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(PixelColor a, PixelColor b)
        {
            return a.Value != b.Value;
        }

        public override readonly string ToString()
        {
            return string.Format("{0} {1} {2} (#{3})", R, G, B, Value);
        }

        public override readonly int GetHashCode()
        {
            return (int)Value;
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj == null || obj is not PixelColor)
                return false;
            else
                return Value == ((PixelColor)obj).Value;
        }
    }
}

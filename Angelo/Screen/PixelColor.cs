using System;

namespace Angelo.Screen
{
    internal readonly struct PixelColor
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly int Value;

        public PixelColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            Value = b;
            Value += g << 8;
            Value += r << 16;
        }

        public PixelColor(int value)
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
        public readonly bool Contains(int value)
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
            return Value;
        }

        public override readonly bool Equals(object? obj)
        {
            if (obj == null || obj is not PixelColor)
                return false;
            else
                return Value == ((PixelColor)obj).Value;
        }

        /// <summary>
        /// Get hue value.
        /// </summary>
        /// <returns>The 0-360° hue value.</returns>
        public int GetHue()
        {
            float r = (R / 255.0f);
            float g = (G / 255.0f);
            float b = (B / 255.0f);
            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);
            float delta = max - min;
            float hue = 0;

            if (delta != 0)
            {
                if (r == max)
                    hue = ((g - b) / 6) / delta;
                else if (g == max)
                    hue = (1.0f / 3) + ((b - r) / 6) / delta;
                else
                    hue = (2.0f / 3) + ((r - g) / 6) / delta;

                if (hue < 0)
                    hue += 1;
                if (hue > 1)
                    hue -= 1;
            }

            return (int)(hue * 360);
        }
    }
}

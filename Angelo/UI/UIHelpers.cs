using System.Runtime.InteropServices;

namespace Angelo.UI
{
    internal static class UIHelpers
    {
        /// <summary>
        /// Converts colors from hue-luminance-saturation (HLS) to RGB format.
        /// </summary>
        /// <param name="H">The original HLS hue value. Can range from 0 to 240.</param>
        /// <param name="L">The original HLS luminance value. Can range from 0 to 240.</param>
        /// <param name="S">The original HLS saturation value. Can range from 0 to 240.</param>
        /// <returns>Returns the RGB value. Byteorder is BGR!</returns>
        [DllImport("shlwapi.dll")]
        public static extern int ColorHLSToRGB(ushort H, ushort L, ushort S);
    }
}

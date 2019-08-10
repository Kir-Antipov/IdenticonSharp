#if NETFRAMEWORK
using System.Drawing;
#else
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
#endif

namespace KE.IdenticonSharp.Compatibility
{
    internal static class ColorHelper
    {
#if NETFRAMEWORK
        public static Color FromRgb(byte r, byte g, byte b) => Color.FromArgb(r, g, b);
        public static Color FromRgb(int r, int g, int b) => Color.FromArgb((byte)r, (byte)g, (byte)b);
        public static Color FromRgb(double r, double g, double b) => Color.FromArgb((byte)r, (byte)g, (byte)b);
#else
        public static Color FromRgb(byte r, byte g, byte b) => new Color(r, g, b);
        public static Color FromRgb(int r, int g, int b) => new Color((byte)r, (byte)g, (byte)b);
        public static Color FromRgb(double r, double g, double b) => new Color((byte)r, (byte)g, (byte)b);
#endif
    }
}

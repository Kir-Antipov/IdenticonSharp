using System;

#if NETFRAMEWORK
using System.Drawing;
#else
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
#endif

namespace IdenticonSharp.Compatibility
{
    internal static class ColorHelper
    {
#if NETFRAMEWORK
        public static Color FromRgb(byte r, byte g, byte b) => Color.FromArgb(r, g, b);
        public static Color FromRgb(int r, int g, int b) => Color.FromArgb((byte)r, (byte)g, (byte)b);
        public static Color FromRgb(double r, double g, double b) => Color.FromArgb((byte)r, (byte)g, (byte)b);

        public static Color FromHex(int rgb) => FromHex((uint)(0xFF000000 | rgb));
        public static Color FromHex(uint argb) => Color.FromArgb((int)argb);
#else
        public static Color FromRgb(byte r, byte g, byte b) => new Color(r, g, b);
        public static Color FromRgb(int r, int g, int b) => new Color((byte)r, (byte)g, (byte)b);
        public static Color FromRgb(double r, double g, double b) => new Color((byte)r, (byte)g, (byte)b);

        public static Color FromHex(int rgb) => FromHex((uint)(0xFF000000 | rgb));
        public static Color FromHex(uint argb)
        {
            byte a = (byte)((argb & 0xFF000000) >> 24);
            byte r = (byte)((argb & 0x00FF0000) >> 16);
            byte g = (byte)((argb & 0x0000FF00) >> 8);
            byte b = (byte)((argb & 0x000000FF));

            return new Color(r, g, b, a);
        }
#endif

        public static Color FromHsl(double h, double s, double l)
        {
            double c = (1 - Math.Abs(2 * l - 1)) * s;
            double x = c * (1 - Math.Abs(h / 60.0 % 2 - 1));
            double m = l - c / 2;
            double[] vals;
            switch ((int)(h / 60) % 6)
            {
                case 0:
                    vals = new[] { c, x, 0 };
                    break;
                case 1:
                    vals = new[] { x, c, 0 };
                    break;
                case 2:
                    vals = new[] { 0, c, x };
                    break;
                case 3:
                    vals = new[] { 0, x, c };
                    break;
                case 4:
                    vals = new[] { x, 0, c };
                    break;
                case 5:
                    vals = new[] { c, 0, x };
                    break;
                default:
                    vals = new[] { 0.0, 0.0, 0.0 };
                    break;
            }

            return FromRgb(Math.Round((vals[0] + m) * 255), Math.Round((vals[1] + m) * 255), Math.Round((vals[2] + m) * 255));
        }
    }
}

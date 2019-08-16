#if NETFRAMEWORK
using System.Drawing;
#else
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
#endif

namespace IdenticonSharp.Compatibility
{
    public static class ImageHelper
    {
        public static Image Load(string filename)
        {
#if NETFRAMEWORK
            return Image.FromFile(filename);
#else
            return SixLabors.ImageSharp.Image.Load(filename);
#endif
        }
    }
}

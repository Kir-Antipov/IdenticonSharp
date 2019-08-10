#if NETFRAMEWORK
using System.Drawing;
#else
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
#endif

namespace IdenticonSharp.Identicons
{
    public interface IIdenticonOptions
    {
        Color Background { get; set; }
    }
}

using IdenticonSharp.Svg;

#if NETFRAMEWORK
using System.Drawing;
#else
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
#endif

namespace IdenticonSharp.Identicons
{
    public interface IIdenticonProvider
    {
        bool ProvidesSvg { get; }

        Image Create(string input);
        Image Create(byte[] input);

        SvgBuilder CreateSvg(string input);
        SvgBuilder CreateSvg(byte[] input);
    }
}

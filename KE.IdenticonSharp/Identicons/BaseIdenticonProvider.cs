using System;
using System.Text;
using IdenticonSharp.Svg;
using IdenticonSharp.Helpers;

#if NETFRAMEWORK
using System.Drawing;
#else
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
#endif

namespace IdenticonSharp.Identicons
{
    public abstract class BaseIdenticonProvider : IIdenticonProvider
    {
        public virtual bool ProvidesSvg => false;

        public Image Create(string input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            return CreateFromBytes(Encoding.UTF8.GetBytes(input));
        }

        public Image Create(byte[] input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            return CreateFromBytes(input);
        }

        public SvgBuilder CreateSvg(string input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            return CreateSvgFromBytes(Encoding.UTF8.GetBytes(input));
        }

        public SvgBuilder CreateSvg(byte[] input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            return CreateSvgFromBytes(input);
        }

        protected abstract Image CreateFromBytes(byte[] input);

        protected virtual SvgBuilder CreateSvgFromBytes(byte[] input)
        {
            Image image = CreateFromBytes(input);
            return new SvgBuilder().Append(new SvgImage(image.ToBase64Link()) { PercentageWidth = 100, PercentageHeight = 100 });
        }
    }
}

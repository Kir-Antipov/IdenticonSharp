using System;
using System.Linq;
using IdenticonSharp.Svg;
using IdenticonSharp.Helpers;
using System.Collections.Generic;
using IdenticonSharp.Compatibility;

#if NETFRAMEWORK
using System.Drawing;
#else
using SixLabors.Primitives;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
#endif

namespace IdenticonSharp.Identicons.Defaults.GitHub
{
    public class GitHubIdenticonProvider : BaseIdenticonProvider, IIdenticonProvider<GitHubIdenticonOptions>
    {
        #region Var
        public override bool ProvidesSvg => true;
        public GitHubIdenticonOptions Options { get; } = new GitHubIdenticonOptions();
        #endregion

        private T CreateFromBytes<T>(byte[] input, Func<bool[,], Color, Color, T> mapper)
        {
            byte[] hash = Options.HashAlgorithm.ComputeHash(input);
            if ((hash?.Length ?? 0) < 1)
                throw new FormatException("The hash algorithm worked unexpectedly");

            int size = Options.SpriteSize;
            bool[,] sprite = new bool[size, size];
            FillSpriteWithHash(sprite, hash);

            return mapper(sprite, Options.Background, ComputeForeground(hash));
        }

        protected override Image CreateFromBytes(byte[] input) => CreateFromBytes(input, FillBitmap);
        protected override SvgBuilder CreateSvgFromBytes(byte[] input) => CreateFromBytes(input, FillSvg);

        protected virtual void FillSpriteWithHash(bool[,] sprite, byte[] hash)
        {
            int side = sprite.GetLength(0);
            int half = (int)Math.Round(side / 2.0, MidpointRounding.AwayFromZero) - 1;

            IEnumerator<bool> flags = hash.Loop().Select(x => (x & 1) == 0).GetEnumerator();
            flags.MoveNext();

            for (int x = half; x >= 0; --x)
                for (int y = 0; y < side; ++y, flags.MoveNext())
                    sprite[y, x] = sprite[y, side - x - 1] = flags.Current;
        }

        protected virtual Color ComputeForeground(byte[] hash)
        {
            byte[] lastBytes = hash.Reverse().Loop().Take(4).Reverse().ToArray();

            double h = (((lastBytes[0] & 0x0f) << 8) | lastBytes[1]).Map(0, 4095, 0, 360);
            double s = lastBytes[2].Map(0, 255, 0, 20);
            double l = lastBytes[3].Map(0, 255, 0, 20);

            return ColorHelper.FromHsl(h, 65 - s, 75 - l);
        }

        protected virtual Bitmap FillBitmap(bool[,] sprite, Color background, Color foreground)
        {
            int side = sprite.GetLength(0);
            int imageSide = side * Options.Scale + Options.Border * 2;

            Bitmap img = new Bitmap(imageSide, imageSide);
            img.Mutate(context =>
            {
                context.Fill(background);
                for (int y = 0; y < side; ++y)
                    for (int x = 0; x < side; ++x)
                        if (sprite[y, x])
                            context.Fill(foreground, new RectangleF(Options.Border + x * Options.Scale, Options.Border + y * Options.Scale, Options.Scale, Options.Scale));
            });
            if (imageSide != Options.Size)
                img = img.Resize(Options.Size, Options.Size);

            return img;
        }

        protected virtual SvgBuilder FillSvg(bool[,] sprite, Color background, Color foreground)
        {
            int side = sprite.GetLength(0);
            decimal offset = Options.Border / (decimal)Options.Scale;
            decimal fullSide = side + 2m * offset;

            SvgBuilder svg = new SvgBuilder()
                .SetViewBox(0, 0, fullSide, fullSide)
                .Append(new SvgRect {
                    PercentageWidth = 100,
                    PercentageHeight = 100,
                    Color = background
                });

            SvgPathBuilder path = new SvgPathBuilder { Color = foreground };
            for (int y = 0; y < side; ++y)
                for (int x = 0; x < side; ++x)
                    if (sprite[y, x])
                        path.AppendPoint(x + offset, y + offset)
                            .AppendRelativeHorizontalLine(1)
                            .AppendRelativeVerticalLine(1)
                            .AppendRelativeHorizontalLine(-1)
                            .AppendClosePath();

            return svg.Append(path);
        }
    }
}

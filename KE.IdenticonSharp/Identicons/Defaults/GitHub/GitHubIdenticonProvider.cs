using System;
using System.Linq;
using IdenticonSharp.Svg;
using IdenticonSharp.Helpers;
using System.Collections.Generic;
using KE.IdenticonSharp.Compatibility;

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

        private T CreateFromBytes<T>(byte[] input, Func<bool[,], int, int, Color, Color, T> mapper)
        {
            byte[] hash = Options.HashAlgorithm.ComputeHash(input);
            if ((hash?.Length ?? 0) < 1)
                throw new FormatException("The hash algorithm worked unexpectedly");

            int size = Options.SpriteSize;
            bool[,] sprite = new bool[size, size];
            FillSpriteWithHash(sprite, hash);

            return mapper(sprite, Options.Factor, Options.Offset, Options.Background, ComputeForeground(hash));
        }

        protected override Image CreateFromBytes(byte[] input) => CreateFromBytes(input, FillBitmap);
        protected override SvgBuilder CreateSvgFromBytes(byte[] input) => CreateFromBytes(input, FillSvg);

        protected virtual void FillSpriteWithHash(bool[,] sprite, byte[] hash)
        {
            int height = sprite.GetLength(0);
            int width = sprite.GetLength(1);
            int half = (int)Math.Round(width / 2.0, MidpointRounding.AwayFromZero) - 1;

            IEnumerator<bool> flags = hash.Loop().Select(x => (x & 1) == 0).GetEnumerator();
            flags.MoveNext();

            for (int x = half; x >= 0; --x)
                for (int y = 0; y < height; ++y, flags.MoveNext())
                    sprite[y, x] = sprite[y, width - x - 1] = flags.Current;
        }

        protected virtual Color ComputeForeground(byte[] hash)
        {
            byte[] lastBytes = hash.Reverse().Loop().Take(4).Reverse().ToArray();

            double h = (((lastBytes[0] & 0x0f) << 8) | lastBytes[1]).Map(0, 4095, 0, 360);
            double s = lastBytes[2].Map(0, 255, 0, 20);
            double l = lastBytes[3].Map(0, 255, 0, 20);

            return ColorHelper.FromHsl(h, 65 - s, 75 - l);
        }

        protected virtual Bitmap FillBitmap(bool[,] sprite, int factor, int offset, Color background, Color foreground)
        {
            int height = sprite.GetLength(0);
            int width = sprite.GetLength(1);

            Bitmap img = new Bitmap(width * factor + offset * 2, height * factor + offset * 2);

            img.Mutate(context =>
            {
                context.Fill(background);
                for (int y = 0; y < height; ++y)
                    for (int x = 0; x < width; ++x)
                        if (sprite[y, x])
                            context.Fill(foreground, new RectangleF(offset + x * factor, offset + y * factor, factor, factor));
            });

            return img;
        }

        protected virtual SvgBuilder FillSvg(bool[,] sprite, int factor, int offset, Color background, Color foreground)
        {
            int height = sprite.GetLength(0);
            int width = sprite.GetLength(1);

            int fullWidth = width * factor + 2 * offset;
            int fullHeight = height * factor + 2 * offset;

            double dOffsetX = offset * 100.0 / fullWidth;
            double dOffsetY = offset * 100.0 / fullHeight;
            double dFactorX = factor * 100.0 / fullWidth;
            double dFactorY = factor * 100.0 / fullHeight;

            SvgBuilder svg = new SvgBuilder { Color = background };
            for (int y = 0; y < height; ++y)
                for (int x = 0; x < width; ++x)
                    if (sprite[y, x])
                        svg.Append(new SvgRect {
                            PercentageX = dOffsetX + dFactorX * x,
                            PercentageY = dOffsetY + dFactorY * y,
                            PercentageWidth = dFactorX,
                            PercentageHeight = dFactorY,
                            Color = foreground
                        });

            return svg;
        }
    }
}

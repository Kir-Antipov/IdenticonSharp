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

            return FromHSL(h, 65 - s, 75 - l);
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

        private static Color FromHSL(double h, double s, double l)
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

            return ColorHelper.FromRgb(Math.Round((vals[0] + m) * 255), Math.Round((vals[1] + m) * 255), Math.Round((vals[2] + m) * 255));
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

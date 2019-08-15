using System;
using IdenticonSharp.Svg;
using IdenticonSharp.Helpers;
using IdenticonSharp.Compatibility;

#if NETFRAMEWORK
using System.Drawing;
#else
using SixLabors.Primitives;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
#endif

namespace IdenticonSharp.Identicons.Defaults.QR
{
    public class QRIdenticonProvider : IIdenticonProvider<QRIdenticonOptions>
    {
        #region Var
        public bool ProvidesSvg => true;
        public QRIdenticonOptions Options { get; } = new QRIdenticonOptions();
        #endregion

        #region Functions
        public Image Create(string text) => Process(text, QRToImage);
        public Image Create(byte[] bytes) => Process(bytes, QRToImage);

        public SvgBuilder CreateSvg(string text) => Process(text, QRToSvg);
        public SvgBuilder CreateSvg(byte[] bytes) => Process(bytes, QRToSvg);

        private T Process<T>(string text, Func<QRCode, T> processor)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            try
            {
                return processor(QRCode.EncodeText(text, Options.CorrectionLevel));
            }
            catch
            {
                return default;
            }
        }
        private T Process<T>(byte[] bytes, Func<QRCode, T> processor)
        {
            if (bytes is null)
                throw new ArgumentNullException(nameof(bytes));

            try
            {
                return processor(QRCode.EncodeBytes(bytes, Options.CorrectionLevel));
            }
            catch
            {
                return default;
            }
        }

        private Image QRToImage(QRCode qr)
        {
            int border = Options.Border;
            int scale = Options.Scale;
            int size = scale * qr.Size + 2 * border;

            Image image = new Bitmap(size, size);
            image.Mutate(context => {
                context.Fill(Options.Background);
                qr.Paint((x, y) => context.Fill(Options.Foreground, new RectangleF(border + x * scale, border + y * scale, scale, scale)));

                if (Options.CenterImage != null)
                {
                    float damage = qr.CorrectionLevel.MaxDamage / 2f;

                    float ratio = Options.CenterImage.Height / (float)Options.CenterImage.Width;

                    int width = (int)(size * Math.Sqrt(damage / ratio));
                    int height = (int)(width * ratio);

                    context.DrawImage(Options.CenterImage, new Rectangle((size - width) / 2, (size - height) / 2, width, height));
                }
            });

            return image;
        }

        private SvgBuilder QRToSvg(QRCode qr)
        {
            decimal border = Options.Border / (decimal)Options.Scale;
            decimal fullSize = qr.Size + 2m * border;

            SvgBuilder svg = new SvgBuilder()
                .SetViewBox(0, 0, fullSize, fullSize)
                .Append(new SvgRect
                {
                    PercentageWidth = 100,
                    PercentageHeight = 100,
                    Color = Options.Background
                });

            SvgPathBuilder path = new SvgPathBuilder { Color = Options.Foreground };
            qr.Paint((x, y) => {
                path.AppendPoint(x + border, y + border)
                    .AppendRelativeHorizontalLine(1)
                    .AppendRelativeVerticalLine(1)
                    .AppendRelativeHorizontalLine(-1)
                    .AppendClosePath();
            });

            svg.Append(path);

            if (Options.CenterImage != null)
            {
                double damage = Math.Sqrt(qr.CorrectionLevel.MaxDamage / 2.0);
                double ratioX = Options.CenterImage.Width / (double)Options.CenterImage.Height;
                double ratioY = Options.CenterImage.Height / (double)Options.CenterImage.Width;

                double width = damage * ratioX;
                double height = damage * ratioY;

                SvgImage img = new SvgImage {
                    PercentageX = (1.0 - width) * 50.0,
                    PercentageY = (1.0 - height) * 50.0,
                    PercentageWidth = width * 100.0,
                    PercentageHeight = height * 100.0,
                    Link = Options.CenterImage.ToBase64Link()
                };

                svg.Append(img);
            }

            return svg;
        }
        #endregion
    }
}

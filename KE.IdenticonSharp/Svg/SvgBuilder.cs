using System.Xml.Linq;
using IdenticonSharp.Helpers;

#if NETFRAMEWORK
using System.Drawing;
#else
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
#endif

namespace IdenticonSharp.Svg
{
    public class SvgBuilder : SvgElement
    {
        #region Var
        private readonly XDocument Document;

        public override Color Color
        {
            get => base.Color;
            set => SetStringAttribute("style", $"background-color:rgba({value.R},{value.G},{value.B},{(value.A / 255.0).ToNormalizedString("F1")})");
        }
        #endregion

        #region Init
        public SvgBuilder() : base(XDocument.Parse("<svg></svg>").Root) => Document = Root.Document;
        #endregion

        #region Functions
        public static implicit operator XDocument(SvgBuilder builder) => builder.Document;

        public SvgBuilder Append(SvgElement element)
        {
            Root.Add((XElement)element);
            return this;
        }

        public SvgBuilder SetViewBox(decimal x, decimal y, decimal width, decimal height)
        {
            SetStringAttribute("viewbox", $"{x.ToNormalizedString()} {y.ToNormalizedString()} {width.ToNormalizedString()} {height.ToNormalizedString()}");
            return this;
        }

        public SvgBuilder SetViewBox(int x, int y, int width, int height)
        {
            SetStringAttribute("viewbox", $"{x} {y} {width} {height}");
            return this;
        }

        public void Save(string filename) => Document.Save(filename);
        #endregion
    }
}

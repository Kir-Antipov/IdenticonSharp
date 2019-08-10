using System.Xml.Linq;

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
            set => SetStringAttribute("style", $"background-color:rgba({value.R},{value.G},{value.B},{(value.A / 255.0).ToString("F1").Replace(',', '.')})");
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

        public void Save(string filename) => Document.Save(filename);
        #endregion
    }
}

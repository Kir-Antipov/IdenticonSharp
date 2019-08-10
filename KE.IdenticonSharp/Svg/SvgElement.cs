using System;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System.Collections.Generic;

#if NETFRAMEWORK
using System.Drawing;
#else
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
#endif

namespace IdenticonSharp.Svg
{
    public class SvgElement
    {
        #region Var
        public virtual string Width
        {
            get => GetStringAttribute("width");
            set => SetStringAttribute("width", value);
        }
        public virtual string Height
        {
            get => GetStringAttribute("height");
            set => SetStringAttribute("height", value);
        }

        public virtual double PercentageWidth { set => Width = $"{value.ToString("F2").Replace(',', '.')}%"; }
        public virtual double PercentageHeight { set => Height = $"{value.ToString("F2").Replace(',', '.')}%"; }

        public virtual int PixelWidth { set => Width = $"{value}px"; }
        public virtual int PixelHeight { set => Height = $"{value}px"; }

        public virtual string X
        {
            get => GetStringAttribute("x");
            set => SetStringAttribute("x", value);
        }
        public virtual string Y
        {
            get => GetStringAttribute("y");
            set => SetStringAttribute("y", value);
        }

        public virtual double PercentageX { set => X = $"{value.ToString("F2").Replace(',', '.')}%"; }
        public virtual double PercentageY { set => Y = $"{value.ToString("F2").Replace(',', '.')}%"; }

        public virtual int PixelX { set => X = $"{value}px"; }
        public virtual int PixelY { set => Y = $"{value}px"; }

        public virtual Color Color
        {
            get
            {
                XAttribute attribute = Root.Attribute("style");
                if (attribute is null)
                    return default;
                string value = attribute.Value ?? string.Empty;
                int start = value.IndexOf("rgba(");
                int end = value.IndexOf(')', start);
                if (start == -1 || end == -1)
                    return default;
                string rgbaStr = value.Substring(start + 5, end - start - 5);
                byte[] rgba = rgbaStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => double.TryParse(x.Replace('.', ','), NumberStyles.Float, new CultureInfo(1049), out var val) ? val : 0)
                    .Take(4)
                    .Select((x, i) => (byte)Math.Round(i == 3 ? x * 255 : x, MidpointRounding.AwayFromZero)).ToArray();
                if (rgba.Length < 4)
                    return default;
#if NETFRAMEWORK
                return Color.FromArgb(rgba[3], rgba[0], rgba[1], rgba[2]);
#else
                return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
#endif
            }
            set => SetStringAttribute("style", $"fill:rgba({value.R},{value.G},{value.B},{(value.A / 255.0).ToString("F1").Replace(',', '.')})");
        }

        public IEnumerable<SvgElement> Elements => Root.Elements().OfType<XElement>().Select(x => new SvgElement(x));

        protected readonly XElement Root;
#endregion

#region Init
        public SvgElement(XElement root) => Root = root;

        public SvgElement(XName name) => Root = new XElement(name);
#endregion

#region Functions
        public static explicit operator XElement(SvgElement element) => element.Root;

        protected string GetStringAttribute(string name) => Root.Attribute(name)?.Value ?? string.Empty;
        protected void SetStringAttribute(string name, string value)
        {
            XAttribute attribute = Root.Attribute(name);
            if (string.IsNullOrEmpty(value))
                attribute?.Remove();
            else
            {
                if (attribute is null)
                    Root.Add(new XAttribute(name, value));
                else
                    attribute.SetValue(value);
            }
        }

        public override string ToString() => Root.ToString();
#endregion
    }
}

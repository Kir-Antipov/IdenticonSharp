using IdenticonSharp.Helpers;

namespace IdenticonSharp.Svg
{
    public class SvgPathBuilder : SvgElement
    {
        #region Var
        private const string PathAttribute = "d";
        #endregion

        #region Init
        public SvgPathBuilder() : base("path") => SetStringAttribute(PathAttribute, string.Empty);
        #endregion

        #region Functions
        public SvgPathBuilder AppendPoint(int x, int y) => Append($"M{x} {y}");
        public SvgPathBuilder AppendPoint(decimal x, decimal y) => Append($"M{x.ToNormalizedString()} {y.ToNormalizedString()}");
        public SvgPathBuilder AppendRelativePoint(int dX, int dY) => Append($"m{dX} {dY}");
        public SvgPathBuilder AppendHorizontalLine(int x) => Append($"H{x}");
        public SvgPathBuilder AppendRelativeHorizontalLine(int dX) => Append($"h{dX}");
        public SvgPathBuilder AppendVerticalLine(int y) => Append($"V{y}");
        public SvgPathBuilder AppendRelativeVerticalLine(int dY) => Append($"v{dY}");
        public SvgPathBuilder AppendLine(int x, int y) => Append($"L{x} {y}");
        public SvgPathBuilder AppendRelativeLine(int dX, int dY) => Append($"l{dX} {dY}");
        public SvgPathBuilder AppendClosePath() => Append("z");

        private SvgPathBuilder Append(string value)
        {
            SetStringAttribute(PathAttribute, GetStringAttribute(PathAttribute) + value);
            return this;
        }
        #endregion
    }
}

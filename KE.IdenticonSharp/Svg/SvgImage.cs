namespace IdenticonSharp.Svg
{
    public class SvgImage : SvgElement
    {
        #region Var
        public string Link
        {
            get => GetStringAttribute("href");
            set => SetStringAttribute("href", value);
        }
        #endregion

        #region Init
        public SvgImage() : base("image") { }
        public SvgImage(string link) : this() => Link = link;
        #endregion
    }
}

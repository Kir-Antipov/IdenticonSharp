using System.Linq;
using System.Text;
using IdenticonSharp.Identicons;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace KE.IdenticonSharp.AspNetCore.TagHelpers
{
    [HtmlTargetElement("gravatar", TagStructure = TagStructure.WithoutEndTag)]
    public class GravatarTagHelper : TagHelper
    {
        #region Var
        private const string GravatarBase = "https://www.gravatar.com/avatar/";

        public string Value { get; set; }
        #endregion

        #region Process
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string value = Value ?? string.Empty;
            string hash = string.Concat(HashProvider.MD5.ComputeHash(Encoding.UTF8.GetBytes(value)).Select(x => x.ToString("x2")));

            output.TagName = "img";
            output.TagMode = TagMode.StartTagOnly;
            output.Attributes.Add("src", $"{GravatarBase}{hash}");
        }
        #endregion
    }
}

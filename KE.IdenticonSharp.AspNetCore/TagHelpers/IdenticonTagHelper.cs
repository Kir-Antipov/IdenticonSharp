﻿using System.Text;
using IdenticonSharp;
using System.Xml.Linq;
using IdenticonSharp.Svg;
using IdenticonSharp.Helpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace KE.IdenticonSharp.AspNetCore.TagHelpers
{
    [HtmlTargetElement("identicon", TagStructure = TagStructure.WithoutEndTag)]
    public class IdenticonTagHelper : TagHelper
    {
        #region Var
        public bool Svg { get; set; }
        public string Value { get; set; }
        #endregion

        #region Process
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string value = Value ?? string.Empty;
            if (Svg)
                ProcessSvg(value, output);
            else
                ProcessImage(value, output);
        }

        private void ProcessSvg(string value, TagHelperOutput output)
        {
            output.TagName = "svg";
            output.TagMode = TagMode.StartTagAndEndTag;

            SvgBuilder svg = IdenticonManager.Default.CreateSvg(value);
            StringBuilder innerSvg = new StringBuilder(256);
            foreach (var x in ((XElement)svg).Elements())
                innerSvg.Append(x);

            foreach (XAttribute attribute in ((XElement)svg).Attributes())
                output.Attributes.Add(attribute.Name.LocalName, attribute.Value);

            output.Content.SetHtmlContent(innerSvg.ToString());
        }

        private void ProcessImage(string value, TagHelperOutput output)
        {
            output.TagName = "img";
            output.TagMode = TagMode.StartTagOnly;
            output.Attributes.Add("src", IdenticonManager.Default.Create(value).ToBase64Link());
        }
        #endregion
    }
}

using System.Web;
using System.Linq;
using System.Text;
using IdenticonSharp;
using System.Xml.Linq;
using IdenticonSharp.Svg;
using IdenticonSharp.Helpers;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace KE.IdenticonSharp.AspNetCore.TagHelpers
{
    [HtmlTargetElement("otp", TagStructure = TagStructure.WithoutEndTag)]
    public class OtpTagHelper : TagHelper
    {
        #region Var
        public bool Svg { get; set; }

        public string Issuer
        {
            get => _issuer;
            set => _issuer = HttpUtility.UrlEncode((value ?? string.Empty).Replace(":", string.Empty));
        }
        private string _issuer;

        public string User
        {
            get => _user;
            set => _user = HttpUtility.UrlEncode((value ?? string.Empty).Replace(":", string.Empty));
        }
        private string _user;

        public int Digits
        {
            get => _digits;
            set => _digits = value < 7 ? 6 : 8;
        }
        private int _digits = 6;

        public bool? EncodeSecret { get; set; }

        public string Secret { get; set; }
        public OtpType Type { get; set; } = OtpType.Default;
        public OtpAlgorithm Algorithm { get; set; } = OtpAlgorithm.Default;
        public int Period { get; set; } = 30;
        public long Counter { get; set; }

        private const string Base32Charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        #endregion

        #region Process
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string link = GenerateLink();
            if (string.IsNullOrEmpty(link))
                return;

            if (Svg)
                ProcessSvg(link, output);
            else
                ProcessImage(link, output);
        }

        private string GenerateLink()
        {
            if (string.IsNullOrEmpty(Secret))
                return null;

            StringBuilder builder = new StringBuilder(128)
                .Append("otpauth://")
                .Append(Type.ToString().ToLower())
                .Append('/');

            if (!string.IsNullOrEmpty(Issuer))
                builder.Append(Issuer).Append(':');

            if (!string.IsNullOrEmpty(User))
                builder.Append(User);

            string encSecret = Secret;
            if (!EncodeSecret.HasValue && encSecret.Any(x => !Base32Charset.Contains(x)) || EncodeSecret.Value)
                encSecret = OtpBase32Encode(encSecret);

            builder.AppendFormat("?secret={0}&algorithm={1}&digits={2}&period={3}&counter={4}", encSecret, Algorithm, Digits, Period, Counter);

            if (!string.IsNullOrEmpty(Issuer))
                builder.AppendFormat("&issuer={0}", Issuer);

            return builder.ToString();
        }

        private void ProcessSvg(string value, TagHelperOutput output)
        {
            output.TagName = "svg";
            output.TagMode = TagMode.StartTagAndEndTag;

            SvgBuilder svg = IdenticonManager.Get("qr").CreateSvg(value);
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
            output.Attributes.Add("src", IdenticonManager.Get("qr").Create(value).ToBase64Link());
        }

        private static string OtpBase32Encode(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            StringBuilder builder = new StringBuilder((bytes.Length + 5 - bytes.Length % 5) / 5 * 8);
            var buckets = bytes.Bucket(5).ToArray();
            int last = buckets.Length - 1;
            for (int i = 0; i < last; ++i)
            {
                var chars = buckets[i]
                    .SelectMany(x => x.ToBits())
                    .Bucket(8)
                    .Select(x => x.Aggregate(0, (a, b) => a * 2 + (b ? 1 : 0)))
                    .Select(x => Base32Charset[x]);

                foreach (char chr in chars)
                    builder.Append(chr);
            }

            List<byte> lastBucket = buckets[last].ToList();
            var lastChars = lastBucket
                .PadRight(5)
                .SelectMany(x => x.ToBits())
                .Bucket(5)
                .Select(x => x.Aggregate(0, (a, b) => a * 2 + (b ? 1 : 0)))
                .Select(x => Base32Charset[x]);

            foreach (char chr in lastChars)
                builder.Append(chr);

            // The padding specified in RFC 3548 section 2.2 is not required and should be omitted
            int replace = new[] { -1, 6, 4, 3, 1, 0 }[lastBucket.Count];
            return builder.ToString(0, builder.Length - replace);
        }
        #endregion
    }

    public enum OtpType
    {
        Totp,
        Hotp,

        Default = Totp
    }

    public enum OtpAlgorithm
    {
        SHA1,
        SHA256,
        SHA512,

        Default = SHA1
    }
}

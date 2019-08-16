using System;
using System.IO;
using System.Linq;
using IdenticonSharp.Svg;
using IdenticonSharp.Helpers;
using System.Threading.Tasks;
using IdenticonSharp.Compatibility;

#if NETFRAMEWORK
using System.Net;
using System.Drawing;
#else
using System.Net.Http;
using SixLabors.Primitives;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.PixelFormats.Rgba32;
using Image = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
using Bitmap = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;
#endif

namespace IdenticonSharp.Identicons.Defaults.Animal
{
    public class AnimalIdenticonProvider : BaseIdenticonProvider, IIdenticonProvider<AnimalIdenticonOptions>
    {
        #region Var
        private static readonly Color[] Colors;
        private static readonly string[] Animals;

        private const string BaseUrl = "https://ssl.gstatic.com/docs/common/profile/{0}_lg.png";

        private readonly Lazy<Task<Image>[]> Images;
        public AnimalIdenticonOptions Options { get; } = new AnimalIdenticonOptions();
        #endregion

        #region Init
        static AnimalIdenticonProvider()
        {
            int[] hexColors = new[] {
                0x721ACB, 0x841ACB, 0x931ACB, 0xA51ACB, 0xB41ACB, 0xC51ACB, 0xCB1ABF, 0xCB1AB1,
                0xCB1A9F, 0xCB1A8D, 0xCB1A7E, 0xCB1A6C, 0xCB1A5E, 0xCB1A4C, 0xCB1A3A, 0xCB1A2B,
                0xCB1A1A, 0xCB2B1A, 0xCB3A1A, 0xCB4C1A, 0xCB5E1A, 0xCB6C1A, 0xCB7E1A, 0xCB8D1A,
                0xCB9F1A, 0xCBB11A, 0xCBBF1A, 0xC5CB1A, 0xB4CB1A, 0xA5CB1A, 0x93CB1A, 0x84CB1A,
                0x72CB1A, 0x61CB1A, 0x52CB1A, 0x40CB1A, 0x31CB1A, 0x1FCB1A, 0x1ACB25, 0x1ACB34,
                0x1ACB46, 0x1ACB58, 0x1ACB67, 0x1ACB78, 0x1ACB87, 0x1ACB99, 0x1ACBAB, 0x1ACBB9,
                0x1ACBCB, 0x1AB9CB, 0x1AABCB, 0x1A99CB, 0x1A87CB, 0x1A78CB, 0x1A67CB, 0x1A58CB,
                0x1A46CB, 0x1A34CB, 0x1A25CB, 0x1F1ACB, 0x311ACB, 0x401ACB, 0x521ACB, 0x611ACB
            };
            Colors = Array.ConvertAll(hexColors, ColorHelper.FromHex);
            Animals = new[] {
                "alligator", "anteater", "armadillo", "auroch", "badger", "bat", "beaver", "buffalo",
                "camel", "capybara", "chameleon", "cheetah", "chinchilla", "chipmunk", "chupacabra", "cormorant",
                "coyote", "crow", "dingo", "dolphin", "duck", "elephant", "ferret", "fox",
                "giraffe", "gopher", "grizzly", "hippo", "hyena", "ibex", "iguana", "jackal",
                "kangaroo", "koala", "kraken", "lemur", "leopard", "liger", "llama", "manatee",
                "mink", "monkey", "moose", "narwhal", "orangutan", "otter", "panda", "penguin",
                "platypus", "python", "quagga", "rabbit", "raccoon", "rhino", "sheep", "shrew",
                "skunk", "squirrel", "tiger", "turtle", "walrus", "wolf", "wolverine", "wombat"           
            };
        }

        public AnimalIdenticonProvider() => Images = new Lazy<Task<Image>[]>(DownloadImages);
        #endregion

        #region Functions
        private Task<Image>[] DownloadImages()
        {
            string cachePath = Options.CachePath;
            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);

            return Animals.Select(x => DownloadImage(string.Format(BaseUrl, x), cachePath, x)).ToArray();
        }

        private static async Task<Image> DownloadImage(string url, string cache, string name)
        {
            string filename = Path.Combine(cache, $"{name}.png");

            if (!File.Exists(filename))
#if NETFRAMEWORK
                using (WebClient client = new WebClient())
                    await client.DownloadFileTaskAsync(url, filename);
#else
                using (HttpClient client = new HttpClient())
                    File.WriteAllBytes(filename, await client.GetByteArrayAsync(url));
#endif

            return ImageHelper.Load(filename);
        }

        protected override Image CreateFromBytes(byte[] input) => Process(input, (imageIndex, background) => 
        {
            int size = Options.Size;
            Image image = new Bitmap(size, size);

            image.Mutate(context => {
                context.Fill(background);
                context.DrawImage(Images.Value[imageIndex].Result, new Rectangle(0, 0, size, size));
            });

            return image;
        });

        protected override SvgBuilder CreateSvgFromBytes(byte[] input) => Process(input, (imageIndex, background) => 
        {
            SvgBuilder svg = new SvgBuilder { Color = background };

            string link = Options.SvgImageAsLink 
                ? string.Format(BaseUrl, Animals[imageIndex]) 
                : Images.Value[imageIndex].Result.ToBase64Link();

            return svg.Append(new SvgImage(link) { PercentageHeight = 100, PercentageWidth = 100 });
        });

        private T Process<T>(byte[] input, Func<int, Color, T> processor)
        {
            byte[] hash = Options.HashAlgorithm.ComputeHash(input);
            byte[] bytes = hash.Reverse().Loop().Take(2).ToArray();
            int number = ((bytes[1] & 0x0F) << 8) + bytes[0];

            int imageIndex = number % Animals.Length;
            Color background = Colors[number / Colors.Length % Colors.Length];

            return processor(imageIndex, background);
        }
#endregion
    }
}

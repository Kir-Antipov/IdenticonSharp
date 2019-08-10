#if NETFRAMEWORK
using System;
using System.Drawing;

namespace KE.IdenticonSharp.Compatibility
{
    internal static class GraphicsHelper
    {
        public static void Mutate(this Image image, Action<GraphicsWrapper> mutator)
        {
            using (Graphics g = Graphics.FromImage(image))
                mutator(new GraphicsWrapper(image, g));
        }
    }
}
#endif

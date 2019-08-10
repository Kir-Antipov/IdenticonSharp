using System.Collections.Generic;

namespace IdenticonSharp.Helpers
{
    public static class EnumerableHelper
    {
        public static IEnumerable<T> Loop<T>(this IEnumerable<T> enumerable)
        {
            while (true)
                foreach (T value in enumerable)
                    yield return value;
        }

        public static IEnumerable<T> Reverse<T>(this IList<T> list)
        {
            int count = list.Count;
            for (int i = count - 1; i >= 0; --i)
                yield return list[i];
        }
    }
}

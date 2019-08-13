using System;
using System.Linq;
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

        public static IEnumerable<IEnumerable<T>> Bucket<T>(this IEnumerable<T> source, int size)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException(nameof(size));

            IEnumerator<T> enumerator = source.GetEnumerator();
            bool enumerating = false;

            do
            {
                List<T> currentBucket = new List<T>(size);
                for (int i = 0; i < size && (enumerating = enumerator.MoveNext()); ++i)
                    currentBucket.Add(enumerator.Current);
                if (currentBucket.Count != 0)
                    yield return currentBucket;
            }
            while (enumerating);
        }

        public static IEnumerable<T> PadLeft<T>(this IEnumerable<T> source, int totalWidth, T value)
        {
            if (totalWidth < 1)
                throw new ArgumentOutOfRangeException(nameof(totalWidth));

            IList<T> list = source is IList<T> conv ? conv : source.ToList();

            int count = list.Count;
            if (count < totalWidth)
            {
                T[] result = new T[totalWidth];

                int padding = totalWidth - count;
                for (int i = 0; i < padding; ++i)
                    result[i] = value;

                for (int i = padding, j = 0; i < totalWidth; ++i, ++j)
                    result[i] = list[j];

                list = result;
            }

            return list;
        }
        public static IEnumerable<T> PadLeft<T>(this IEnumerable<T> source, int totalWidth) => PadLeft(source, totalWidth, default);


        public static IEnumerable<T> PadRight<T>(this IEnumerable<T> source, int totalWidth) => PadRight(source, totalWidth, default);
        public static IEnumerable<T> PadRight<T>(this IEnumerable<T> source, int totalWidth, T value)
        {
            if (totalWidth < 1)
                throw new ArgumentOutOfRangeException(nameof(totalWidth));

            int count = 0;
            foreach (T current in source)
            {
                ++count;
                yield return current;
            }

            int padding = totalWidth - count;
            for (int i = 0; i < padding; ++i)
                yield return value;
        }

        public static IEnumerable<T> TakeFromEnd<T>(this IEnumerable<T> source, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            IList<T> list = source is IList<T> conv ? conv : source.ToList();

            for (int i = Math.Max(list.Count - count, 0); i < list.Count; ++i)
                yield return list[i];
        }

        public static IEnumerable<T> Straighten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            List<IEnumerator<T>> enumerators = source.Select(x => x.GetEnumerator()).ToList();
            bool working;
            do
            {
                working = false;
                for (int i = 0; i < enumerators.Count; ++i)
                {
                    IEnumerator<T> current = enumerators[i];
                    bool hasValue = current.MoveNext();
                    working |= hasValue;
                    if (hasValue)
                        yield return current.Current;
                }
            } while (working);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> GetRandom<T>(this IEnumerable<T> source, int count) { return source.Shuffle().Take(count); }

        public static List<List<T>> Shuffle<T>(this IEnumerable<T> source, int numShuffles)
        {
            var results = new List<List<T>>();
            for (int i = 0; i < numShuffles; i++)
            {
                results.Add(source.Shuffle().ToList());
            }
            return results;
        }
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) { return source.OrderBy(x => Guid.NewGuid()); }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var knownKeys = new HashSet<TKey>();
            return source.Where(element => knownKeys.Add(keySelector(element)));
        }

        public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }

        public static IEnumerable<T> TakeAllButLast<T>(this IEnumerable<T> source)
        {
            var it = source.GetEnumerator();
            bool hasRemainingItems;
            var isFirst = true;
            var item = default(T);

            do
            {
                hasRemainingItems = it.MoveNext();
                if (hasRemainingItems)
                {
                    if (!isFirst) yield return item;
                    item = it.Current;
                    isFirst = false;
                }
            } while (hasRemainingItems);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities.Extensions
{
    public static class ListExtensions
    {
        public static List<int> Combine(this List<int> a, List<int> b)
        {
            return a.Select((x, i) => x + b[i]).ToList();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static int IndexOfNextNumber(this List<string> parts, int startIndex)
        {
            for (var i = startIndex; i < parts.Count; i++)
            {
                if (parts[i].IsDouble()) return i;
            }
            return -1;
        }
    }
}
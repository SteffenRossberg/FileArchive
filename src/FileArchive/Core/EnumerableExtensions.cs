using System;
using System.Collections.Generic;

namespace FileArchive.Core
{
    public static class EnumerableExtensions
    {
        public static void ForEach<TItem>(this IEnumerable<TItem> source, Action<TItem> action)
            => source.ForEach((item, _) => action(item));

        public static void ForEach<TItem>(this IEnumerable<TItem> source, Action<TItem, int> action)
        {
            var index = -1;
            if (source is IReadOnlyList<TItem> list)
            {
                for (index = 0; index < list.Count; ++index)
                {
                    action(list[index], index);
                }
            }
            else
            {
                foreach (var item in source)
                {
                    action(item, ++index);
                }
            }
        }
    }
}

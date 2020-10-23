using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerStarter.Shared
{
    public static class ListExtensions
    {
        public static bool EqualsByIndex<T, TKey>(this IList<T> list, IList<T> other, Func<T, TKey> orderBy)
        {
            if (list.Count != other.Count)
                return false;

            return EqualsByIndex(list.OrderBy(orderBy).ToList(), other.OrderBy(orderBy).ToList());
        }

        public static bool EqualsByIndex<T>(this IList<T> list, IList<T> other)
        {
            if (list.Count != other.Count)
                return false;

            for (int i = 0; i < list.Count; i++)
            {
                bool serverEquals = list[i].Equals(other[i]);
                if (!serverEquals)
                    return false;
            }

            return true;
        }

        public static bool EqualsBySelector<T, TKey>(this IList<T> list, IList<T> other, Func<T, TKey> selector)
        {
            if (list.Count != other.Count)
                return false;

            var dictionary = other.ToDictionary(selector);
            foreach (T item in list)
            {
                var key = selector(item);
                if (!dictionary.ContainsKey(key))
                    return false;

                var itemEquals = item.Equals(dictionary[key]);
                if (!itemEquals)
                    return false;
            }

            return true;
        }
    }
}
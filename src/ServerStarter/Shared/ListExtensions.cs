using System.Collections.Generic;

namespace ServerStarter.Shared
{
    public static class ListExtensions
    {
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
    }
}
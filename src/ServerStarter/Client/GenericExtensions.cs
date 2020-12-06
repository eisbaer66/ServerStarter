using System.Collections.Generic;

namespace ServerStarter.Client
{
    public static class GenericExtensions
    {
        public static T AddTo<T>(this T item, ICollection<T> list)
        {
            list.Add(item);
            return item;
        }
    }
}
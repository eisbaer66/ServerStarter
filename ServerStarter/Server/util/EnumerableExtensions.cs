using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerStarter.Server.util
{
    public static class EnumerableExtensions
    {
        public static async Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            var array = tasks.ToArray();
            await Task.WhenAll(array);

            return array.Select(t => t.Result);
        }
        public static async Task<IList<T>> WhenAllList<T>(this IEnumerable<Task<T>> tasks)
        {
            IEnumerable<T> items = await WhenAll(tasks);
            return items.ToList();
        }
    }
}
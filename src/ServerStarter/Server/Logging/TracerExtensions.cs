using System;
using System.Threading.Tasks;
using Elastic.Apm.Api;

namespace ServerStarter.Server.Logging
{
    public static class TracerExtensions
    {
        public static T Capture<T>(this ITracer tracer, string name, string type, Func<T> func)
        {
            var transaction = tracer.CurrentTransaction;
            if (transaction == null)
            {
                return tracer.CaptureTransaction(name, type,  func);
            }

            return transaction.CaptureSpan(name, type, func);
        }
        public static async Task<T> CaptureAsync<T>(this ITracer tracer, string name, string type, Func<Task<T>> func)
        {
            var transaction = tracer.CurrentTransaction;
            if (transaction == null)
            {
                return await tracer.CaptureTransaction(name, type, func);
            }

            return await transaction.CaptureSpan(name, type, func);
        }
    }
}
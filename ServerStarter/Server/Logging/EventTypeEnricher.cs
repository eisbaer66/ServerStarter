using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Murmur;
using Serilog.Core;
using Serilog.Events;

namespace ServerStarter.Server.Logging
{
    internal class EventTypeEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent is null)
                throw new ArgumentNullException(nameof(logEvent));

            if (propertyFactory is null)
                throw new ArgumentNullException(nameof(propertyFactory));

            Murmur32         murmur          = MurmurHash.Create32();
            byte[]           bytes           = Encoding.UTF8.GetBytes(logEvent.MessageTemplate.Text);
            byte[]           hash            = murmur.ComputeHash(bytes);
            string           hexadecimalHash = BitConverter.ToString(hash).Replace("-", "");
            LogEventProperty eventId         = propertyFactory.CreateProperty("EventType", hexadecimalHash);
            logEvent.AddPropertyIfAbsent(eventId);
        }
    }
}

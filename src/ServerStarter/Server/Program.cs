using System;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using ServerStarter.Server.Logging;

namespace ServerStarter.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((ctx, config) =>
                {
                                config
                                    .ReadFrom.Configuration(ctx.Configuration)
                                    .Enrich.With<EventTypeEnricher>()
                                    .Destructure.With<JsonDocumentDestructuringPolicy>()
                                    .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName);

                                var settings = new ElasticSettings();
                                ctx.Configuration.Bind("ServerStarters:Elastic", settings);
                                if (settings.AreSet())
                                    config.WriteTo.Elasticsearch(new
                                                             ElasticsearchSinkOptions(new Uri(settings.Url))
                                                             {
                                                                 CustomFormatter = new EcsTextFormatterMetadata(),
                                                                 ModifyConnectionSettings =
                                                                     c => c.BasicAuthentication(settings.Username, settings.Password),
                                                                 IndexFormat = settings.IndexFormat,
                                                             });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }

    public class JsonDocumentDestructuringPolicy : IDestructuringPolicy
    {
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            if (!(value is JsonDocument jdoc))
            {
                result = null;
                return false;
            }

            result = Destructure(jdoc.RootElement);
            return true;
        }

        static LogEventPropertyValue Destructure(in JsonElement jel)
        {
            switch (jel.ValueKind)
            {
                case JsonValueKind.Array:
                    return new SequenceValue(jel.EnumerateArray().Select(ae => Destructure(in ae)));

                case JsonValueKind.False:
                    return new ScalarValue(false);

                case JsonValueKind.True:
                    return new ScalarValue(true);

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return new ScalarValue(null);

                case JsonValueKind.Number:
                    return new ScalarValue(jel.GetDecimal());

                case JsonValueKind.String:
                    return new ScalarValue(jel.GetString());

                case JsonValueKind.Object:
                    return new StructureValue(jel.EnumerateObject().Select(jp => new LogEventProperty(jp.Name, Destructure(jp.Value))));

                default:
                    throw new ArgumentException("Unrecognized value kind " + jel.ValueKind + ".");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Elastic.Apm.SerilogEnricher;
using Elastic.CommonSchema.Serilog;
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
                                    .WriteTo.Elasticsearch(new
                                                             ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                                                             {
                                                                 CustomFormatter = new EcsTextFormatter(),
                                                                 ModifyConnectionSettings =
                                                                     c => c.BasicAuthentication("ServerStarter",
                                                                                                ctx.Configuration["ServerStarters:ElasticPassword"])
                                                             });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

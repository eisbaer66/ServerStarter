using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Elastic.CommonSchema.Serilog;
using Microsoft.Extensions.Configuration;
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
                                    .Enrich.With<EventTypeEnricher>();

                                var settings = new ElasticSettings();
                                ctx.Configuration.Bind("ServerStarters:Elastic", settings);
                                if (settings.AreSet())
                                    config.WriteTo.Elasticsearch(new
                                                             ElasticsearchSinkOptions(new Uri(settings.Url))
                                                             {
                                                                 CustomFormatter = new EcsTextFormatter(),
                                                                 ModifyConnectionSettings =
                                                                     c => c.BasicAuthentication(settings.Username, settings.Password)
                                                             });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

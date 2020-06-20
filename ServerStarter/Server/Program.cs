using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
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

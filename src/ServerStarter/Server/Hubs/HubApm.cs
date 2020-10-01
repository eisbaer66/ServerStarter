using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace ServerStarter.Server.Hubs
{
    //SignalR currently has no easy way to implement cross-cutting behaviour. looking forward for (https://docs.microsoft.com/en-us/aspnet/core/signalr/hub-filters?view=aspnetcore-5.0)
    //apm-agent-dotnet's methods to fill transaction.Request from HttpRequest are internal, so i copied their default implementation minus the body-capture
    //both of these are jank and hopefully get cleaned up in the future
    public interface IHubApm<T> where T: Hub
    {
        Task Trace(string name, Func<Task> action);
    }

    public class HubApm<T> : IHubApm<T> where T : Hub
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<T>           _logger;

        public HubApm(IHttpContextAccessor accessor, ILogger<T> logger)
        {
            _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Trace(string name, Func<Task> action)
        {
            await Agent.Tracer.CaptureTransaction("SignalR " + name, ApiConstants.TypeRequest,
                                                  async t =>
                                                  {
                                                      t.Custom["icebear.HubApm"] = "true";
                                                      FillSampledTransactionContextRequest(_accessor.HttpContext, t, _logger);
                                                      await action();
                                                  });
        }

        private static void FillSampledTransactionContextRequest(HttpContext context, ITransaction transaction, ILogger<T> logger)
        {
            try
            {
                if (context?.Request == null) return;

                var url = new Url
                          {
                              Full     = context.Request.GetEncodedUrl(),
                              HostName = context.Request.Host.Host,
                              Protocol = GetProtocolName(context.Request.Protocol),
                              Raw      = GetRawUrl(context.Request, logger) ?? context.Request.GetEncodedUrl(),
                              PathName = context.Request.Path,
                              Search   = context.Request.QueryString.Value.Length > 0 ? context.Request.QueryString.Value.Substring(1) : string.Empty
                          };

                transaction.Context.Request = new Request(context.Request.Method, url)
                                              {
                                                  Socket      = new Socket { Encrypted = context.Request.IsHttps, RemoteAddress = context.Connection?.RemoteIpAddress?.ToString() },
                                                  HttpVersion = GetHttpVersion(context.Request.Protocol),
                                                  Headers     = GetHeaders(context.Request.Headers, Agent.Config)
                                              };
                
                //transaction.CollectRequestBody(false, context.Request, logger, transaction.ConfigSnapshot);
            }
            catch (Exception ex)
            {
                // context.request is optional: https://github.com/elastic/apm-server/blob/64a4ab96ba138050fe496b17d31deb2cf8830deb/docs/spec/request.json#L5
                logger?.LogError(ex, "Exception thrown while trying to fill request context for sampled transaction {TransactionId}", transaction.Id);
            }
        }

        private static Dictionary<string, string> GetHeaders(IHeaderDictionary headers, IConfigurationReader configSnapshot) =>
            configSnapshot.CaptureHeaders && headers != null
                ? headers.ToDictionary(header => header.Key, header => header.Value.ToString())
                : null;

        private static string GetRawUrl(HttpRequest httpRequest, ILogger<T> logger)
        {
            try
            {
                var rawPathAndQuery = httpRequest.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

                if (!string.IsNullOrEmpty(rawPathAndQuery) && rawPathAndQuery.Count() > 0 && rawPathAndQuery[0] != '/')
                    return rawPathAndQuery;

                return rawPathAndQuery == null ? null : UriHelper.BuildAbsolute(httpRequest.Scheme, httpRequest.Host, rawPathAndQuery);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed reading RawUrl");
                return null;
            }
        }

        private static string GetProtocolName(string protocol)
        {
            switch (protocol)
            {
                case { } s when string.IsNullOrEmpty(s):
                    return string.Empty;
                case { } s when s.StartsWith("HTTP", StringComparison.InvariantCulture): //in case of HTTP/2.x we only need HTTP
                    return "HTTP";
                default:
                    return protocol;
            }
        }

        private static string GetHttpVersion(string protocolString)
        {
            switch (protocolString)
            {
                case "HTTP/1.0":
                    return "1.0";
                case "HTTP/1.1":
                    return "1.1";
                case "HTTP/2.0":
                    return "2.0";
                case null:
                    return "unknown";
                default:
                    return protocolString.Replace("HTTP/", string.Empty);
            }
        }

    }
}
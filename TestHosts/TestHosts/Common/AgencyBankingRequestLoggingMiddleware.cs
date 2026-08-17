using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Shared.Logger;

namespace TestHosts.Common;

public static class AgencyBankingRequestLoggingMiddleware
{
    public static IApplicationBuilder UseAgencyBankingRequestLogging(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            if (!context.Request.Path.StartsWithSegments("/api/agencybanking", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            string method = context.Request.Method;
            string path = context.Request.Path.Value ?? string.Empty;
            string traceId = context.TraceIdentifier;

            Logger.LogInformation($"Agency Banking request started. {method} {path} TraceId={traceId}");

            try
            {
                await next();

                stopwatch.Stop();

                if (context.Response.StatusCode >= StatusCodes.Status500InternalServerError)
                {
                    Logger.LogWarning(
                        $"Agency Banking request failed after {stopwatch.ElapsedMilliseconds} ms. {method} {path} -> {context.Response.StatusCode}");
                }
                else if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
                {
                    Logger.LogWarning(
                        $"Agency Banking request completed with client error after {stopwatch.ElapsedMilliseconds} ms. {method} {path} -> {context.Response.StatusCode}");
                }
                else
                {
                    Logger.LogInformation(
                        $"Agency Banking request completed after {stopwatch.ElapsedMilliseconds} ms. {method} {path} -> {context.Response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Logger.LogError($"Agency Banking request threw after {stopwatch.ElapsedMilliseconds} ms. {method} {path} TraceId={traceId}", ex);
                throw;
            }
        });
    }
}

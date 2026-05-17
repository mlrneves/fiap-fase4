using Datadog.Trace;
using Infrastructure.CrossCutting.Correlation;
using Microsoft.Extensions.Primitives;
using Serilog.Context;
using System.Diagnostics;

namespace CatalogAPI.Infra.Middleware
{
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "x-correlation-id";

        public CorrelationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ICorrelationIdGenerator correlationIdGenerator)
        {
            var correlationId = GetOrCreateCorrelationId(context, correlationIdGenerator).ToString();

            context.Items["X-Correlation-Id"] = correlationId;

            var span = Tracer.Instance.ActiveScope?.Span;
            span?.SetTag("correlation_id", correlationId);

            using (LogContext.PushProperty("correlation_id", correlationId))
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                AddCorrelationIdHeaderToResponse(context, correlationId);
                await _next(context);
            }
        }

        private static StringValues GetOrCreateCorrelationId(
            HttpContext context,
            ICorrelationIdGenerator correlationIdGenerator)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId)
                && !StringValues.IsNullOrEmpty(correlationId))
            {
                return correlationId;
            }

            return new StringValues(correlationIdGenerator.Get());
        }

        private static void AddCorrelationIdHeaderToResponse(HttpContext context, string correlationId)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });
        }
    }

    public static class CorrelationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationMiddleware>();
        }
    }
}
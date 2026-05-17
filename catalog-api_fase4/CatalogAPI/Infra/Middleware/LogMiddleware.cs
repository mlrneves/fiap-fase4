using System.Diagnostics;
using System.Text;

namespace CatalogAPI.Infra.Middleware
{
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogMiddleware> _logger;

        public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.Items["X-Correlation-Id"]?.ToString();

            var request = context.Request;
            var originalBodyStream = context.Response.Body;

            await using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            _logger.LogInformation(
                "HTTP Request started {Method} {Path} CorrelationId={CorrelationId} status={status}",
                request.Method,
                request.Path,
                correlationId,
                "info");

            try
            {
                await _next(context);

                stopwatch.Stop();

                var statusCode = context.Response.StatusCode;

                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBody, Encoding.UTF8).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                var truncatedResponseBody = Truncate(responseText, 2000);

                if (statusCode >= 500)
                {
                    _logger.LogError(
                        "HTTP Request finished with server error {Method} {Path} StatusCode={StatusCode} ElapsedMs={Elapsed} CorrelationId={CorrelationId} ResponseBody={ResponseBody} status={status}",
                        request.Method,
                        request.Path,
                        statusCode,
                        stopwatch.ElapsedMilliseconds,
                        correlationId,
                        truncatedResponseBody,
                        "error");
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "HTTP Request finished with client error {Method} {Path} StatusCode={StatusCode} ElapsedMs={Elapsed} CorrelationId={CorrelationId} ResponseBody={ResponseBody} status={status}",
                        request.Method,
                        request.Path,
                        statusCode,
                        stopwatch.ElapsedMilliseconds,
                        correlationId,
                        truncatedResponseBody,
                        "warn");
                }
                else
                {
                    _logger.LogInformation(
                        "HTTP Request finished {Method} {Path} StatusCode={StatusCode} ElapsedMs={Elapsed} CorrelationId={CorrelationId} status={status}",
                        request.Method,
                        request.Path,
                        statusCode,
                        stopwatch.ElapsedMilliseconds,
                        correlationId,
                        "info");
                }

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                context.Response.Body = originalBodyStream;

                _logger.LogError(
                    ex,
                    "HTTP Request exception {Method} {Path} ElapsedMs={Elapsed} CorrelationId={CorrelationId} status={status}",
                    request.Method,
                    request.Path,
                    stopwatch.ElapsedMilliseconds,
                    correlationId,
                    "error");

                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private static string? Truncate(string? text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return text.Length <= maxLength
                ? text
                : text[..maxLength];
        }
    }

    public static class LogMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogMiddleware>();
        }
    }
}
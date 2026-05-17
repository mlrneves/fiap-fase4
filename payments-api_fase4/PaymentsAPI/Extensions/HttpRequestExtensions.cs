using Microsoft.AspNetCore.Http;

namespace PaymentsAPI.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string? GetInternalApiKey(this HttpRequest request)
        {
            if (request.Headers.TryGetValue("x-internal-api-key", out var values))
                return values.FirstOrDefault();

            return null;
        }
    }
}
using Microsoft.AspNetCore.Http;

namespace Infrastructure.CrossCutting.Correlation
{
    public class CorrelationIdGenerator : ICorrelationIdGenerator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdGenerator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Get()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return Guid.NewGuid().ToString();

            if (context.Items.TryGetValue("X-Correlation-Id", out var correlationId))
                return correlationId?.ToString() ?? Guid.NewGuid().ToString();

            return Guid.NewGuid().ToString();
        }
    }
}
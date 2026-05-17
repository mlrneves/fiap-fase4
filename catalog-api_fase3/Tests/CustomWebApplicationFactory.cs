using Core.Entity;
using Core.Repository;
using Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Substituir Redis por cache em memória
            var cacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDistributedCache));
            if (cacheDescriptor is not null) services.Remove(cacheDescriptor);
            services.AddDistributedMemoryCache();

            // Substituir Elasticsearch por implementação no-op
            var searchDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ISearchService));
            if (searchDescriptor is not null) services.Remove(searchDescriptor);
            services.AddScoped<ISearchService, NullSearchService>();

            // Substituir DynamoDB por repositório no-op
            var auditLogDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAuditLogRepository));
            if (auditLogDescriptor is not null) services.Remove(auditLogDescriptor);
            services.AddScoped<IAuditLogRepository, NullAuditLogRepository>();
        });
    }
}

file sealed class NullSearchService : ISearchService
{
    public Task IndexGameAsync(Game game) => Task.CompletedTask;
    public Task RemoveGameAsync(int gameId) => Task.CompletedTask;
    public Task<IList<GameDto>> SearchAsync(string query) => Task.FromResult<IList<GameDto>>(new List<GameDto>());
}

file sealed class NullAuditLogRepository : IAuditLogRepository
{
    public Task AddAsync(AuditLog log) => Task.CompletedTask;
    public Task<IList<AuditLog>> GetByEntityTypeAsync(string entityType, int limit = 50)
        => Task.FromResult<IList<AuditLog>>(new List<AuditLog>());
}

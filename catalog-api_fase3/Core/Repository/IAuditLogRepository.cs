using Core.Entity;

namespace Core.Repository
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog log);
        Task<IList<AuditLog>> GetByEntityTypeAsync(string entityType, int limit = 50);
    }
}

using Core.Entity;
using Infrastructure.CrossCutting.Correlation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Infrastructure.Repository
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ICorrelationIdGenerator? _correlationIdGenerator;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICorrelationIdGenerator? correlationIdGenerator = null)
            : base(options)
        {
            _correlationIdGenerator = correlationIdGenerator;
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<UserLibrary> UserLibraries { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            AddAuditLogs();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddAuditLogs();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void AddAuditLogs()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e =>
                    e.Entity is IAuditableEntity &&
                    e.Entity is not AuditLog &&
                    e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            if (!entries.Any())
                return;

            var correlationId = _correlationIdGenerator?.Get();

            foreach (var entry in entries)
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    EntityName = entry.Metadata.ClrType.Name,
                    EntityId = entry.State == EntityState.Added
                        ? string.Empty
                        : (GetPrimaryKey(entry) ?? string.Empty),
                    Action = GetAction(entry.State),
                    OldValues = entry.State == EntityState.Added
                        ? null
                        : SerializePropertyValues(entry.OriginalValues, false),
                    NewValues = entry.State == EntityState.Deleted
                        ? null
                        : SerializePropertyValues(entry.CurrentValues, entry.State == EntityState.Added),
                    CreatedAtUtc = DateTime.UtcNow,
                    CorrelationId = correlationId
                };

                AuditLogs.Add(auditLog);
            }
        }

        private static string? GetPrimaryKey(EntityEntry entry)
        {
            var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            return key?.CurrentValue?.ToString();
        }

        private static string GetAction(EntityState state)
        {
            return state switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => state.ToString()
            };
        }

        private static string? SerializePropertyValues(PropertyValues values, bool ignoreTemporaryId)
        {
            var dict = new Dictionary<string, object?>();

            foreach (var property in values.Properties)
            {
                var value = values[property];

                if (ignoreTemporaryId &&
                    property.IsPrimaryKey() &&
                    value is int intValue &&
                    intValue < 0)
                {
                    continue;
                }

                dict[property.Name] = value;
            }

            return dict.Count == 0 ? null : JsonSerializer.Serialize(dict);
        }
    }
}
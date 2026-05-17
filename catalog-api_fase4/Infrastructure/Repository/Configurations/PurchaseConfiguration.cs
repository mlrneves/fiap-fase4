using Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Repository.Configurations
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.ToTable("Purchases");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).UseIdentityColumn();
            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.UpdatedAt);
            builder.Property(p => p.UserId).IsRequired();
            builder.Property(p => p.GameId).IsRequired();
            builder.Property(p => p.PurchaseDate).IsRequired();
            builder.Property(p => p.PricePaid).IsRequired().HasColumnType("decimal(10,2)");

            builder.HasOne(p => p.Game)
                   .WithMany(g => g.Purchases)
                   .HasForeignKey(p => p.GameId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(p => new { p.UserId, p.GameId }).IsUnique();
        }
    }
}

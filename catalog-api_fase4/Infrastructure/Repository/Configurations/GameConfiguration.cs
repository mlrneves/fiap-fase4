using Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Repository.Configurations
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.ToTable("Games");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).UseIdentityColumn();
            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.UpdatedAt);
            builder.Property(p => p.Title).IsRequired().HasMaxLength(150);
            builder.Property(p => p.Description).HasMaxLength(1000);

            builder.Property(p => p.Price).IsRequired().HasColumnType("decimal(10,2)");
            builder.Property(p => p.Genre).HasMaxLength(50);
            builder.Property(p => p.Developer).HasMaxLength(100);
            builder.Property(p => p.ReleaseDate);

            builder.HasMany(g => g.Promotions)
                .WithOne(p => p.Game)
                .HasForeignKey(p => p.GameId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

}

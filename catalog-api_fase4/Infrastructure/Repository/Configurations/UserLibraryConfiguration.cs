using Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Repository.Configurations
{
    public class UserLibraryConfiguration : IEntityTypeConfiguration<UserLibrary>
    {
        public void Configure(EntityTypeBuilder<UserLibrary> builder)
        {
            builder.ToTable("UserLibraries");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.GameId).IsRequired();
            builder.Property(x => x.PurchaseId).IsRequired();
            builder.Property(x => x.AddedAt).IsRequired();

            builder.HasOne(x => x.Game)
                .WithMany()
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(x => new { x.UserId, x.GameId })
                .IsUnique(false);
        }
    }
}
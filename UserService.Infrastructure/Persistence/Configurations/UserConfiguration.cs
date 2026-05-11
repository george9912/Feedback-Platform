using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(u => u.Role)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(u => u.AzureAdObjectId)
                   .HasMaxLength(100);

            builder.Property(u => u.UserPrincipalName)
                   .HasMaxLength(200);

            builder.Property(u => u.DisplayName)
                   .HasMaxLength(200);

            builder.Property(u => u.CreatedAt)
                   .IsRequired();

            builder.Property(u => u.UpdatedAt)
                   .IsRequired();

            builder.HasIndex(u => u.AzureAdObjectId)
                   .IsUnique()
                   .HasFilter("[AzureAdObjectId] IS NOT NULL");

            builder.HasIndex(u => u.Email);

            builder.HasIndex(u => u.UserPrincipalName);
        }
    }
}
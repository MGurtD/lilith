using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Auth
{
    public class ApiKeyBuilder : IEntityTypeConfiguration<ApiKey>
    {
        public void Configure(EntityTypeBuilder<ApiKey> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder
                .Property(b => b.Description)
                .HasMaxLength(500);

            builder
                .Property(b => b.KeyPrefix)
                .IsRequired()
                .HasMaxLength(32);

            builder
                .Property(b => b.KeyHash)
                .IsRequired()
                .HasMaxLength(255);

            builder
                .Property(b => b.Scopes)
                .HasMaxLength(1000);

            builder
                .Property(b => b.ExpiresOn)
                .HasColumnType("timestamp without time zone");

            builder
                .Property(b => b.LastUsedOn)
                .HasColumnType("timestamp without time zone");

            builder
                .HasIndex(b => b.KeyPrefix)
                .IsUnique()
                .HasDatabaseName("UK_ApiKey_KeyPrefix");

            builder
                .HasIndex(b => b.Disabled)
                .HasDatabaseName("IX_ApiKey_Disabled");

            builder
                .HasIndex(b => b.ExpiresOn)
                .HasDatabaseName("IX_ApiKey_ExpiresOn");

            builder.ToTable("ApiKeys");
        }
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Shared;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class ParameterBuilder : IEntityTypeConfiguration<Parameter>
    {
        public const string TABLE_NAME = "Parameters";

        public void Configure(EntityTypeBuilder<Parameter> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Key)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Value)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(2000);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(b => new { b.Key }, $"UK_{TABLE_NAME}")
                .IsUnique();

            builder.ToTable(TABLE_NAME);
        }

    }
}

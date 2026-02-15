using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class TaxBuilder : IEntityTypeConfiguration<Tax>
    {
        public const string TABLE_NAME = "Taxes";

        public void Configure(EntityTypeBuilder<Tax> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Percentatge)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION, 
                              ApplicationDbContextConstants.DECIMAL_SCALE);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(builder => builder.Name, $"UK_{TABLE_NAME}")
                .IsUnique();

            builder.ToTable(TABLE_NAME);
        }

    }
}

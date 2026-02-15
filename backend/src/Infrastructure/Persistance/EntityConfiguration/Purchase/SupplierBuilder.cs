using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class SupplierBuilder : IEntityTypeConfiguration<Supplier>
    {
        public const string TABLE_NAME = "Suppliers";

        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.ComercialName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);
            builder
                .Property(b => b.TaxName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);
            builder
                .Property(b => b.VatNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);
            builder
                .Property(b => b.Country)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Region)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.City)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.PostalCode)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);
            builder
                .Property(b => b.Address)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);
            builder
                .Property(b => b.Phone)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);
            builder
                .Property(b => b.AccountNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(35);
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(builder => builder.ComercialName, $"UK_{TABLE_NAME}_ComercialName")
                .IsUnique();

            builder.ToTable(TABLE_NAME);
        }

    }
}

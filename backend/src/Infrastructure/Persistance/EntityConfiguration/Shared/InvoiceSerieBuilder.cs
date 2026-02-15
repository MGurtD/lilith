using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Shared;

public class InvoiceSerieBuilder : IEntityTypeConfiguration<InvoiceSerie>
{
    public const string TABLE_NAME = "PurchaseInvoiceSeries";

    public void Configure(EntityTypeBuilder<InvoiceSerie> builder)
    {
        builder.ConfigureBase();

        builder
            .Property(b => b.Name)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(250);
        builder
            .Property(b => b.Description)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(250);
        builder
            .Property(b => b.Prefix)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(50);
        builder
            .Property(b => b.Suffix)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(50);
        builder
            .Property(b => b.NextNumber)
            .HasDefaultValue(1)
            .IsRequired();
        builder
            .Property(b => b.Length)
            .HasDefaultValue(1)
            .IsRequired();

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder
            .HasIndex(builder => builder.Name, $"UK_{TABLE_NAME}_Name")
            .IsUnique();

        builder.ToTable(TABLE_NAME);
    }

}
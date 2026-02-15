using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Sales;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class CustomerBuilder : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Code)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);

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
                .Property(b => b.Web)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(150);

            builder
                .Property(b => b.AccountNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(35);

            builder
                .Property(b => b.Observations)
                .IsRequired()
                .HasColumnType("varchar")
                .HasDefaultValue(4000);
            builder
                .Property(b => b.InvoiceNotes)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(4000);

            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .Property(b => b.PreferredLanguage)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10)
                .HasDefaultValue("ca");

            builder
                .HasKey(b => b.Id)
                .HasName("PK_Customer");

            builder
                .HasIndex(builder => builder.ComercialName, "UK_Customer_Name")
                .IsUnique();

            builder
                .HasIndex(builder => builder.VatNumber, "UK_Customer_VatNumber")
                .IsUnique();

            builder.ToTable("Customers");
        }
    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Sales;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class SalesInvoiceDueDateBuilder : IEntityTypeConfiguration<SalesInvoiceDueDate>
    {
        public const string TABLE_NAME = "SalesInvoiceDueDates";

        public void Configure(EntityTypeBuilder<SalesInvoiceDueDate> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.DueDate)
                .IsRequired();
            builder
                .Property(b => b.Amount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE); ;

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder.ToTable(TABLE_NAME);
        }

    }
}

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class PurchaseInvoiceDueDateBuilder : IEntityTypeConfiguration<PurchaseInvoiceDueDate>
    {
        public const string TABLE_NAME = "PurchaseInvoiceDueDates";

        public void Configure(EntityTypeBuilder<PurchaseInvoiceDueDate> builder)
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

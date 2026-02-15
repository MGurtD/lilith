using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class SalesInvoiceVerifactuRequestBuilder : IEntityTypeConfiguration<SalesInvoiceVerifactuRequest>
    {
        public void Configure(EntityTypeBuilder<SalesInvoiceVerifactuRequest> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.SalesInvoiceId)
                .IsRequired()
                .HasColumnType("uuid")
                .HasMaxLength(512);
            builder
                .Property(b => b.Request)
                .IsRequired()
                .HasColumnType("text");
            builder
                .Property(b => b.Hash)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.Response)
                .IsRequired()
                .HasColumnType("text");
            builder
                .Property(b => b.TimestampResponse)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.Success)
                .IsRequired()
                .HasColumnType("boolean");
            builder.
                Property(b => b.Status)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);            
            builder
                .Property(b => b.QrCodeUrl)
                .IsRequired(false)
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.QrCodeBase64)
                .IsRequired(false)
                .HasColumnType("text");

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{nameof(SalesInvoiceVerifactuRequest)}");

            builder
                .HasIndex(b => b.SalesInvoiceId)
                .HasDatabaseName($"IX_{nameof(SalesInvoiceVerifactuRequest)}_{nameof(SalesInvoiceVerifactuRequest.SalesInvoiceId)}");
        }
    }
}

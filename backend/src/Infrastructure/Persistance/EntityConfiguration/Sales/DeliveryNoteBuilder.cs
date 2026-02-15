using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class DeliveryNoteBuilder : IEntityTypeConfiguration<DeliveryNote>
    {
        public const string TABLE_NAME = "DeliveryNotes";

        public void Configure(EntityTypeBuilder<DeliveryNote> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.DeliveryDate)
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.Number)
                .IsRequired()
                .HasDefaultValue("0")
                .HasColumnType("varchar")
                .HasMaxLength(50);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(builder => new { builder.Number }, $"UK_{TABLE_NAME}")
                .IsUnique();

            builder.ToTable(TABLE_NAME);
        }
    }
}

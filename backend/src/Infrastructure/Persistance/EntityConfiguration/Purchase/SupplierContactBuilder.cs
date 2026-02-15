using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class SupplierContactBuilder : IEntityTypeConfiguration<SupplierContact>
    {
        public const string TABLE_NAME = "SupplierContacts";

        public void Configure(EntityTypeBuilder<SupplierContact> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.FirstName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.LastName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Charge)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Email)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Phone)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);
            builder
                .Property(b => b.PhoneExtension)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(5);
            builder
                .Property(b => b.Observations)
                .IsRequired()
                .HasColumnType("text");
            builder
                .Property(b => b.Default)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder.ToTable(TABLE_NAME);
        }

    }
}

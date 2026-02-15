using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Sales;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class CustomerTypeBuilder : IEntityTypeConfiguration<CustomerType>
    {
        public void Configure(EntityTypeBuilder<CustomerType> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_CustomerType");

            builder
                .HasIndex(builder => builder.Name, "UK_CustomerType_Name")
                .IsUnique();

            builder.ToTable("CustomerTypes");

        }
    }
}

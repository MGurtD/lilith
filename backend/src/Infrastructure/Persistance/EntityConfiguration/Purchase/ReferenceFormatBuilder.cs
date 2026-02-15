using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class ReferenceFormatBuilder : IEntityTypeConfiguration<ReferenceFormat>
    {
        public const string TABLE_NAME = "ReferenceFormats";
        public void Configure(EntityTypeBuilder<ReferenceFormat> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Code)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.Description)
                .HasColumnType("varchar")
                .HasMaxLength(250);
            
            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");
            builder
                .HasIndex(b => b.Code)
                .HasDatabaseName($"IX_{TABLE_NAME}_Code")
                .IsUnique();

            builder.ToTable($"{TABLE_NAME}");

        }
    }
}

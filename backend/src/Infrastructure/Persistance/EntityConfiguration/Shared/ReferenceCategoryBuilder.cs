using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Shared;

namespace Infrastructure.Persistance.EntityConfiguration.Warehouse
{
    public class ReferenceCategoryBuilder : IEntityTypeConfiguration<ReferenceCategory>
    {
        public const string TABLE_NAME = "ReferenceCategories";

        public void Configure(EntityTypeBuilder<ReferenceCategory> builder)
        {
            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            
            builder
                .HasKey(b => b.Name)
                .HasName($"PK_{TABLE_NAME}");

            builder.ToTable(TABLE_NAME);
        }
    }
}
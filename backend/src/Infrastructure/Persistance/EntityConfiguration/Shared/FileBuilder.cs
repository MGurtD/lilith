using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class FileBuilder : IEntityTypeConfiguration<Domain.Entities.File>
    {
        public const string TABLE_NAME = "Files";

        public void Configure(EntityTypeBuilder<Domain.Entities.File> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Entity)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(100);
            builder
                .Property(b => b.EntityId)
                .ValueGeneratedNever()
                .HasColumnType("uuid");
            builder
                .Property(b => b.Type);
            builder
                .Property(b => b.Size)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION, 
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.OriginalName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(1000);
            builder
                .Property(b => b.Path)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(4000);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(builder => new { builder.EntityId, builder.Entity }, $"UK_{TABLE_NAME}_Entity");

            builder.ToTable(TABLE_NAME);
        }

    }
}

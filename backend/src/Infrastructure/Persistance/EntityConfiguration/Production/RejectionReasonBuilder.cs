using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;

public class RejectionReasonBuilder : IEntityTypeConfiguration<RejectionReason>
{
    public const string TABLE_NAME = "RejectionReasons";

    public void Configure(EntityTypeBuilder<RejectionReason> builder)
    {
        builder.ConfigureBase();

        builder
            .Property(b => b.Code)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(20);

        builder
            .Property(b => b.Name)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(100);

        builder
            .Property(b => b.Description)
            .HasColumnType("text");

        builder
            .Property(b => b.Color)
            .HasColumnType("varchar")
            .HasMaxLength(20);

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder
            .HasIndex(b => b.Code)
            .IsUnique()
            .HasDatabaseName("UK_RejectionReason_Code");

        builder.ToTable(TABLE_NAME);
    }
}

using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class PhaseTemplateDetailBuilder : IEntityTypeConfiguration<PhaseTemplateDetail>
{
    public const string TABLE_NAME = "PhaseTemplateDetail";
    public void Configure(EntityTypeBuilder<PhaseTemplateDetail> builder)
    {
        builder.ConfigureBase();

        builder
            .Property(b => b.Order)
            .HasColumnType("integer");
        builder
            .Property(b => b.Comment)
            .IsRequired()
            .HasColumnType("text");

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder.ToTable(TABLE_NAME);
    }
}

using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class PhaseTemplateBuilder : IEntityTypeConfiguration<PhaseTemplate>
{
    public const string TABLE_NAME = "PhaseTemplate";
    public void Configure(EntityTypeBuilder<PhaseTemplate> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.Name)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(200);
        builder
            .Property(b => b.Description)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(500);

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder.ToTable(TABLE_NAME);
    }
}

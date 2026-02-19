using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class WorkcenterBuilder : IEntityTypeConfiguration<Workcenter>
    {
        public void Configure(EntityTypeBuilder<Workcenter> builder)
        {
            builder.ConfigureBase();
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
                .Property(b => b.costHour)
                .HasColumnType("decimal");
            builder
               .Property(b => b.ProfitPercentage)
               .IsRequired()
               .HasColumnType("decimal")
               .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                             ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.MultiWoAvailable)
                .HasDefaultValue(false)
                .HasColumnType("boolean");
            builder
                .HasKey(b => b.Id)
                .HasName("PK_Workcenter");
            builder.HasIndex(builder => builder.Name, "UK_Workcenter_Name");

            builder.ToTable("Workcenters");
        }
    }
}
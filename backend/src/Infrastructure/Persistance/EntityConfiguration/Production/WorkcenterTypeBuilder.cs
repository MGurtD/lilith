using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class WorkcenterTypeBuilder : IEntityTypeConfiguration<WorkcenterType>
    {
        public void Configure(EntityTypeBuilder<WorkcenterType> builder)
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
               .Property(b => b.ProfitPercentage)
               .IsRequired()
               .HasColumnType("decimal")
               .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                             ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_WorkcenterType");
            builder.HasIndex(builder => builder.Name, "UK_WorkcenterType_Name");

            builder.ToTable("WorkcenterTypes");
        }
    }
}

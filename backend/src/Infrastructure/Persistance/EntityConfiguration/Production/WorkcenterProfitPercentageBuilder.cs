using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.EntityConfiguration.Production;

public class WorkcenterProfitPercentageBuilder : IEntityTypeConfiguration<WorkcenterProfitPercentage>
{
    public void Configure(EntityTypeBuilder<WorkcenterProfitPercentage> builder)
    {
        builder.ConfigureBase();
        
        builder
            .Property(b => b.ProfitPercentage)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                          ApplicationDbContextConstants.DECIMAL_SCALE);
        
        builder
            .HasKey(b => b.Id)
            .HasName("PK_WorkcenterProfitPercentage");

        builder.ToTable("WorkcenterProfitPercentages");
    }
}
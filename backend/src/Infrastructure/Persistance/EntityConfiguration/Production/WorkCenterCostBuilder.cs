using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class WorkCenterCostBuilder : IEntityTypeConfiguration<WorkcenterCost>
    {
        public void Configure(EntityTypeBuilder<WorkcenterCost> builder)
        {
            builder.ConfigureBase();
            builder
               .Property(b => b.Cost)
               .IsRequired()
               .HasColumnType("decimal")
               .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                             ApplicationDbContextConstants.DECIMAL_SCALE);

            builder
               .HasKey(b => b.Id)
               .HasName("PK_WorkcenterCost");
            builder
                .HasIndex(builder => new { builder.WorkcenterId, builder.MachineStatusId }, $"UK_WorkcenterCosts_Workcenter_MachineStatus")
                .IsUnique();

            builder.ToTable("WorkCenterCosts");
        }
    }
}

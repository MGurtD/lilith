using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class ShiftDetailBuilder : IEntityTypeConfiguration<ShiftDetail>
    {
        public void Configure(EntityTypeBuilder<ShiftDetail> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.StartTime)
                .IsRequired()
                .HasColumnType("time");
            builder
                .Property(b => b.EndTime)
                .IsRequired()
                .HasColumnType("time");
            builder
                .Property(b => b.IsProductiveTime)
                .HasColumnType("bool");

            builder.ToTable("ShiftDetails");
        }
    }
}



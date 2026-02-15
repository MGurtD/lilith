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
    public class OperatorTypeBuilder : IEntityTypeConfiguration<OperatorType>
    {
        public void Configure(EntityTypeBuilder<OperatorType> builder)
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
               .Property(b => b.Cost)
               .IsRequired()
               .HasColumnType("decimal")
               .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                             ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_OperatorType");
            builder.HasIndex(builder => builder.Name, "UK_OperatorType_Name");

            builder.ToTable("OperatorTypes");
        }
    }
}

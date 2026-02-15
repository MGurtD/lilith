using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class OperatorBuilder : IEntityTypeConfiguration<Operator>
    {
        public void Configure(EntityTypeBuilder<Operator> builder) 
        {
            builder.ConfigureBase();
            builder
                 .Property(b => b.Code)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);
            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(25);
            builder
                .Property(b => b.Surname)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder.Property(b => b.VatNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(25);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_Operator");
            builder.HasIndex(builder => builder.Code, "UK_Operator_Code");
            builder.HasIndex(builder => builder.VatNumber, "UK_Operator_VatNumber");

            builder.ToTable("Operators");
        }
    }
}

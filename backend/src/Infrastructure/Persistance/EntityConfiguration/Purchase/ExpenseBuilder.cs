using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class ExpenseBuilder : IEntityTypeConfiguration<Expenses>
    {
        public const string TABLE_NAME = "Expenses";
        public void Configure(EntityTypeBuilder<Expenses> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.CreationDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.PaymentDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.EndDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.Amount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.Recurring)
                .HasColumnType("bool");
            builder
                .Property(b => b.Frecuency)
                .IsRequired()
                .HasColumnType("integer");
            builder
                .Property(b => b.PaymentDay)
                .IsRequired()
                .HasColumnType("integer");
            builder
                .Property(b => b.RelatedExpenseId)
                .HasColumnType("varchar");
            builder
                   .HasKey(b => b.Id)
                   .HasName($"PK_{TABLE_NAME}");



            builder.ToTable("Expenses");

        }
    }
}

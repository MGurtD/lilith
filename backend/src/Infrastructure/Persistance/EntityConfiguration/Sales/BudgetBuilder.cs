using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class BudgetBuilder : IEntityTypeConfiguration<Budget>
    {
        public const string TABLE_NAME = "Budgets";

        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Number)
                .IsRequired()
                .HasDefaultValue("0")
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.Date)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.AcceptanceDate)
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.AutoRejectedDate)
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.Amount)                
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.Notes)
                .HasColumnType("varchar")
                .HasMaxLength(4000);

            builder
                .Property(b => b.UserNotes)
                .HasColumnType("varchar")
                .HasMaxLength(4000);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(b => b.Date)
                .IsDescending()
                .HasDatabaseName($"IX_{TABLE_NAME}_{nameof(Budget.Date)}");
            

            builder.ToTable(TABLE_NAME);
        }
    }
}

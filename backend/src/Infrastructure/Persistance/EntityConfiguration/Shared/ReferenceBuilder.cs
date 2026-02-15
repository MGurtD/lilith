using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class ReferenceBuilder : IEntityTypeConfiguration<Reference>
    {
        public void Configure(EntityTypeBuilder<Reference> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Code)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);
            builder
                .Property(b => b.Cost)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Price)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.TransportAmount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.PRICE_DECIMAL_SCALE);
            builder
                .Property(b => b.Version)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);
            
            builder
                .Property(b => b.LastCost)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);

            builder
                .Property(b => b.WorkMasterCost)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);

            builder
                .Property(b => b.Sales)
                .IsRequired()
                .HasColumnType("boolean");

            builder
                .Property(b => b.Purchase)
                .IsRequired()
                .HasColumnType("boolean");

            builder
                .Property(b => b.Production)
                .IsRequired()
                .HasColumnType("boolean");
            builder
                .Property(b => b.IsService)
                .IsRequired()
                .HasColumnType("boolean");

            builder
                   .HasOne(rc => rc.ReferenceType)
                   .WithMany(rc => rc.References)
                   .HasForeignKey(r => r.ReferenceTypeId)
                   .HasPrincipalKey(rc => rc.Id);

            builder
                   .HasOne(r => r.Category)
                   .WithMany(rc => rc.References)
                   .HasForeignKey(r => r.CategoryName)
                   .HasPrincipalKey(rc => rc.Name);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_Reference");
            
            builder.HasIndex(builder => new { builder.Code, builder.Version }, "UK_Reference_Code_Version");

            builder.ToTable("References");
        }
    }
}

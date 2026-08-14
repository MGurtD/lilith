using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class EnterpriseBuilder : IEntityTypeConfiguration<Enterprise>
    {
        public void Configure(EntityTypeBuilder<Enterprise> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.BrandName)
                .HasColumnType("varchar")
                .HasMaxLength(60);
            builder
                .Property(b => b.PrimaryColor)
                .HasColumnType("varchar")
                .HasMaxLength(7);

            // Default site (optional)
            builder
                .Property(b => b.DefaultSiteId)
                .IsRequired(false);

            builder
                .HasOne(b => b.DefaultSite)
                .WithMany()
                .HasForeignKey(b => b.DefaultSiteId)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .HasOne<Domain.Entities.File>()
                .WithMany()
                .HasForeignKey(b => b.LogoMainFileId)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .HasOne<Domain.Entities.File>()
                .WithMany()
                .HasForeignKey(b => b.LogoSidebarFileId)
                .OnDelete(DeleteBehavior.SetNull);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_Enterprise");

            builder.HasIndex(builder => builder.Name, "UK_Enterprise_Name");
            builder
                .HasIndex(builder => builder.Disabled, "UK_Enterprise_SingleEnabled")
                .IsUnique()
                .HasFilter("\"Disabled\" = false");

            builder.ToTable("Enterprises");
        }

    }
}

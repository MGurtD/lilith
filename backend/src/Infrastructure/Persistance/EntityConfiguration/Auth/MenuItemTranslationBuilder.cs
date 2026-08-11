using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Auth
{
    public class MenuItemTranslationBuilder : IEntityTypeConfiguration<MenuItemTranslation>
    {
        public void Configure(EntityTypeBuilder<MenuItemTranslation> builder)
        {
            builder.ConfigureBase();

            builder.Property(b => b.LanguageCode)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);

            builder.Property(b => b.Title)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder.HasOne(b => b.MenuItem)
                .WithMany(m => m.Translations)
                .HasForeignKey(b => b.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => new { b.MenuItemId, b.LanguageCode }, "UK_MenuItemTranslation_MenuItem_Language")
                .IsUnique();

            builder.HasKey(b => b.Id).HasName("PK_MenuItemTranslation");
            builder.ToTable("MenuItemTranslations");
        }
    }
}

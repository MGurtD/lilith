using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Auth;

namespace Infrastructure.Persistance.EntityConfiguration.Auth
{
    public class UserTableViewBuilder : IEntityTypeConfiguration<UserTableView>
    {
        public void Configure(EntityTypeBuilder<UserTableView> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Page)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .Property(b => b.IsDefault)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .Property(b => b.ViewConfig)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(8000);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_UserTableView");

            // Unique composite index: UserId + Page + Name
            builder.HasIndex(builder => new { builder.UserId, builder.Page, builder.Name }, "UK_UserTableView_UserId_Page_Name")
                .IsUnique();

            // Index for finding default view: UserId + Page + IsDefault
            builder.HasIndex(builder => new { builder.UserId, builder.Page, builder.IsDefault }, "IX_UserTableView_UserId_Page_IsDefault");

            // Foreign key to User (restrict delete - don't allow deleting user with views)
            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("UserTableViews");
        }
    }
}
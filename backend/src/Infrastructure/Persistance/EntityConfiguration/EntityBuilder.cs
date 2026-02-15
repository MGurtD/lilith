using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public static class EntityBaseConfiguration
    {
        public static void ConfigureBase<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : Entity
        {
            builder
                .Property(b => b.Id)
                .ValueGeneratedNever()
                .HasColumnType("uuid");
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
            builder
                .Property(e => e.CreatedOn)
                .HasColumnType("timestamp without time zone")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NOW()");
            builder
                .Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("NOW()");
        }

        /// <summary>
        /// Configures base entity properties excluding timestamp fields (CreatedOn/UpdatedOn).
        /// Use this method for entities that explicitly ignore timestamp tracking.
        /// </summary>
        public static void ConfigureBaseWithoutTimestamps<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : Entity
        {
            builder
                .Property(b => b.Id)
                .ValueGeneratedNever()
                .HasColumnType("uuid");
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
            
            // Explicitly ignore timestamp properties
            builder.Ignore(e => e.CreatedOn);
            builder.Ignore(e => e.UpdatedOn);
        }
    }
}

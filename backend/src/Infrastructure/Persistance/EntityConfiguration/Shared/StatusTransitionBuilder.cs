using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class StatusTransitionBuilder : IEntityTypeConfiguration<StatusTransition>
    {
        public const string TABLE_NAME = "StatusTransitions";

        public void Configure(EntityTypeBuilder<StatusTransition> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(builder => builder.Name, $"UK_{TABLE_NAME}_Name")
                .IsUnique();

            //builder
            //    .HasOne(bp => bp.Status)
            //    .WithMany()
            //    .HasForeignKey(bp => bp.StatusId)
            //    .IsRequired();

            //builder
            //    .HasOne(bp => bp.StatusTo)
            //    .WithMany()
            //    .HasForeignKey(bp => bp.StatusToId)
            //    .IsRequired();

            builder.ToTable(TABLE_NAME);
        }

    }
}

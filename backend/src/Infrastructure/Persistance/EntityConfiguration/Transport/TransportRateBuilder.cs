using Domain.Entities.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Transport
{
    public class TransportRateBuilder : IEntityTypeConfiguration<TransportRate>
    {
        public void Configure(EntityTypeBuilder<TransportRate> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .Property(b => b.SupplierId)
                .IsRequired();

            builder
                .Property(b => b.SupplierId)
                .IsRequired();

            builder
                .Property(b => b.ValidFrom)
                .IsRequired()
                .HasColumnType("date");

            builder
                .Property(b => b.ValidTo)
                .IsRequired()
                .HasColumnType("date");

            builder
                .HasMany(tr => tr.Details)
                .WithOne(trd => trd.TransportRate)
                .HasForeignKey(trd => trd.TransportRateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_TransportRate");

            builder
                .HasIndex(builder => new { builder.Name, builder.ValidFrom, builder.ValidTo }, "UK_TransportRate_Name_Dates")
                .IsUnique();

            builder.ToTable("TransportRate");
        }
    }

    public class TransportRateDetailBuilder : IEntityTypeConfiguration<TransportRateDetail>
    {
        public void Configure(EntityTypeBuilder<TransportRateDetail> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.TransportRateId)
                .IsRequired();

            builder
                .Property(b => b.MinWeight)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0);

            builder
                .Property(b => b.MaxWeight)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0);

            builder
                .Property(b => b.MinVolume)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0);

            builder
                .Property(b => b.MaxVolume)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0);

            builder
                .Property(b => b.MinDistance)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0);

            builder
                .Property(b => b.MaxDistance)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0);

            builder
                .Property(b => b.Price)
                .IsRequired()
                .HasColumnType("decimal(18,4)")
                .HasDefaultValue(0);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_TransportRateDetail");

            builder.ToTable("TransportRateDetail");
        }
    }
}

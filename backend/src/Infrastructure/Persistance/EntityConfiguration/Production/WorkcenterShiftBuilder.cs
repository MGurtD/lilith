using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistance.EntityConfiguration.Production;

public class WorkcenterShiftBuilder : IEntityTypeConfiguration<WorkcenterShift>
{
    public const string TableName = "WorkcenterShifts";

    public void Configure(EntityTypeBuilder<WorkcenterShift> builder)
    {
        builder.ConfigureBaseWithoutTimestamps();

        builder
            .Property(b => b.Current)
            .IsRequired();
        builder
            .Property(b => b.StartTime)
            .IsRequired()
            .HasColumnType("timestamp without time zone");
        builder
            .Property(b => b.EndTime)
            .HasColumnType("timestamp without time zone");

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TableName}");
        builder
          .HasIndex(e => new { e.WorkcenterId, e.StartTime })
          .HasDatabaseName($"IX_{TableName}_Unique") // Nombre opcional del índice
          .IsUnique(); // Opcional: índice único
        

        builder.ToTable(TableName, "data");
    }
}
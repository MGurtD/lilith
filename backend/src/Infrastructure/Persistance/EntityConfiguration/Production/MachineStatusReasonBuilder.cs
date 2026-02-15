using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;

public class MachineStatusReasonBuilder : IEntityTypeConfiguration<MachineStatusReason>
{
    public const string TABLE_NAME = "MachineStatusReasons";
    
    public void Configure(EntityTypeBuilder<MachineStatusReason> builder)
    {
        builder.ConfigureBase();
        
        builder
            .Property(b => b.Code)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(20);
            
        builder
            .Property(b => b.Name)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(100);
            
        builder
            .Property(b => b.Description)
            .HasColumnType("text");
            
        builder
            .Property(b => b.Color)
            .HasColumnType("varchar")
            .HasMaxLength(20);
            
        builder
            .Property(b => b.Icon)
            .HasColumnType("varchar")
            .HasMaxLength(50);

        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder
            .HasIndex(b => new { b.Code, b.MachineStatusId })
            .IsUnique()
            .HasDatabaseName("UK_MachineStatusReason_Code_MachineStatusId");

        builder.ToTable(TABLE_NAME);
    }
}

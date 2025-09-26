using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Data.Configurations
{
    public class PrinterConsumableConfiguration : IEntityTypeConfiguration<PrinterConsumable>
    {
        public void Configure(EntityTypeBuilder<PrinterConsumable> builder)
        {
            builder.ToTable("PrinterConsumables");
            
            builder.HasKey(pc => pc.Id);
            
            builder.Property(pc => pc.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(pc => pc.PartNumber)
                .HasMaxLength(50);
                
            builder.Property(pc => pc.Unit)
                .HasMaxLength(20);
                
            // Indexes
            builder.HasIndex(pc => pc.PrinterId);
            builder.HasIndex(pc => pc.Type);
            
            // Configure enum conversion
            builder.Property(pc => pc.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
        }
    }
}

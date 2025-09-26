using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Data.Configurations
{
    public class PrinterConfiguration : IEntityTypeConfiguration<Printer>
    {
        public void Configure(EntityTypeBuilder<Printer> builder)
        {
            builder.ToTable("Printers");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.Model)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.SerialNumber)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.IpAddress)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.Location)
                .HasMaxLength(200);
                
            builder.Property(p => p.Description)
                .HasMaxLength(500);
                
            // Indexes
            builder.HasIndex(p => p.IpAddress)
                .IsUnique();
                
            builder.HasIndex(p => p.SerialNumber)
                .IsUnique();
                
            // Relationships
            builder.HasMany(p => p.Consumables)
                .WithOne(c => c.Printer)
                .HasForeignKey(c => c.PrinterId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(p => p.PrintJobs)
                .WithOne(j => j.Printer)
                .HasForeignKey(j => j.PrinterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

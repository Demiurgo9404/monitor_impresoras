using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Data.Configurations
{
    public class AlertConfiguration : IEntityTypeConfiguration<Alert>
    {
        public void Configure(EntityTypeBuilder<Alert> builder)
        {
            builder.ToTable("Alerts");
            
            builder.HasKey(a => a.Id);
            
            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);
                
            builder.Property(a => a.Message)
                .IsRequired();
                
            builder.Property(a => a.ResolutionNotes)
                .HasMaxLength(1000);
                
            builder.Property(a => a.Source)
                .HasMaxLength(100);
                
            // Indexes
            builder.HasIndex(a => a.PrinterId);
            builder.HasIndex(a => a.Status);
            builder.HasIndex(a => a.Type);
            builder.HasIndex(a => a.CreatedAt);
            
            // Configure enum conversions
            builder.Property(a => a.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
                
            // Configure relationships
            builder.HasOne(a => a.Printer)
                .WithMany()
                .HasForeignKey(a => a.PrinterId)
                .OnDelete(DeleteBehavior.SetNull);
                
            builder.HasOne(a => a.AcknowledgedBy)
                .WithMany()
                .HasForeignKey(a => a.AcknowledgedById)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

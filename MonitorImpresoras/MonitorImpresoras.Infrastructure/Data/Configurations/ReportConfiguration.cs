using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Data.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Reports");
            
            builder.HasKey(r => r.Id);
            
            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(r => r.Description)
                .HasMaxLength(500);
                
            builder.Property(r => r.FilePath)
                .HasMaxLength(500);
                
            builder.Property(r => r.ErrorMessage)
                .HasMaxLength(1000);
                
            builder.Property(r => r.RecurrencePattern)
                .HasMaxLength(50);
                
            // Indexes
            builder.HasIndex(r => r.Type);
            builder.HasIndex(r => r.Status);
            builder.HasIndex(r => r.GeneratedAt);
            builder.HasIndex(r => r.GeneratedBy);
            
            // Configure enum conversions
            builder.Property(r => r.Type)
                .HasConversion<string>()
                .HasMaxLength(50);
                
            builder.Property(r => r.Format)
                .HasConversion<string>()
                .HasMaxLength(10);
                
            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
        }
    }
}

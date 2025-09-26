using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonitorImpresoras.Domain.Entities;

namespace MonitorImpresoras.Infrastructure.Data.Configurations
{
    public class PrintJobConfiguration : IEntityTypeConfiguration<PrintJob>
    {
        public void Configure(EntityTypeBuilder<PrintJob> builder)
        {
            builder.ToTable("PrintJobs");
            
            builder.HasKey(pj => pj.Id);
            
            builder.Property(pj => pj.DocumentName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(pj => pj.JobStatus)
                .HasMaxLength(20);
                
            builder.Property(pj => pj.ErrorMessage)
                .HasMaxLength(500);
                
            builder.Property(pj => pj.Cost)
                .HasColumnType("decimal(18,2)");
                
            // Indexes
            builder.HasIndex(pj => pj.PrinterId);
            builder.HasIndex(pj => pj.UserId);
            builder.HasIndex(pj => pj.PrintedAt);
            
            // Configure relationships
            builder.HasOne(pj => pj.Printer)
                .WithMany(p => p.PrintJobs)
                .HasForeignKey(pj => pj.PrinterId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(pj => pj.User)
                .WithMany()
                .HasForeignKey(pj => pj.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

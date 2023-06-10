using Microsoft.EntityFrameworkCore;

namespace Emailing.Models
{
    public class EmailingDbContext : DbContext
    {
        public EmailingDbContext(DbContextOptions<EmailingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Document>()
                .HasIndex(u => u.DocumentId)
                .IsUnique();

            builder.Entity<Status>().HasData(
                new Status { Id = 1, Name = "UploadedDocument" },
                new Status { Id = 2, Name = "SentEmail" }
                );

            builder.Entity<Document>().HasOne(e => e.Status).WithMany().HasForeignKey(e => e.StatusId).IsRequired();
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<Status> Statuses { get; set; }
    }
}

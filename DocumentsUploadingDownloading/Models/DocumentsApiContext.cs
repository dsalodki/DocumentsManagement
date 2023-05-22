using Microsoft.EntityFrameworkCore;

namespace DocumentsUploadingDownloading.Models
{
    public class DocumentsApiContext : DbContext
    {
        public DocumentsApiContext(DbContextOptions<DocumentsApiContext> options) : base(options)
        {
            
        }

        public DbSet<Document> Documents { get; set; }
    }
}

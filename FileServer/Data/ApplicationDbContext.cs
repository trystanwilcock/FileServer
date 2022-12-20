using FileServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileRecord> FileRecords { get; set; }
        public DbSet<DirectoryRecord> DirectoryRecords { get; set; }
    }
}
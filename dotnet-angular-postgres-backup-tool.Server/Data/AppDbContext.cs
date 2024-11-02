using dotnet_angular_postgres_backup_tool.Server.Models;
using Microsoft.EntityFrameworkCore;
namespace dotnet_angular_postgres_backup_tool.Server.Data
{
    /// <summary>
    /// Entity Framework database context for managing backup operations
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /// <summary>
        /// DbSet representing the backup log entries in the database
        /// </summary>
        public DbSet<BackupLogEntry> BackupLog { get; set; }
    }
}

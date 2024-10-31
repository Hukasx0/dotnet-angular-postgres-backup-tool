using dotnet_angular_postgres_backup_tool.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_angular_postgres_backup_tool.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<BackupLogEntry> BackupLog {  get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace dotnet_angular_postgres_backup_tool.Server.Models
{
    public class BackupLogEntry
    {
        [Key]
        public int Id { get; set; }
        public string DbName { get; set; }
        public DateTime BackupDate { get; set; }
        public string BackupPath { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace dotnet_angular_postgres_backup_tool.Server.Models
{
    public class BackupLogEntry
    {
        public int Id { get; set; }
        public string DatabaseName { get; set; } = null!;
        public DateTime BackupDate { get; set; }
        public string BackupPath { get; set; } = null!;
        public Status Status { get; set; }
        public string? ErrorMessage { get; set; }
        public long? BackupSizeBytes { get; set; }
        public TimeSpan? Duration { get; set; }
    }

    public enum Status
    {
        InProgress,
        Success,
        Failed
    }
}

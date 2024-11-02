using System.ComponentModel.DataAnnotations;
namespace dotnet_angular_postgres_backup_tool.Server.Models
{
    /// <summary>
    /// Represents a single database backup operation entry in the log
    /// </summary>
    public class BackupLogEntry
    {
        /// <summary>
        /// Primary key for the backup log entry
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the database that was backed up
        /// </summary>
        public string DatabaseName { get; set; } = null!;

        /// <summary>
        /// UTC timestamp when the backup was initiated
        /// </summary>
        public DateTime BackupDate { get; set; }

        /// <summary>
        /// Full filesystem path where the backup file is stored
        /// </summary>
        public string BackupPath { get; set; } = null!;

        /// <summary>
        /// Current status of the backup operation
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// Error message if the backup failed, null otherwise
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Size of the backup file in bytes
        /// </summary>
        public long? BackupSizeBytes { get; set; }

        /// <summary>
        /// Total time taken to complete the backup operation
        /// </summary>
        public TimeSpan? Duration { get; set; }
    }

    /// <summary>
    /// Represents the possible states of a backup operation
    /// </summary>
    public enum Status
    {
        InProgress,
        Success,
        Failed
    }
}

using dotnet_angular_postgres_backup_tool.Server.Data;
using dotnet_angular_postgres_backup_tool.Server.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Diagnostics;
using System.Text;

namespace dotnet_angular_postgres_backup_tool.Server.Services
{
    /// <summary>
    /// Service responsible for executing PostgreSQL database backups using pg_dump
    /// Implements Quartz.IJob for scheduled execution
    /// </summary>
    public class BackupService : IJob
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<BackupService> _logger;

        public BackupService(AppDbContext context, IConfiguration config, ILogger<BackupService> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Executes the backup operation as a scheduled job
        /// </summary>
        /// <param name="context">Job execution context provided by Quartz</param>
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting database backup...");
            var startTime = DateTime.UtcNow;

            // Get database name from configuration
            var dbName = _config["BackupSettings:DbName"];
            if (string.IsNullOrWhiteSpace(dbName))
            {
                _logger.LogError("No database name provided");
                return;
            }

            try
            {
                // Determine backup directory path
                var backupDir = _config["BackupSettings:Path"] ??
                    Path.Combine(Environment.CurrentDirectory, "Backups");

                Directory.CreateDirectory(backupDir);

                // Generate unique backup filename using timestamp
                var backupFileName = $"backup_{dbName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mm}.sql";
                var backupPath = Path.Combine(backupDir, backupFileName);

                _logger.LogInformation($"Backup will be saved to: {backupPath}");

                // Create initial backup log entry
                var newDbLogEntry = new BackupLogEntry
                {
                    DatabaseName = dbName,
                    BackupDate = startTime,
                    Status = Status.InProgress,
                    BackupPath = backupPath
                };

                _context.BackupLog.Add(newDbLogEntry);
                await _context.SaveChangesAsync();

                // Execute pg_dump using Process
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        // Configure pg_dump process
                        // Make sure pg_dump is in PATH
                        FileName = "pg_dump",
                        Arguments = $"-h localhost -U postgres -d \"{dbName}\" -f \"{backupPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };

                    // Set PostgreSQL password if provided in configuration
                    if (_config["BackupSettings:PostgresPassword"] != null)
                    {
                        process.StartInfo.EnvironmentVariables["PGPASSWORD"] =
                            _config["BackupSettings:PostgresPassword"];
                    }

                    // Capture process output and errors
                    var output = new StringBuilder();
                    var error = new StringBuilder();

                    process.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            output.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    // Execute backup process
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    await process.WaitForExitAsync();

                    // Check for backup process errors
                    if (process.ExitCode != 0)
                    {
                        _logger.LogError($"pg_dump error: {error}");
                        throw new Exception($"Backup failed with error: {error}");
                    }
                }

                // Update backup log entry with success information
                var backupFile = new FileInfo(backupPath);
                var endTime = DateTime.UtcNow;

                newDbLogEntry.Status = Status.Success;
                newDbLogEntry.BackupSizeBytes = backupFile.Length;
                newDbLogEntry.Duration = endTime - startTime;

                _logger.LogInformation(
                    $"Backup was completed successfully. Backup size: {backupFile.Length / 1024.0 / 1024.0}MB, Duration: {newDbLogEntry.Duration}"
                );
            }
            catch (Exception e)
            {
                // Handle backup failure
                _logger.LogError(e, $"Backup failed for database {dbName}");

                // Update or create failure log entry
                var failedEntry = await _context.BackupLog
                    .OrderByDescending(x => x.BackupDate)
                    .FirstOrDefaultAsync();

                if (failedEntry != null)
                {
                    failedEntry.Status = Status.Failed;
                    failedEntry.ErrorMessage = e.Message;
                }
                else
                {
                    var newFailedEntry = new BackupLogEntry
                    {
                        DatabaseName = dbName,
                        BackupDate = startTime,
                        Status = Status.Failed,
                        ErrorMessage = e.Message,
                        BackupPath = "FAILED_BACKUP"
                    };
                    _context.BackupLog.Add(newFailedEntry);
                }
            }
            finally
            {
                // Ensure changes are saved to database
                await _context.SaveChangesAsync();
            }
        }
    }
}

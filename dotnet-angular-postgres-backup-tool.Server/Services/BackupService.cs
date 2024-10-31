using dotnet_angular_postgres_backup_tool.Server.Data;
using dotnet_angular_postgres_backup_tool.Server.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Diagnostics;
using System.Text;

namespace dotnet_angular_postgres_backup_tool.Server.Services
{
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

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting database backup...");
            var startTime = DateTime.UtcNow;

            var dbName = _config["BackupSettings:DbName"];
            if (string.IsNullOrWhiteSpace(dbName))
            {
                _logger.LogError("No database name provided");
                return;
            }

            try
            {
                var backupDir = _config["BackupSettings:Path"] ??
                    Path.Combine(Environment.CurrentDirectory, "Backups");

                Directory.CreateDirectory(backupDir);

                var backupFileName = $"backup_{dbName}_{DateTime.UtcNow:yyyy-MM-dd_HH-mmss}.sql";
                var backupPath = Path.Combine(backupDir, backupFileName);

                _logger.LogInformation($"Backup will be saved to: {backupPath}");

                var newDbLogEntry = new BackupLogEntry
                {
                    DatabaseName = dbName,
                    BackupDate = startTime,
                    Status = Status.InProgress,
                    BackupPath = backupPath
                };

                _context.BackupLog.Add(newDbLogEntry);
                await _context.SaveChangesAsync();

                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        // Make sure pg_dump is in PATH
                        FileName = "pg_dump",
                        // or use
                        // FileName = @"C:\Program Files\PostgreSQL\16\bin\pg_dump.exe",
                        //
                        Arguments = $"-h localhost -U postgres -d \"{dbName}\" -f \"{backupPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };

                    if (_config["BackupSettings:PostgresPassword"] != null)
                    {
                        process.StartInfo.EnvironmentVariables["PGPASSWORD"] =
                            _config["BackupSettings:PostgresPassword"];
                    }

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

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError($"pg_dump error: {error}");
                        throw new Exception($"Backup failed with error: {error}");
                    }
                }

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
                _logger.LogError(e, $"Backup failed for database {dbName}");

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
                await _context.SaveChangesAsync();
            }
        }
    }
}
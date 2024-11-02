using dotnet_angular_postgres_backup_tool.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace dotnet_angular_postgres_backup_tool.Server.Controllers
{
    /// <summary>
    /// API controller for managing database backup operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BackupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BackupController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all backup entries, ordered by date descending
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBackups()
        {
            var backups = await _context.BackupLog.OrderByDescending(x => x.BackupDate).ToListAsync();
            return Ok(backups);
        }

        /// <summary>
        /// Retrieves the most recent backup entry
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestBackup()
        {
            var latestBackup = await _context.BackupLog.OrderByDescending(x => x.BackupDate).FirstOrDefaultAsync();
            if (latestBackup == null)
            {
                return NotFound();
            }
            return Ok(latestBackup);
        }
    }
}

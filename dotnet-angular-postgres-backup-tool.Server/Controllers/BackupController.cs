using dotnet_angular_postgres_backup_tool.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_angular_postgres_backup_tool.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackupController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BackupController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetBackups()
        {
            var backups = await _context.BackupLog.OrderByDescending(x => x.BackupDate).ToListAsync();
            return Ok(backups);
        }

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

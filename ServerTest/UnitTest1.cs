using dotnet_angular_postgres_backup_tool.Server.Controllers;
using dotnet_angular_postgres_backup_tool.Server;
using dotnet_angular_postgres_backup_tool.Server.Data;
using dotnet_angular_postgres_backup_tool.Server.Models;
using dotnet_angular_postgres_backup_tool.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Quartz;
using System.Net;
using System.Text;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ServerTest
{
    public class BackupServiceTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;
        private readonly Mock<IJobExecutionContext> _jobContextMock;
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly IConfiguration _configuration;

        public BackupServiceTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _jobContextMock = new Mock<IJobExecutionContext>();

            // Setup in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestBackupDb")
                .Options;

            // Setup configuration
            var configValues = new Dictionary<string, string>
            {
                {"BackupSettings:DbName", "test_db"},
                {"BackupSettings:Path", Path.Combine(Path.GetTempPath(), "TestBackups")},
                {"BackupSettings:PostgresPassword", "test_password"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
        }

        [Fact]
        public async Task Execute_SuccessfulBackup_CreatesBackupLogEntry()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            var service = new BackupService(context, _configuration, _loggerMock.Object);

            // Act
            await service.Execute(_jobContextMock.Object);

            // Assert
            var backupEntry = await context.BackupLog.FirstOrDefaultAsync();
            Assert.NotNull(backupEntry);
            Assert.Equal("test_db", backupEntry.DatabaseName);
            Assert.Equal(Status.Success, backupEntry.Status);
            Assert.NotNull(backupEntry.BackupSizeBytes);
            Assert.NotNull(backupEntry.Duration);
        }

        [Fact]
        public async Task Execute_BackupFailure_LogsError()
        {
            // Arrange
            var invalidConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"BackupSettings:DbName", "invalid_db"},
                    {"BackupSettings:Path", "/invalid/path"}
                })
                .Build();

            using var context = new AppDbContext(_dbContextOptions);
            var service = new BackupService(context, invalidConfig, _loggerMock.Object);

            // Act
            await service.Execute(_jobContextMock.Object);

            // Assert
            var backupEntry = await context.BackupLog.FirstOrDefaultAsync();
            Assert.NotNull(backupEntry);
            Assert.Equal(Status.Failed, backupEntry.Status);
            Assert.NotNull(backupEntry.ErrorMessage);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task Execute_MissingDatabaseName_ReturnsEarly()
        {
            // Arrange
            var emptyConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            using var context = new AppDbContext(_dbContextOptions);
            var service = new BackupService(context, emptyConfig, _loggerMock.Object);

            // Act
            await service.Execute(_jobContextMock.Object);

            // Assert
            Assert.Empty(await context.BackupLog.ToListAsync());
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No database name provided")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }
    }

    public class BackupControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public BackupControllerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestBackupControllerDb")
                .Options;
        }

        [Fact]
        public async Task GetBackups_ReturnsAllBackupsOrderedByDate()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            await SeedTestData(context);

            var controller = new BackupController(context);

            // Act
            var result = await controller.GetBackups() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var backups = result.Value as List<BackupLogEntry>;
            Assert.NotNull(backups);
            Assert.Equal(3, backups.Count);
            Assert.True(backups[0].BackupDate > backups[1].BackupDate); // Check ordering
        }

        [Fact]
        public async Task GetLatestBackup_ReturnsLatestBackup()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            await SeedTestData(context);

            var controller = new BackupController(context);

            // Act
            var result = await controller.GetLatestBackup() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var backup = result.Value as BackupLogEntry;
            Assert.NotNull(backup);
            Assert.Equal("test_db_3", backup.DatabaseName);
        }

        [Fact]
        public async Task GetLatestBackup_NoBackups_ReturnsNotFound()
        {
            // Arrange
            using var context = new AppDbContext(_dbContextOptions);
            context.BackupLog.RemoveRange(context.BackupLog);
            await context.SaveChangesAsync();

            var controller = new BackupController(context);

            // Act
            var result = await controller.GetLatestBackup();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        private async Task SeedTestData(AppDbContext context)
        {
            context.BackupLog.RemoveRange(context.BackupLog);
            await context.SaveChangesAsync();

            var testData = new List<BackupLogEntry>
            {
                new BackupLogEntry
                {
                    DatabaseName = "test_db_1",
                    BackupDate = DateTime.UtcNow.AddDays(-2),
                    BackupPath = "/path/to/backup1.sql",
                    Status = Status.Success
                },
                new BackupLogEntry
                {
                    DatabaseName = "test_db_2",
                    BackupDate = DateTime.UtcNow.AddDays(-1),
                    BackupPath = "/path/to/backup2.sql",
                    Status = Status.Success
                },
                new BackupLogEntry
                {
                    DatabaseName = "test_db_3",
                    BackupDate = DateTime.UtcNow,
                    BackupPath = "/path/to/backup3.sql",
                    Status = Status.Success
                }
            };

            await context.BackupLog.AddRangeAsync(testData);
            await context.SaveChangesAsync();
        }
    }

    // Integration tests
    public class BackupIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BackupIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace the real DbContext with in-memory database
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestIntegrationDb");
                    });
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetBackups_ReturnsSuccessStatusCode()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/backup");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json; charset=utf-8",
                response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task GetLatestBackup_NoBackups_ReturnsNotFound()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/backup/latest");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
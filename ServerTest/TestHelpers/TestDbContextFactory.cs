using dotnet_angular_postgres_backup_tool.Server.Data;
using Microsoft.EntityFrameworkCore;
using System;

public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;

        return new AppDbContext(options);
    }
}
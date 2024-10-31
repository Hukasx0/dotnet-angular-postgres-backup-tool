
using dotnet_angular_postgres_backup_tool.Server.Data;
using dotnet_angular_postgres_backup_tool.Server.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.AspNetCore;

namespace dotnet_angular_postgres_backup_tool.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DatabaseConnection"))
                .EnableSensitiveDataLogging()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            );

            builder.Services.AddQuartz(q =>
            {
                var jobKey = new JobKey("BackupJob");
                q.AddJob<BackupService>(o => o.WithIdentity(jobKey));
                q.AddTrigger(o =>
                o.ForJob(jobKey).WithIdentity("BackupJobTrigger")
                  // Every 3 hours
                 .WithCronSchedule("0 0 */3 * * ?")
                );
            });

            builder.Services.AddQuartzHostedService();


            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}

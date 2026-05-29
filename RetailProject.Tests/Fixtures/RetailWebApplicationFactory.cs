using Application.Interfaces.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Configurations.Logging;
using Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;

namespace RetailProject.Tests.Fixtures;

public class RetailWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public RetailWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var dbContextDescriptors = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(RetailDbContext) ||
                    descriptor.ServiceType == typeof(
                        DbContextOptions<RetailDbContext>) ||
                    descriptor.ServiceType == typeof(DbContextOptions) ||
                    (descriptor.ServiceType.IsGenericType &&
                     descriptor.ServiceType.GetGenericTypeDefinition() ==
                     typeof(IDbContextOptionsConfiguration<>) &&
                     descriptor.ServiceType.GenericTypeArguments[0] ==
                     typeof(RetailDbContext)))
                .ToList();
            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<RetailDbContext, TestRetailDbContext>(
                (sp, options) =>
                {
                    options.UseSqlite(_connection);
                    options.AddInterceptors(
                        sp.GetRequiredService<
                            AuditSaveChangesInterceptor>());
                });

            // Dummy Gemini settings
            services.RemoveAll<IOptions<GeminiSettings>>();
            services.Configure<GeminiSettings>(s =>
            {
                s.ApiKey = "test-key";
                s.Model = "test-model";
            });

            // Mock AI service
            services.RemoveAll<IAiSummaryService>();
            var mockAi = new Mock<IAiSummaryService>();
            mockAi
                .Setup(x => x.GenerateSummaryAsync(
                    It.IsAny<Domain.Entities.Logging.ChangeLog>()))
                .ReturnsAsync("Mocked AI summary.");
            services.AddScoped<IAiSummaryService>(
                _ => mockAi.Object);

            // Remove background services
            services.RemoveAll<
                Microsoft.Extensions.Hosting.IHostedService>();
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _connection.Dispose();

        base.Dispose(disposing);
    }
}

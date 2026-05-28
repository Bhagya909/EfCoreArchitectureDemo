using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RetailProject.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public RetailWebApplicationFactory Factory { get; }
    public HttpClient Client { get; private set; } = null!;

    public DatabaseFixture()
    {
        Factory = new RetailWebApplicationFactory();
    }

    public async ValueTask InitializeAsync()
    {
        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        await context.Database.EnsureDeletedAsync(
            TestContext.Current.CancellationToken);
        await context.Database.EnsureCreatedAsync(
            TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        await context.Database.EnsureDeletedAsync(
            TestContext.Current.CancellationToken);

        await Factory.DisposeAsync();
    }

    public IServiceScope CreateScope()
        => Factory.Services.CreateScope();
}

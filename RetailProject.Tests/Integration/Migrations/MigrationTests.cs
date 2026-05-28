using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailProject.Tests.Fixtures;
using System.Data.Common;

namespace RetailProject.Tests.Integration.Migrations;

public class MigrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private static readonly CancellationToken Ct
        = TestContext.Current.CancellationToken;

    public MigrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AllMigrations_ApplyCleanly_ToFreshDatabase()
    {
        var databaseName = $"RetailProject_MigrationTest_{Guid.NewGuid():N}";
        var connectionString =
            $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<RetailDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var context = new RetailDbContext(options);

        try
        {
            await context.Database.EnsureDeletedAsync(Ct);
            await context.Database.MigrateAsync(Ct);

            var appliedMigrations = await context.Database
                .GetAppliedMigrationsAsync(Ct);
            var pendingMigrations = await context.Database
                .GetPendingMigrationsAsync(Ct);

            Assert.NotEmpty(appliedMigrations);
            Assert.Empty(pendingMigrations);
            Assert.Contains(
                appliedMigrations,
                migration => migration.Contains("AddPaymentReferenceNumber"));
            Assert.Contains(
                appliedMigrations,
                migration => migration.Contains("AddAiSummaryErrorColumn"));

            var connection = context.Database.GetDbConnection();
            await context.Database.OpenConnectionAsync(Ct);

            Assert.Equal(1,
                await ExecuteScalarIntAsync(
                    connection,
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Payments' AND COLUMN_NAME = 'ReferenceNumber'",
                    Ct));

            Assert.Equal(1,
                await ExecuteScalarIntAsync(
                    connection,
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChangeLogs' AND COLUMN_NAME = 'AiSummaryError'",
                    Ct));
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
            await context.Database.EnsureDeletedAsync(Ct);
        }
    }

    [Fact]
    public async Task Products_SoftDeleteFilter_ExcludesDeletedRecords()
    {
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        var product = new Domain.Entities.Catalog.Product(
            "Deleted Product", "SKU-DEL-MIG-001", 10.00m);
        product.SoftDelete();
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);

        var products = await context.Products
            .ToListAsync(Ct);
        Assert.DoesNotContain(
            products, p => p.SKU == "SKU-DEL-MIG-001");

        var allProducts = await context.Products
            .IgnoreQueryFilters()
            .ToListAsync(Ct);
        Assert.Contains(
            allProducts, p => p.SKU == "SKU-DEL-MIG-001");
    }

    [Fact]
    public async Task Customers_SoftDeleteFilter_ExcludesDeletedRecords()
    {
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        var customer = new Domain.Entities.Orders.Customer(
            "Deleted Customer", "deleted-mig@test.com");
        customer.SoftDelete();
        context.Customers.Add(customer);
        await context.SaveChangesAsync(Ct);

        var customers = await context.Customers
            .ToListAsync(Ct);
        Assert.DoesNotContain(
            customers, c => c.Email == "deleted-mig@test.com");

        var allCustomers = await context.Customers
            .IgnoreQueryFilters()
            .ToListAsync(Ct);
        Assert.Contains(
            allCustomers, c => c.Email == "deleted-mig@test.com");
    }

    [Fact]
    public async Task Database_AllDbSets_AreQueryable()
    {
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        var ct = TestContext.Current.CancellationToken;

        var productCount = await context.Products.CountAsync(ct);
        var categoryCount = await context.Categories.CountAsync(ct);
        var customerCount = await context.Customers.CountAsync(ct);
        var orderCount = await context.Orders.CountAsync(ct);
        var paymentCount = await context.Payments.CountAsync(ct);
        var inventoryCount = await context.Inventories.CountAsync(ct);
        var changeLogCount = await context.ChangeLogs.CountAsync(ct);

        Assert.True(productCount >= 0);
        Assert.True(categoryCount >= 0);
        Assert.True(customerCount >= 0);
        Assert.True(orderCount >= 0);
        Assert.True(paymentCount >= 0);
        Assert.True(inventoryCount >= 0);
        Assert.True(changeLogCount >= 0);
    }

    private static async Task<int> ExecuteScalarIntAsync(
        DbConnection connection,
        string sql,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var result = await command.ExecuteScalarAsync(ct);

        return Convert.ToInt32(result);
    }
}

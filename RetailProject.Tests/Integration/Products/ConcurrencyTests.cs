using Domain.Entities.Catalog;
using Domain.Entities.Inventory;
using Domain.Exceptions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailProject.Tests.Fixtures;
using InventoryEntity = Domain.Entities.Inventory.Inventory;

namespace RetailProject.Tests.Integration.Products;

public class ConcurrencyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ConcurrencyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Inventory_WhenUpdatedFromStaleContext_ThrowsConcurrencyConflict()
    {
        var ct = TestContext.Current.CancellationToken;

        int inventoryId;

        using (var setupScope = _fixture.CreateScope())
        {
            var setupContext = setupScope.ServiceProvider
                .GetRequiredService<RetailDbContext>();

            var product = new Product(
                "Concurrency Product",
                $"SKU-CONC-{Guid.NewGuid():N}",
                10.00m);

            setupContext.Products.Add(product);
            await setupContext.SaveChangesAsync(ct);

            var inventory = new InventoryEntity(product.Id, 10);
            setupContext.Inventories.Add(inventory);
            await setupContext.SaveChangesAsync(ct);

            inventoryId = inventory.Id;
        }

        using var firstScope = _fixture.CreateScope();
        using var secondScope = _fixture.CreateScope();

        var firstContext = firstScope.ServiceProvider
            .GetRequiredService<RetailDbContext>();
        var secondContext = secondScope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        var secondUnitOfWork = secondScope.ServiceProvider
            .GetRequiredService<Application.Interfaces.IUnitOfWork>();

        var firstInventory = await firstContext.Inventories
            .FirstOrDefaultAsync(i => i.Id == inventoryId, ct);

        var secondInventory = await secondContext.Inventories
            .FirstOrDefaultAsync(i => i.Id == inventoryId, ct);

        Assert.NotNull(firstInventory);
        Assert.NotNull(secondInventory);

        firstInventory.RemoveStock(1);
        firstContext.Entry(firstInventory)
            .Property(nameof(InventoryEntity.RowVersion))
            .CurrentValue = Guid.NewGuid().ToByteArray();
        await firstContext.SaveChangesAsync(ct);

        secondInventory.RemoveStock(1);
        secondContext.Entry(secondInventory)
            .Property(nameof(InventoryEntity.RowVersion))
            .CurrentValue = Guid.NewGuid().ToByteArray();

        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => secondUnitOfWork.SaveChangesAsync());
    }
}

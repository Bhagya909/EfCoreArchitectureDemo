using Application.DTOs.Inventory;
using Application.DTOs.Orders;
using Application.DTOs.Payments;
using Application.DTOs.Products;
using RetailProject.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RetailProject.Tests.Integration.Inventory;

public class InventoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _client;

    private static string UniqueSku(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}";

    public InventoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    private async Task<int> CreateProductAsync(
        string sku, CancellationToken ct)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = $"Inventory Product {sku}",
                SKU = sku,
                BasePrice = 10.00m
            }, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var product = await response.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        return product!.Id;
    }

    [Fact]
    public async Task AddInventory_ThenReadBack_QuantityMatches()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await CreateProductAsync(
            UniqueSku("SKU-INV-001"), ct);

        var addResponse = await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto
            {
                ProductId = productId,
                Quantity = 50
            }, ct);

        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);

        var inventory = await addResponse.Content
            .ReadFromJsonAsync<InventoryResponseDto>(ct);

        Assert.NotNull(inventory);
        Assert.Equal(productId, inventory.ProductId);
        Assert.Equal(50, inventory.Quantity);

        var getResponse = await _client.GetAsync(
            $"/api/inventory/{productId}", ct);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content
            .ReadFromJsonAsync<InventoryResponseDto>(ct);

        Assert.NotNull(fetched);
        Assert.Equal(50, fetched.Quantity);
    }

    [Fact]
    public async Task AddInventory_Twice_QuantityAccumulates()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await CreateProductAsync(
            UniqueSku("SKU-INV-002"), ct);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto { ProductId = productId, Quantity = 20 }, ct);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto { ProductId = productId, Quantity = 30 }, ct);

        var getResponse = await _client.GetAsync(
            $"/api/inventory/{productId}", ct);

        var inventory = await getResponse.Content
            .ReadFromJsonAsync<InventoryResponseDto>(ct);

        Assert.NotNull(inventory);
        Assert.Equal(50, inventory.Quantity);
    }

    [Fact]
    public async Task AddInventory_InvalidQuantity_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await CreateProductAsync(
            UniqueSku("SKU-INV-003"), ct);

        var response = await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto { ProductId = productId, Quantity = 0 }, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidateStock_SufficientStock_ReturnsTrue()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await CreateProductAsync(
            UniqueSku("SKU-INV-004"), ct);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto { ProductId = productId, Quantity = 10 }, ct);

        var response = await _client.GetAsync(
            $"/api/inventory/validate?productId={productId}&quantity=5", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(productId,
            doc.RootElement.GetProperty("productId").GetInt32());
        Assert.Equal(5,
            doc.RootElement.GetProperty("quantity").GetInt32());
        Assert.True(
            doc.RootElement.GetProperty("isAvailable").GetBoolean());
    }

    [Fact]
    public async Task ValidateStock_InsufficientStock_ReturnsFalse()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await CreateProductAsync(
            UniqueSku("SKU-INV-005"), ct);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto { ProductId = productId, Quantity = 3 }, ct);

        var response = await _client.GetAsync(
            $"/api/inventory/validate?productId={productId}&quantity=10", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);
        var isAvailable = doc.RootElement
            .GetProperty("isAvailable")
            .GetBoolean();

        Assert.False(isAvailable);
    }

    [Fact]
    public async Task GetLowStock_ReturnsProductsBelowThreshold()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await CreateProductAsync(
            UniqueSku("SKU-LOW-001"), ct);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto { ProductId = productId, Quantity = 3 }, ct);

        var response = await _client.GetAsync(
            "/api/inventory/low-stock?threshold=5", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content
            .ReadFromJsonAsync<List<InventoryResponseDto>>(ct);

        Assert.NotNull(items);
        Assert.Contains(items, i => i.ProductId == productId);
    }

    [Fact]
    public async Task GetOutOfStock_ReturnsZeroQuantityProducts()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await CreateProductAsync(
            UniqueSku("SKU-OOS-001"), ct);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto { ProductId = productId, Quantity = 1 }, ct);

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Out Of Stock Customer",
                Email = $"oos-{Guid.NewGuid():N}@test.com"
            }, ct);

        Assert.Equal(HttpStatusCode.Created, customerResponse.StatusCode);

        var customer = await customerResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(customer);

        var orderResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderDto
            {
                CustomerId = customer.Id,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = productId, Quantity = 1 }
                }
            }, ct);

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(order);

        var paymentResponse = await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = order.OrderId }, ct);

        Assert.Equal(HttpStatusCode.Created, paymentResponse.StatusCode);

        var response = await _client.GetAsync(
            "/api/inventory/out-of-stock", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content
            .ReadFromJsonAsync<List<InventoryResponseDto>>(ct);

        Assert.NotNull(items);
        Assert.Contains(items,
            i => i.ProductId == productId && i.Quantity == 0);
    }

    [Fact]
    public async Task GetInventorySummary_ReturnsAggregates()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(
            "/api/inventory/summary?lowStockThreshold=10", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content
            .ReadFromJsonAsync<InventorySummaryDto>(ct);

        Assert.NotNull(summary);
        Assert.True(summary.TotalProductsTracked >= 0);
    }

    [Fact]
    public async Task GetTransactionHistory_ReturnsTransactions()
    {
        var ct = TestContext.Current.CancellationToken;

        var productId = await CreateProductAsync(
            UniqueSku("SKU-TXH-001"), ct);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto { ProductId = productId, Quantity = 25 }, ct);

        var response = await _client.GetAsync(
            $"/api/inventory/transactions/{productId}?top=10", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var transactions = await response.Content
            .ReadFromJsonAsync<List<InventoryTransactionResponseDto>>(ct);

        Assert.NotNull(transactions);
        var count = transactions.Count;
        Assert.True(count >= 1);
    }
}

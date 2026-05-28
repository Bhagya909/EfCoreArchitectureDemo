using Application.DTOs.Orders;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailProject.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace RetailProject.Tests.Integration.Customers;

public class CustomerIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _client;

    private static string UniqueEmail(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}@test.com";

    public CustomerIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task CreateCustomer_ThenReadBack_FieldsMatchExactly()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateCustomerDto
        {
            Name = "John Doe",
            Email = UniqueEmail("johndoe")
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/customers", dto, ct);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(dto.Name, created.Name);
        Assert.Equal(dto.Email.ToLowerInvariant(), created.Email);

        var getResponse = await _client.GetAsync(
            $"/api/customers/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_DuplicateEmail_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateCustomerDto
        {
            Name = "Duplicate Customer",
            Email = "duplicate@test.com"
        };

        await _client.PostAsJsonAsync("/api/customers", dto, ct);

        var duplicate = await _client.PostAsJsonAsync(
            "/api/customers", dto, ct);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_EmptyName_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateCustomerDto
        {
            Name = "",
            Email = "emptyname@test.com"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/customers", dto, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCustomer_EmptyEmail_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateCustomerDto
        {
            Name = "Valid Name",
            Email = ""
        };

        var response = await _client.PostAsJsonAsync(
            "/api/customers", dto, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCustomer_NameOnly_Succeeds()
    {
        var ct = TestContext.Current.CancellationToken;

        var createDto = new CreateCustomerDto
        {
            Name = "Original Name",
            Email = UniqueEmail("updatetest")
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/customers", createDto, ct);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(created);

        var updateDto = new UpdateCustomerDto
        {
            Name = "Updated Name"
        };

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/customers/{created.Id}", updateDto, ct);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal(createDto.Email.ToLowerInvariant(), updated.Email);
    }

    [Fact]
    public async Task SoftDeleteCustomer_ThenGet_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var createDto = new CreateCustomerDto
        {
            Name = "To Delete Customer",
            Email = UniqueEmail("todelete")
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/customers", createDto, ct);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/customers/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/customers/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task SoftDeleteCustomer_VerifyPersistedInDatabase()
    {
        var ct = TestContext.Current.CancellationToken;

        var createDto = new CreateCustomerDto
        {
            Name = "DB Delete Customer",
            Email = UniqueEmail("dbdelete")
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/customers", createDto, ct);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(created);

        await _client.DeleteAsync(
            $"/api/customers/{created.Id}", ct);

        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        var customer = await context.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == created.Id, ct);

        Assert.NotNull(customer);
        Assert.True(customer.IsDeleted);
    }

    [Fact]
    public async Task GetAllCustomers_ReturnsListSuccessfully()
    {
        var ct = TestContext.Current.CancellationToken;

        await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerDto
            {
                Name = "List Customer 1",
                Email = UniqueEmail("list1")
            }, ct);

        await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerDto
            {
                Name = "List Customer 2",
                Email = UniqueEmail("list2")
            }, ct);

        var response = await _client.GetAsync(
            "/api/customers", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var customers = await response.Content
            .ReadFromJsonAsync<List<CustomerResponseDto>>(ct);

        Assert.NotNull(customers);
        Assert.True(customers.Count >= 2);
    }

    [Fact]
    public async Task GetCustomerOrders_ReturnsOrdersSuccessfully()
    {
        var ct = TestContext.Current.CancellationToken;

        var createDto = new CreateCustomerDto
        {
            Name = "Orders Customer",
            Email = UniqueEmail("ordercustomer")
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/customers", createDto, ct);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(created);

        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new Application.DTOs.Products.CreateProductDto
            {
                Name = "Customer Orders Product",
                SKU = $"cust-orders-{Guid.NewGuid():N}",
                BasePrice = 25.00m
            }, ct);

        Assert.Equal(HttpStatusCode.Created, productResponse.StatusCode);

        var product = await productResponse.Content
            .ReadFromJsonAsync<Application.DTOs.Products.ProductResponseDto>(ct);

        Assert.NotNull(product);

        var inventoryResponse = await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new Application.DTOs.Inventory.CreateInventoryDto
            {
                ProductId = product.Id,
                Quantity = 5
            }, ct);

        Assert.Equal(HttpStatusCode.Created, inventoryResponse.StatusCode);

        var orderResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderDto
            {
                CustomerId = created.Id,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product.Id, Quantity = 2 }
                }
            }, ct);

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(order);

        var response = await _client.GetAsync(
            $"/api/customers/{created.Id}/orders", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content
            .ReadFromJsonAsync<List<OrderResponseDto>>(ct);

        Assert.NotNull(orders);
        Assert.Contains(orders,
            o => o.OrderId == order.OrderId &&
                 o.CustomerId == created.Id &&
                 o.Status == "PendingPayment");
    }
}

using Application.Common;
using Application.DTOs.Products;
using Application.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailProject.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace RetailProject.Tests.Integration.Products;

public class WriteReadValidationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _client;

    private static string UniqueSku(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}";

    public WriteReadValidationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task CreateProduct_ThenReadBack_FieldsMatchExactly()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateProductDto
        {
            Name = "Test Product",
            SKU = UniqueSku("SKU-WR-001"),
            BasePrice = 99.99m
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/products", dto, ct);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(dto.Name, created.Name);
        Assert.Equal(dto.SKU, created.SKU);
        Assert.Equal(dto.BasePrice, created.BasePrice);
        Assert.NotNull(created.RowVersion);

        var getResponse = await _client.GetAsync(
            $"/api/products/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(dto.Name, fetched.Name);
        Assert.Equal(dto.SKU, fetched.SKU);
        Assert.Equal(dto.BasePrice, fetched.BasePrice);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSKU_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateProductDto
        {
            Name = "Product A",
            SKU = "SKU-DUP-001",
            BasePrice = 50.00m
        };

        await _client.PostAsJsonAsync("/api/products", dto, ct);

        var duplicate = await _client.PostAsJsonAsync(
            "/api/products", dto, ct);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_InvalidPrice_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateProductDto
        {
            Name = "Bad Product",
            SKU = "SKU-BAD-001",
            BasePrice = -10.00m
        };

        var response = await _client.PostAsJsonAsync(
            "/api/products", dto, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_EmptyName_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateProductDto
        {
            Name = "",
            SKU = "SKU-EMPTY-001",
            BasePrice = 10.00m
        };

        var response = await _client.PostAsJsonAsync(
            "/api/products", dto, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SoftDeleteProduct_ThenGet_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateProductDto
        {
            Name = "To Delete",
            SKU = UniqueSku("SKU-DEL-002"),
            BasePrice = 25.00m
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/products", dto, ct);
        var created = await createResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/products/{created!.Id}", ct);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/products/{created.Id}", ct);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_VerifyPersistedInDatabase()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateProductDto
        {
            Name = "DB Verify Product",
            SKU = UniqueSku("SKU-DB-001"),
            BasePrice = 75.00m
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/products", dto, ct);
        var created = await createResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        var product = await context.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == created!.Id, ct);

        Assert.NotNull(product);
        Assert.Equal(dto.Name, product.Name);
        Assert.Equal(dto.SKU, product.SKU);
        Assert.Equal(dto.BasePrice, product.BasePrice);
        Assert.False(product.IsDeleted);
        Assert.NotNull(product.RowVersion);
    }

    [Fact]
    public async Task GetAllProducts_Paged_ReturnsCorrectPage()
    {
        var ct = TestContext.Current.CancellationToken;

        for (int i = 1; i <= 3; i++)
        {
            await _client.PostAsJsonAsync("/api/products",
                new CreateProductDto
                {
                    Name = $"Paged Product {i}",
                    SKU = UniqueSku($"SKU-PAGE-00{i}"),
                    BasePrice = i * 10.00m
                }, ct);
        }

        var response = await _client.GetAsync(
            "/api/products?pageNumber=1&pageSize=2", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<ProductResponseDto>>(ct);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.True(result.TotalCount >= 3);
    }
}

using Application.DTOs.Categories;
using Application.DTOs.Inventory;
using Application.DTOs.Products;
using RetailProject.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace RetailProject.Tests.Integration.Categories;

public class CategoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _client;

    private static string UniqueSku(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}";

    private static string UniqueCategoryName(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}";

    public CategoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    private async Task<CategoryResponseDto> CreateCategoryAsync(
        string name, CancellationToken ct)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/categories",
            new CreateCategoryDto { Name = name }, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var category = await response.Content
            .ReadFromJsonAsync<CategoryResponseDto>(ct);

        return category!;
    }

    private async Task<ProductResponseDto> CreateProductAsync(
        string sku, CancellationToken ct)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = $"Category Product {sku}",
                SKU = sku,
                BasePrice = 20.00m
            }, ct);

        var body = await response.Content.ReadAsStringAsync(ct);

        if (response.StatusCode != HttpStatusCode.Created)
            Assert.Fail("PRODUCT CREATE BODY: " + body);

        return (await response.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct))!;
    }

    [Fact]
    public async Task CreateCategory_ThenReadBack_FieldsMatchExactly()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateCategoryDto
        {
            Name = UniqueCategoryName("Electronics")
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/categories", dto, ct);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CategoryResponseDto>(ct);

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(dto.Name, created.Name);

        var getResponse = await _client.GetAsync(
            $"/api/categories/{created.Id}", ct);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_DuplicateName_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateCategoryDto { Name = "Duplicate Category" };

        await _client.PostAsJsonAsync("/api/categories", dto, ct);

        var duplicate = await _client.PostAsJsonAsync(
            "/api/categories", dto, ct);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_EmptyName_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;

        var dto = new CreateCategoryDto { Name = "" };

        var response = await _client.PostAsJsonAsync(
            "/api/categories", dto, ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAllCategories_ReturnsListSuccessfully()
    {
        var ct = TestContext.Current.CancellationToken;

        await CreateCategoryAsync(UniqueCategoryName("Books"), ct);
        await CreateCategoryAsync(UniqueCategoryName("Clothing"), ct);

        var response = await _client.GetAsync("/api/categories", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content
            .ReadFromJsonAsync<List<CategoryResponseDto>>(ct);

        Assert.NotNull(categories);
        Assert.True(categories.Count >= 2);
    }

    [Fact]
    public async Task SoftDeleteCategory_ThenGet_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var category = await CreateCategoryAsync(
            UniqueCategoryName("To Delete Category"), ct);

        var deleteResponse = await _client.DeleteAsync(
            $"/api/categories/{category.Id}", ct);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync(
            $"/api/categories/{category.Id}", ct);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task AssignCategory_ToProduct_Succeeds()
    {
        var ct = TestContext.Current.CancellationToken;

        var category = await CreateCategoryAsync(
            UniqueCategoryName("Assign Test Category"), ct);

        var product = await CreateProductAsync(
            UniqueSku("SKU-ASGN-001"), ct);

        var assignResponse = await _client.PostAsJsonAsync(
            $"/api/products/{product.Id}/categories",
            new AssignCategoryDto { CategoryId = category.Id }, ct);

        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
    }

    [Fact]
    public async Task AssignCategory_ThenGetProductCategories_ContainsCategory()
    {
        var ct = TestContext.Current.CancellationToken;

        var category = await CreateCategoryAsync(
            UniqueCategoryName("Product Category Test"), ct);

        var product = await CreateProductAsync(
            UniqueSku("SKU-PCAT-001"), ct);

        await _client.PostAsJsonAsync(
            $"/api/products/{product.Id}/categories",
            new AssignCategoryDto { CategoryId = category.Id }, ct);

        var getResponse = await _client.GetAsync(
            $"/api/products/{product.Id}/categories", ct);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var categories = await getResponse.Content
            .ReadFromJsonAsync<List<ProductCategoryResponseDto>>(ct);

        Assert.NotNull(categories);
        Assert.Contains(categories,
            c => c.CategoryId == category.Id);
    }

    [Fact]
    public async Task RemoveCategory_FromProduct_Succeeds()
    {
        var ct = TestContext.Current.CancellationToken;

        var category = await CreateCategoryAsync(
            UniqueCategoryName("Remove Test Category"), ct);

        var product = await CreateProductAsync(
            UniqueSku("SKU-REM-001"), ct);

        await _client.PostAsJsonAsync(
            $"/api/products/{product.Id}/categories",
            new AssignCategoryDto { CategoryId = category.Id }, ct);

        var removeResponse = await _client.DeleteAsync(
            $"/api/products/{product.Id}/categories/{category.Id}",
            ct);

        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);
    }

    [Fact]
    public async Task GetProductsByCategory_ReturnsPaginatedResults()
    {
        var ct = TestContext.Current.CancellationToken;

        var category = await CreateCategoryAsync(
            UniqueCategoryName("Paged Category"), ct);

        for (int i = 1; i <= 3; i++)
        {
            var product = await CreateProductAsync(
                UniqueSku($"SKU-PGCAT-00{i}"), ct);

            await _client.PostAsJsonAsync(
                $"/api/products/{product.Id}/categories",
                new AssignCategoryDto { CategoryId = category.Id }, ct);
        }

        var response = await _client.GetAsync(
            $"/api/categories/{category.Id}/products?page=1&pageSize=2",
            ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<Application.Common.PagedResult<ProductCategoryResponseDto>>(ct);

        Assert.NotNull(result);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.All(result.Items,
            item => Assert.Equal(category.Id, item.CategoryId));
    }

    [Fact]
    public async Task ArchiveProductsByCategory_ArchivesAllProducts()
    {
        var ct = TestContext.Current.CancellationToken;

        var category = await CreateCategoryAsync(
            UniqueCategoryName("Archive Category"), ct);

        var product1 = await CreateProductAsync(
            UniqueSku("SKU-ARCH-001"), ct);

        var product2 = await CreateProductAsync(
            UniqueSku("SKU-ARCH-002"), ct);

        await _client.PostAsJsonAsync(
            $"/api/products/{product1.Id}/categories",
            new AssignCategoryDto { CategoryId = category.Id }, ct);

        await _client.PostAsJsonAsync(
            $"/api/products/{product2.Id}/categories",
            new AssignCategoryDto { CategoryId = category.Id }, ct);

        var archiveResponse = await _client.PostAsync(
            $"/api/categories/{category.Id}/archive-products",
            null, ct);

        Assert.Equal(HttpStatusCode.OK, archiveResponse.StatusCode);

        var product1Response = await _client.GetAsync(
            $"/api/products/{product1.Id}", ct);

        Assert.Equal(HttpStatusCode.NotFound, product1Response.StatusCode);

        var product2Response = await _client.GetAsync(
            $"/api/products/{product2.Id}", ct);

        Assert.Equal(HttpStatusCode.NotFound, product2Response.StatusCode);
    }
}

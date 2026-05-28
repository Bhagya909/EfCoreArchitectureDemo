using Application.DTOs.Inventory;
using Application.DTOs.Orders;
using Application.DTOs.Payments;
using Application.DTOs.Products;
using RetailProject.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace RetailProject.Tests.Integration.Payments;

public class PaymentIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _client;

    private static string UniqueSku(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}";

    private static string UniqueEmail(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}@test.com";

    public PaymentIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    private async Task<(int orderId, decimal total)>
        CreatePaidOrderSetupAsync(
            string productSku,
            string customerEmail,
            CancellationToken ct)
    {
        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = $"Payment Product {productSku}",
                SKU = productSku,
                BasePrice = 100.00m
            }, ct);

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto
            {
                ProductId = product!.Id,
                Quantity = 10
            }, ct);

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Payment Customer",
                Email = customerEmail
            }, ct);

        var customer = await customerResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        var orderResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderDto
            {
                CustomerId = customer!.Id,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product.Id, Quantity = 2 }
                }
            }, ct);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        return (order!.OrderId, order.TotalAmount);
    }

    [Fact]
    public async Task CompletePayment_ThenGetById_FieldsMatchExactly()
    {
        var ct = TestContext.Current.CancellationToken;

        var (orderId, total) = await CreatePaidOrderSetupAsync(
            UniqueSku("SKU-PAY-001"), UniqueEmail("pay1"), ct);

        var paymentResponse = await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = orderId }, ct);

        Assert.Equal(HttpStatusCode.Created, paymentResponse.StatusCode);

        var payment = await paymentResponse.Content
            .ReadFromJsonAsync<PaymentResponseDto>(ct);

        Assert.NotNull(payment);
        Assert.True(payment.PaymentId > 0);
        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal("Completed", payment.Status);
        Assert.Equal(total, payment.Amount);
        Assert.NotNull(payment.PaidAt);

        var getResponse = await _client.GetAsync(
            $"/api/payments/{payment.PaymentId}", ct);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content
            .ReadFromJsonAsync<PaymentResponseDto>(ct);

        Assert.NotNull(fetched);
        Assert.Equal(payment.PaymentId, fetched.PaymentId);
        Assert.Equal("Completed", fetched.Status);
    }

    [Fact]
    public async Task GetPaymentByOrderId_ReturnsCorrectPayment()
    {
        var ct = TestContext.Current.CancellationToken;

        var (orderId, _) = await CreatePaidOrderSetupAsync(
            UniqueSku("SKU-PAY-002"), UniqueEmail("pay2"), ct);

        await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = orderId }, ct);

        var response = await _client.GetAsync(
            $"/api/payments/order/{orderId}", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payment = await response.Content
            .ReadFromJsonAsync<PaymentResponseDto>(ct);

        Assert.NotNull(payment);
        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal("Completed", payment.Status);
    }

    [Fact]
    public async Task CompletePayment_NonExistentOrder_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = 99999 }, ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CompletePayment_AlreadyPaidOrder_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;

        var (orderId, _) = await CreatePaidOrderSetupAsync(
            UniqueSku("SKU-PAY-003"), UniqueEmail("pay3"), ct);

        await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = orderId }, ct);

        var secondPayment = await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = orderId }, ct);

        Assert.Equal(HttpStatusCode.Conflict, secondPayment.StatusCode);
    }

    [Fact]
    public async Task GetPaymentById_NotFound_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(
            "/api/payments/99999", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPaymentByOrderId_NotFound_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await _client.GetAsync(
            "/api/payments/order/99999", ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

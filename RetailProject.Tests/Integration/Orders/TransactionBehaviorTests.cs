using Application.DTOs.Inventory;
using Application.DTOs.Orders;
using Application.DTOs.Payments;
using Application.DTOs.Products;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailProject.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace RetailProject.Tests.Integration.Orders;

public class TransactionBehaviorTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _client;

    private static string UniqueSku(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}";

    private static string UniqueEmail(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}@test.com";

    public TransactionBehaviorTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task CreateOrder_ThenCompletePayment_InventoryDeducted()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange — product
        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = "Transaction Product",
                SKU = UniqueSku("SKU-TXN-001"),
                BasePrice = 50.00m
            }, ct);

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        Assert.NotNull(product);

        // Arrange — inventory
        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto
            {
                ProductId = product.Id,
                Quantity = 10
            }, ct);

        // Arrange — customer
        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Transaction Customer",
                Email = UniqueEmail("txn")
            }, ct);

        var customer = await customerResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(customer);

        // Act — create order
        var orderResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderDto
            {
                CustomerId = customer.Id,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product.Id, Quantity = 3 }
                }
            }, ct);

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(order);

        // Act — complete payment
        var paymentResponse = await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = order.OrderId }, ct);

        Assert.Equal(HttpStatusCode.Created, paymentResponse.StatusCode);

        // Assert — inventory deducted
        var inventoryResponse = await _client.GetAsync(
            $"/api/inventory/{product.Id}", ct);

        var inventory = await inventoryResponse.Content
            .ReadFromJsonAsync<InventoryResponseDto>(ct);

        Assert.NotNull(inventory);
        Assert.Equal(7, inventory.Quantity);

        // Assert — order status is Paid
        var orderGetResponse = await _client.GetAsync(
            $"/api/orders/{order.OrderId}", ct);

        var updatedOrder = await orderGetResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(updatedOrder);
        Assert.Equal("Paid", updatedOrder.Status);
    }

    [Fact]
    public async Task CompleteOrder_AfterPayment_SetsOrderCompleted()
    {
        var ct = TestContext.Current.CancellationToken;

        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = "Complete Order Product",
                SKU = UniqueSku("SKU-COMPLETE-001"),
                BasePrice = 40.00m
            }, ct);

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        Assert.NotNull(product);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto
            {
                ProductId = product.Id,
                Quantity = 5
            }, ct);

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Complete Order Customer",
                Email = UniqueEmail("complete")
            }, ct);

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
                    new() { ProductId = product.Id, Quantity = 1 }
                }
            }, ct);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(order);

        await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = order.OrderId }, ct);

        var completeResponse = await _client.PutAsync(
            $"/api/orders/{order.OrderId}/complete", null, ct);

        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var completedOrder = await completeResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(completedOrder);
        Assert.Equal("Completed", completedOrder.Status);
    }

    [Fact]
    public async Task CompleteOrder_WithoutPayment_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;

        var order = await CreatePendingOrderAsync(
            productName: "Complete Without Payment Product",
            skuPrefix: "SKU-COMPLETE-NOPAY",
            customerName: "Complete Without Payment Customer",
            emailPrefix: "complete-nopay",
            ct);

        // SQLite does not enforce RowVersion the same way SQL Server does.
        // Concurrency conflict behavior is covered at service level in
        // ConcurrencyTests.cs, so API 409 coverage here uses business-state
        // conflicts from the order completion workflow.
        var completeResponse = await _client.PutAsync(
            $"/api/orders/{order.OrderId}/complete", null, ct);

        Assert.Equal(HttpStatusCode.Conflict, completeResponse.StatusCode);

        var body = await completeResponse.Content
            .ReadAsStringAsync(ct);

        Assert.Contains(
            "payment",
            body.ToLowerInvariant());
    }

    [Fact]
    public async Task CompleteOrder_AfterCancellation_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;

        var order = await CreatePendingOrderAsync(
            productName: "Complete Cancelled Product",
            skuPrefix: "SKU-COMPLETE-CANCEL",
            customerName: "Complete Cancelled Customer",
            emailPrefix: "complete-cancel",
            ct);

        var cancelResponse = await _client.PutAsync(
            $"/api/orders/{order.OrderId}/cancel", null, ct);

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        // SQLite does not enforce RowVersion the same way SQL Server does.
        // Concurrency conflict behavior is covered at service level in
        // ConcurrencyTests.cs, so API 409 coverage here uses business-state
        // conflicts from the order completion workflow.
        var completeResponse = await _client.PutAsync(
            $"/api/orders/{order.OrderId}/complete", null, ct);

        Assert.Equal(HttpStatusCode.Conflict, completeResponse.StatusCode);
    }

    [Fact]
    public async Task CompletePayment_WhenInventoryStockoutAfterOrderCreation_RollsBackPaymentAndLogsFailure()
    {
        var ct = TestContext.Current.CancellationToken;

        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = "Payment Rollback Product",
                SKU = UniqueSku("SKU-PAYROLL-001"),
                BasePrice = 25.00m
            }, ct);

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        Assert.NotNull(product);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto
            {
                ProductId = product.Id,
                Quantity = 1
            }, ct);

        var firstCustomerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Payment Rollback Customer One",
                Email = UniqueEmail("payroll-one")
            }, ct);

        var firstCustomer = await firstCustomerResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(firstCustomer);

        var secondCustomerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Payment Rollback Customer Two",
                Email = UniqueEmail("payroll-two")
            }, ct);

        var secondCustomer = await secondCustomerResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(secondCustomer);

        var firstOrderResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderDto
            {
                CustomerId = firstCustomer.Id,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product.Id, Quantity = 1 }
                }
            }, ct);

        var firstOrder = await firstOrderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(firstOrder);

        var secondOrderResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderDto
            {
                CustomerId = secondCustomer.Id,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product.Id, Quantity = 1 }
                }
            }, ct);

        var secondOrder = await secondOrderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(secondOrder);

        var firstPaymentResponse = await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = firstOrder.OrderId }, ct);

        Assert.Equal(HttpStatusCode.Created, firstPaymentResponse.StatusCode);

        var failedPaymentResponse = await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = secondOrder.OrderId }, ct);

        Assert.Equal(HttpStatusCode.Conflict, failedPaymentResponse.StatusCode);

        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        var failedOrder = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == secondOrder.OrderId, ct);

        Assert.NotNull(failedOrder);
        Assert.Equal(Domain.Enums.OrderStatus.PendingPayment, failedOrder.Status);

        var strayPayment = await context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == secondOrder.OrderId, ct);

        Assert.Null(strayPayment);

        var failureLog = await context.ChangeLogs
            .FirstOrDefaultAsync(log =>
                log.ActionType == "PAYMENT_FAILED" &&
                log.ReferenceId == secondOrder.OrderId, ct);

        Assert.NotNull(failureLog);
    }

    [Fact]
    public async Task CreateOrder_InsufficientStock_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange — product with low stock
        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = "Low Stock Product",
                SKU = UniqueSku("SKU-LOW-001"),
                BasePrice = 30.00m
            }, ct);

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        Assert.NotNull(product);

        // Add only 2 units
        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto
            {
                ProductId = product.Id,
                Quantity = 2
            }, ct);

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Low Stock Customer",
                Email = UniqueEmail("lowstock")
            }, ct);

        var customer = await customerResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(customer);

        // Act — order 5 units but only 2 available
        var orderResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderDto
            {
                CustomerId = customer.Id,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product.Id, Quantity = 5 }
                }
            }, ct);

        Assert.Equal(HttpStatusCode.Conflict, orderResponse.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_PendingPayment_Succeeds()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange
        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = "Cancel Product",
                SKU = UniqueSku("SKU-CANCEL-001"),
                BasePrice = 20.00m
            }, ct);

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        Assert.NotNull(product);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto
            {
                ProductId = product.Id,
                Quantity = 5
            }, ct);

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Cancel Customer",
                Email = UniqueEmail("cancel")
            }, ct);

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
                    new() { ProductId = product.Id, Quantity = 1 }
                }
            }, ct);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(order);

        // Act — cancel order
        var cancelResponse = await _client.PutAsync(
            $"/api/orders/{order.OrderId}/cancel", null, ct);

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        var cancelled = await cancelResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(cancelled);
        Assert.Equal("Cancelled", cancelled.Status);
    }

    [Fact]
    public async Task CancelOrder_AlreadyPaid_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;

        // Arrange — full order + payment flow
        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = "Paid Cancel Product",
                SKU = UniqueSku("SKU-PAIDCAN-001"),
                BasePrice = 15.00m
            }, ct);

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        Assert.NotNull(product);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto
            {
                ProductId = product.Id,
                Quantity = 5
            }, ct);

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Paid Cancel Customer",
                Email = UniqueEmail("paidcancel")
            }, ct);

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
                    new() { ProductId = product.Id, Quantity = 1 }
                }
            }, ct);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(order);

        // Complete payment
        await _client.PostAsJsonAsync(
            "/api/payments",
            new CompletePaymentDto { OrderId = order.OrderId }, ct);

        // Act — try to cancel paid order
        var cancelResponse = await _client.PutAsync(
            $"/api/orders/{order.OrderId}/cancel", null, ct);

        Assert.Equal(HttpStatusCode.Conflict, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_RollsBack_WhenProductNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = "Rollback Customer",
                Email = UniqueEmail("rollback")
            }, ct);

        var customer = await customerResponse.Content
            .ReadFromJsonAsync<CustomerResponseDto>(ct);

        Assert.NotNull(customer);

        // Act — order with non-existent product
        var orderResponse = await _client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderDto
            {
                CustomerId = customer.Id,
                Items = new List<CreateOrderItemDto>
                {
                    new() { ProductId = 99999, Quantity = 1 }
                }
            }, ct);

        Assert.Equal(HttpStatusCode.NotFound, orderResponse.StatusCode);

        // Assert — no order was persisted
        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        var orderCount = await context.Orders
            .CountAsync(o => o.CustomerId == customer.Id, ct);

        Assert.Equal(0, orderCount);
    }

    private async Task<OrderResponseDto> CreatePendingOrderAsync(
        string productName,
        string skuPrefix,
        string customerName,
        string emailPrefix,
        CancellationToken ct)
    {
        var productResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new CreateProductDto
            {
                Name = productName,
                SKU = UniqueSku(skuPrefix),
                BasePrice = 30.00m
            }, ct);

        var product = await productResponse.Content
            .ReadFromJsonAsync<ProductResponseDto>(ct);

        Assert.NotNull(product);

        await _client.PostAsJsonAsync(
            "/api/inventory/add",
            new CreateInventoryDto
            {
                ProductId = product.Id,
                Quantity = 5
            }, ct);

        var customerResponse = await _client.PostAsJsonAsync(
            "/api/customers",
            new CreateCustomerDto
            {
                Name = customerName,
                Email = UniqueEmail(emailPrefix)
            }, ct);

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
                    new() { ProductId = product.Id, Quantity = 1 }
                }
            }, ct);

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);

        var order = await orderResponse.Content
            .ReadFromJsonAsync<OrderResponseDto>(ct);

        Assert.NotNull(order);

        return order;
    }
}

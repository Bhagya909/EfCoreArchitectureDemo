using Domain.Entities.Inventory;
using Domain.Entities.Orders;
using Domain.Enums;

namespace RetailProject.Tests.Unit.Domain;

public class OrderDomainTests
{
    [Fact]
    public void MarkAsPaid_FromPendingPayment_Succeeds()
    {
        var order = new Order(customerId: 1, totalAmount: 100m);

        order.MarkAsPaid();

        Assert.Equal(OrderStatus.Paid, order.Status);
    }

    [Fact]
    public void MarkAsPaid_FromCompleted_ThrowsInvalidOperationException()
    {
        var order = new Order(customerId: 1, totalAmount: 100m);
        order.MarkAsPaid();
        order.MarkAsCompleted();

        Assert.Throws<InvalidOperationException>(
            order.MarkAsPaid);
    }

    [Fact]
    public void MarkAsPaid_FromCancelled_ThrowsInvalidOperationException()
    {
        var order = new Order(customerId: 1, totalAmount: 100m);
        order.Cancel();

        Assert.Throws<InvalidOperationException>(
            order.MarkAsPaid);
    }

    [Fact]
    public void MarkAsCompleted_FromPaid_Succeeds()
    {
        var order = new Order(customerId: 1, totalAmount: 100m);
        order.MarkAsPaid();

        order.MarkAsCompleted();

        Assert.Equal(OrderStatus.Completed, order.Status);
    }

    [Fact]
    public void MarkAsCompleted_FromPendingPayment_ThrowsInvalidOperationException()
    {
        var order = new Order(customerId: 1, totalAmount: 100m);

        Assert.Throws<InvalidOperationException>(
            order.MarkAsCompleted);
    }

    [Fact]
    public void Cancel_FromPendingPayment_Succeeds()
    {
        var order = new Order(customerId: 1, totalAmount: 100m);

        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_FromPaid_ThrowsInvalidOperationException()
    {
        var order = new Order(customerId: 1, totalAmount: 100m);
        order.MarkAsPaid();

        Assert.Throws<InvalidOperationException>(
            order.Cancel);
    }

    [Fact]
    public void InventoryTransaction_ZeroQuantity_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new InventoryTransaction(
                productId: 1,
                quantityChange: 0,
                type: InventoryTransactionType.IN,
                reason: "Invalid"));
    }

    [Fact]
    public void InventoryTransaction_InvalidEnum_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new InventoryTransaction(
                productId: 1,
                quantityChange: 1,
                type: (InventoryTransactionType)999,
                reason: "Invalid"));
    }

    [Fact]
    public void InventoryTransaction_InWithNegativeQuantity_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new InventoryTransaction(
                productId: 1,
                quantityChange: -1,
                type: InventoryTransactionType.IN,
                reason: "Invalid"));
    }

    [Fact]
    public void InventoryTransaction_OutWithPositiveQuantity_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new InventoryTransaction(
                productId: 1,
                quantityChange: 1,
                type: InventoryTransactionType.OUT,
                reason: "Invalid"));
    }
}

using Domain.Entities.Logging;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetailProject.Tests.Fixtures;

namespace RetailProject.Tests.Integration.Infrastructure;

public class UpgradePipelineTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UpgradePipelineTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UpgradeRunner_IsIdempotent_RunningTwiceProducesNoErrors()
    {
        var ct = TestContext.Current.CancellationToken;

        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        await RemovePaymentReferenceUpgradeLogAsync(context, ct);

        var upgradeRunner = scope.ServiceProvider
            .GetRequiredService<Application.Interfaces.Upgrades.IUpgradeRunner>();

        await upgradeRunner.RunUpgradesAsync();
        await upgradeRunner.RunUpgradesAsync();

        var upgradeLogs = await context.ChangeLogs
            .Where(l =>
                l.ActionType == "UPGRADE_EXECUTED" &&
                l.RawData == "Upgrade=GeneratePaymentReferenceNumbers")
            .ToListAsync(ct);

        Assert.Single(upgradeLogs);
    }

    [Fact]
    public async Task GeneratePaymentReferenceNumbersUpgrade_SetsReferenceNumbers()
    {
        var ct = TestContext.Current.CancellationToken;

        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        await RemovePaymentReferenceUpgradeLogAsync(context, ct);

        var customer = new Domain.Entities.Orders.Customer(
            "Upgrade Test",
            $"upgrade-{Guid.NewGuid():N}@test.com");
        context.Customers.Add(customer);
        await context.SaveChangesAsync(ct);

        var order = new Domain.Entities.Orders.Order(customer.Id, 100.00m);
        context.Orders.Add(order);
        await context.SaveChangesAsync(ct);

        var payment = new Domain.Entities.Orders.Payment(order.Id, 100.00m);
        payment.MarkAsCompleted();
        context.Payments.Add(payment);
        await context.SaveChangesAsync(ct);

        var paymentBefore = await context.Payments
            .FirstOrDefaultAsync(p => p.Id == payment.Id, ct);

        Assert.NotNull(paymentBefore);
        Assert.Null(paymentBefore.ReferenceNumber);

        var upgradeRunner = scope.ServiceProvider
            .GetRequiredService<Application.Interfaces.Upgrades.IUpgradeRunner>();

        await upgradeRunner.RunUpgradesAsync();

        var paymentAfter = await context.Payments
            .FirstOrDefaultAsync(p => p.Id == payment.Id, ct);

        Assert.NotNull(paymentAfter);
        Assert.NotNull(paymentAfter.ReferenceNumber);
        Assert.Equal($"PAY-{payment.Id:D6}", paymentAfter.ReferenceNumber);
    }

    [Fact]
    public async Task UpgradeRunner_LogsExecutedUpgrade_ToChangeLogs()
    {
        var ct = TestContext.Current.CancellationToken;

        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        await RemovePaymentReferenceUpgradeLogAsync(context, ct);

        var upgradeRunner = scope.ServiceProvider
            .GetRequiredService<Application.Interfaces.Upgrades.IUpgradeRunner>();

        await upgradeRunner.RunUpgradesAsync();

        var upgradeLog = await context.ChangeLogs
            .FirstOrDefaultAsync(
                l => l.ActionType == "UPGRADE_EXECUTED" &&
                l.RawData == "Upgrade=GeneratePaymentReferenceNumbers",
                ct);

        Assert.NotNull(upgradeLog);
        Assert.Equal("UPGRADE_EXECUTED", upgradeLog.ActionType);
    }

    [Fact]
    public async Task UpgradeRunner_DoesNotRerun_AlreadyExecutedUpgrade()
    {
        var ct = TestContext.Current.CancellationToken;

        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        await RemovePaymentReferenceUpgradeLogAsync(context, ct);

        var upgradeRunner = scope.ServiceProvider
            .GetRequiredService<Application.Interfaces.Upgrades.IUpgradeRunner>();

        await upgradeRunner.RunUpgradesAsync();

        var countBefore = await context.ChangeLogs
            .CountAsync(
                l => l.ActionType == "UPGRADE_EXECUTED" &&
                l.RawData == "Upgrade=GeneratePaymentReferenceNumbers",
                ct);

        await upgradeRunner.RunUpgradesAsync();

        var countAfter = await context.ChangeLogs
            .CountAsync(
                l => l.ActionType == "UPGRADE_EXECUTED" &&
                l.RawData == "Upgrade=GeneratePaymentReferenceNumbers",
                ct);

        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public async Task BackfillAiSummaryStatusUpgrade_BackfillsLegacyAiLogs_AndIsIdempotent()
    {
        var ct = TestContext.Current.CancellationToken;

        using var scope = _fixture.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<RetailDbContext>();

        await RemoveUpgradeLogAsync(
            context,
            "BackfillAiSummaryStatus",
            ct);

        var legacyLog = new ChangeLog(
            actionType: "LEGACY_AI_EVENT",
            entityName: "ChangeLog",
            referenceId: null,
            rawData: "Legacy AI summary existed before status tracking.",
            description: "Legacy AI log.",
            logSource: LogSource.Audit,
            correlationId: Guid.NewGuid());

        context.ChangeLogs.Add(legacyLog);
        await context.SaveChangesAsync(ct);

        await context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE ChangeLogs SET AiSummary = {"Legacy generated summary."}, AiSummaryStatus = {"NotRequested"} WHERE Id = {legacyLog.Id}",
            ct);

        context.ChangeTracker.Clear();

        var upgradeRunner = scope.ServiceProvider
            .GetRequiredService<Application.Interfaces.Upgrades.IUpgradeRunner>();

        await upgradeRunner.RunUpgradesAsync();
        await upgradeRunner.RunUpgradesAsync();

        var upgradedLog = await context.ChangeLogs
            .FirstOrDefaultAsync(l => l.Id == legacyLog.Id, ct);

        Assert.NotNull(upgradedLog);
        Assert.Equal(AiSummaryStatus.Completed, upgradedLog.AiSummaryStatus);
        Assert.Equal("Legacy generated summary.", upgradedLog.AiSummary);
        Assert.NotNull(upgradedLog.AiSummaryGeneratedAt);

        var upgradeLogCount = await context.ChangeLogs
            .CountAsync(
                l => l.ActionType == "UPGRADE_EXECUTED" &&
                l.RawData == "Upgrade=BackfillAiSummaryStatus",
                ct);

        Assert.Equal(1, upgradeLogCount);
    }

    private static async Task RemovePaymentReferenceUpgradeLogAsync(
        RetailDbContext context,
        CancellationToken ct)
    {
        await RemoveUpgradeLogAsync(
            context,
            "GeneratePaymentReferenceNumbers",
            ct);
    }

    private static async Task RemoveUpgradeLogAsync(
        RetailDbContext context,
        string upgradeName,
        CancellationToken ct)
    {
        var upgradeLogs = await context.ChangeLogs
            .Where(l =>
                l.ActionType == "UPGRADE_EXECUTED" &&
                l.RawData == $"Upgrade={upgradeName}")
            .ToListAsync(ct);

        context.ChangeLogs.RemoveRange(upgradeLogs);
        await context.SaveChangesAsync(ct);
    }
}

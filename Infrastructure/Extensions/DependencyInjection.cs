using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces.Upgrades;
using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Configurations.Logging;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Upgrades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IProductService, ProductService>();

        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IChangeLogRepository, ChangeLogRepository>();

        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IChangeLogService, ChangeLogService>();

        services.Configure<GeminiSettings>(
                configuration.GetSection(GeminiSettings.SectionName));
        services.AddScoped<IAiSummaryService, GeminiSummaryService>();
        services.AddScoped<IAiEnrichmentProcessor, AiEnrichmentProcessor>();
        services.AddHostedService<AiEnrichmentBackgroundService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IDataUpgrade,
        GeneratePaymentReferenceNumbersUpgrade>();

        services.AddScoped<IDataUpgrade,
        BackfillAiSummaryStatusUpgrade>();

        services.AddScoped<IUpgradeRunner,
            UpgradeRunner>();

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerService, CustomerService>();
        // Category
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}

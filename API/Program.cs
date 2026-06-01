using API.Middleware;
using API.Swagger;
using Application.Interfaces.Upgrades;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

builder.Services.AddDbContext<RetailDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "DefaultConnection"));

        options.AddInterceptors(
            serviceProvider.GetRequiredService<
                AuditSaveChangesInterceptor>());
    });

// Infrastructure — repositories, services, DbContext
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ProblemDetails
builder.Services.AddProblemDetails();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Retail Project API",
        Version = "v1",
        Description =
            "A production-oriented retail backend demonstrating " +
            "Clean Architecture, EF Core advanced features, " +
            "database evolution, and AI-enhanced observability."
    });

    options.TagActionsBy(api =>
        new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });

    options.DocInclusionPredicate((_, _) => true);
    options.OperationFilter<RetailSwaggerOperationFilter>();
    options.SchemaFilter<RetailSwaggerSchemaFilter>();
    options.SupportNonNullableReferenceTypes();

    foreach (var xmlFile in new[]
    {
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
        "Application.xml"
    })
    {
        var xmlPath =
            Path.Combine(AppContext.BaseDirectory, xmlFile);

        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Run migrations and upgrade pipeline on startup — skip in Testing
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<RetailDbContext>();
    await context.Database.MigrateAsync();
    var upgradeRunner = services.GetRequiredService<IUpgradeRunner>();
    await upgradeRunner.RunUpgradesAsync();
}

// Global exception middleware — must be first
app.UseMiddleware<ExceptionMiddleware>();

// Swagger
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("v1/swagger.json", "RetailProject API v1");
    options.DocumentTitle = "Retail Project API";
    options.DisplayRequestDuration();
    options.DisplayOperationId();
    options.EnableDeepLinking();
    options.DefaultModelExpandDepth(2);
    options.DefaultModelsExpandDepth(1);
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
});

// Root redirect to Swagger
app.MapGet("/", () => Results.Redirect("swagger"));

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }

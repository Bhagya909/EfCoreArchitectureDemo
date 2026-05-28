using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

public class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<RetailDbContext>
{
    public RetailDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = FindApiProjectPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. " +
                $"Set it in {Path.Combine(apiProjectPath, "appsettings.json")} " +
                "or provide it through configuration before running EF Core commands.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<RetailDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new RetailDbContext(optionsBuilder.Options);
    }

    private static string FindApiProjectPath()
    {
        var searchRoots = new[]
        {
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        };

        foreach (var searchRoot in searchRoots)
        {
            var currentDirectory = new DirectoryInfo(searchRoot);

            while (currentDirectory is not null)
            {
                var apiProjectPath = Path.Combine(
                    currentDirectory.FullName,
                    "API");

                if (File.Exists(Path.Combine(apiProjectPath, "API.csproj")))
                    return apiProjectPath;

                if (currentDirectory.Name.Equals(
                        "API",
                        StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(Path.Combine(
                        currentDirectory.FullName,
                        "API.csproj")))
                {
                    return currentDirectory.FullName;
                }

                currentDirectory = currentDirectory.Parent;
            }
        }

        throw new InvalidOperationException(
            "Could not locate the API project directory. " +
            "Run EF Core commands from the solution root, Infrastructure project, " +
            "API project, or another directory under the solution.");
    }
}

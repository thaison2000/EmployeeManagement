using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EmployeeManagement.Data;

public sealed class EmployeeDbContextFactory : IDesignTimeDbContextFactory<EmployeeDbContext>
{
    public EmployeeDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<global::Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = configuration.GetConnectionString("EmployeeDatabase")
            ?? throw new InvalidOperationException(
                "Set ConnectionStrings__EmployeeDatabase or configure it with User Secrets.");

        var optionsBuilder = new DbContextOptionsBuilder<EmployeeDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new EmployeeDbContext(optionsBuilder.Options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Gestionnaire.ApiService.Data;

public sealed class ContextbdFactory : IDesignTimeDbContextFactory<Contextbd>
{
    public Contextbd CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__sitecontent")
            ?? "Host=localhost;Database=gestionnaire;Username=postgres";
        var options = new DbContextOptionsBuilder<Contextbd>()
            .UseNpgsql(connectionString)
            .Options;

        return new Contextbd(options);
    }
}

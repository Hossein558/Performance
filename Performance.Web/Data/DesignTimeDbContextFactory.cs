using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Performance.Web.Data;

/// <summary>
/// Used exclusively by EF Core CLI commands (migrations add, database update).
/// Always targets the test/development database so production data is never touched
/// during design-time operations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=HE110749-LP2B;Database=test;User Id=sa;Password=135246Eac;TrustServerCertificate=True;");
        return new AppDbContext(optionsBuilder.Options);
    }
}

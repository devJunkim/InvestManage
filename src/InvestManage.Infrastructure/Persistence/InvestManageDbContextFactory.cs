using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvestManage.Infrastructure.Persistence;

public sealed class InvestManageDbContextFactory : IDesignTimeDbContextFactory<InvestManageDbContext>
{
    private const string DevelopmentConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=InvestManage;Trusted_Connection=True;TrustServerCertificate=True";

    public InvestManageDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__InvestManage")
            ?? DevelopmentConnectionString;

        var options = new DbContextOptionsBuilder<InvestManageDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new InvestManageDbContext(options);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InvestManage.Infrastructure.Persistence;

public sealed class InvestManageDbContextFactory : IDesignTimeDbContextFactory<InvestManageDbContext>
{
    public InvestManageDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<InvestManageDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=InvestManage;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new InvestManageDbContext(options);
    }
}

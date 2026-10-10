using InvestManage.Application.Accounts;
using InvestManage.Application.Investments;
using InvestManage.Application.Transactions;
using InvestManage.Application.Users;
using InvestManage.Infrastructure.Persistence;
using InvestManage.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InvestManage.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInvestManageInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<InvestManageDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer => sqlServer.EnableRetryOnFailure()));
        services.AddScoped<IInvestmentAccountRepository, InvestmentAccountRepository>();
        services.AddScoped<IInvestmentItemRepository, InvestmentItemRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}

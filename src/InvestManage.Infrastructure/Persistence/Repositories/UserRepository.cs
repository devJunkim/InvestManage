using InvestManage.Application.Users;
using InvestManage.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace InvestManage.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(InvestManageDbContext context) : IUserRepository
{
    public Task<User?> FindByIdentifierAsync(
        string normalizedIdentifier,
        CancellationToken cancellationToken) =>
        context.Users.AsNoTracking().SingleOrDefaultAsync(
            user => user.NormalizedEmail == normalizedIdentifier ||
                    user.NormalizedLoginId == normalizedIdentifier,
            cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<bool> LoginIdExistsAsync(string normalizedLoginId, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(user => user.NormalizedLoginId == normalizedLoginId, cancellationToken);

    public void Add(User user) => context.Users.Add(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}

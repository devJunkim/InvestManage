using InvestManage.Domain.Users;

namespace InvestManage.Application.Users;

public interface IUserRepository
{
    Task<User?> FindByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> LoginIdExistsAsync(string normalizedLoginId, CancellationToken cancellationToken);

    void Add(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

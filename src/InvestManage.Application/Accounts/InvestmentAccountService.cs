using InvestManage.Application.Common;
using InvestManage.Domain.Accounts;

namespace InvestManage.Application.Accounts;

public sealed class InvestmentAccountService(IInvestmentAccountRepository repository)
{
    public Task<IReadOnlyList<InvestmentAccount>> ListAsync(
        Guid? userId,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw Validation("userId", "A non-empty user identifier is required.");
        }

        return repository.ListAsync(userId, includeArchived, cancellationToken);
    }

    public async Task<InvestmentAccount> GetAsync(
        Guid id,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        ValidateAccountId(id);

        return await repository.FindAsync(id, includeArchived, false, cancellationToken)
            ?? throw new ResourceNotFoundException("Investment account", id);
    }

    public async Task<InvestmentAccount> CreateAsync(
        CreateInvestmentAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        var (name, currencyCode) = ValidateDetails(
            command.UserId,
            command.Name,
            command.Type,
            command.CurrencyCode);

        var errors = new Dictionary<string, string[]>();
        if (!await repository.UserExistsAsync(command.UserId, cancellationToken))
        {
            errors["userId"] = ["The specified user does not exist."];
        }

        if (!await repository.CurrencyExistsAsync(currencyCode, cancellationToken))
        {
            errors["currencyCode"] = ["The specified currency does not exist."];
        }

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var account = new InvestmentAccount(
            Guid.NewGuid(),
            command.UserId,
            name,
            command.Type,
            currencyCode);

        repository.Add(account);
        await repository.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<InvestmentAccount> UpdateAsync(
        Guid id,
        UpdateInvestmentAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateAccountId(id);
        var account = await FindForLifecycleAsync(id, cancellationToken);

        if (account.IsArchived)
        {
            throw Validation("account", "Reactivate the investment account before updating it.");
        }

        var (name, currencyCode) = ValidateDetails(
            account.UserId,
            command.Name,
            command.Type,
            command.CurrencyCode);

        if (!await repository.CurrencyExistsAsync(currencyCode, cancellationToken))
        {
            throw Validation("currencyCode", "The specified currency does not exist.");
        }

        account.UpdateDetails(name, command.Type, currencyCode);
        await repository.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ValidateAccountId(id);
        var account = await FindForLifecycleAsync(id, cancellationToken);
        account.Archive();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ValidateAccountId(id);
        var account = await FindForLifecycleAsync(id, cancellationToken);
        account.Reactivate();
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<InvestmentAccount> FindForLifecycleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await repository.FindAsync(id, true, true, cancellationToken)
            ?? throw new ResourceNotFoundException("Investment account", id);
    }

    private static (string Name, string CurrencyCode) ValidateDetails(
        Guid userId,
        string? name,
        InvestmentAccountType type,
        string? currencyCode)
    {
        var errors = new Dictionary<string, string[]>();

        if (userId == Guid.Empty)
        {
            errors["userId"] = ["A non-empty user identifier is required."];
        }

        var normalizedName = name?.Trim() ?? string.Empty;
        if (normalizedName.Length == 0)
        {
            errors["name"] = ["An account name is required."];
        }
        else if (normalizedName.Length > 200)
        {
            errors["name"] = ["The account name cannot exceed 200 characters."];
        }

        if (!Enum.IsDefined(type))
        {
            errors["type"] = ["The account type is not supported."];
        }

        var normalizedCurrency = currencyCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalizedCurrency.Length != 3 ||
            normalizedCurrency.Any(character => !char.IsAsciiLetterUpper(character)))
        {
            errors["currencyCode"] = ["A currency code must contain exactly three ASCII letters."];
        }

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        return (normalizedName, normalizedCurrency);
    }

    private static void ValidateAccountId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw Validation("id", "A non-empty investment account identifier is required.");
        }
    }

    private static ApplicationValidationException Validation(string key, string message) =>
        new(new Dictionary<string, string[]> { [key] = [message] });
}

using InvestManage.Application.Common;
using InvestManage.Domain.Accounts;
using InvestManage.Domain.Investments;

namespace InvestManage.Application.Investments;

public sealed class InvestmentItemService(IInvestmentItemRepository repository)
{
    public Task<IReadOnlyList<InvestmentItem>> SearchAsync(
        InvestmentItemSearch query,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();
        if (query.Type.HasValue && !Enum.IsDefined(query.Type.Value))
        {
            errors["type"] = ["The investment type is not supported."];
        }

        var search = NormalizeOptional(query.Search);
        if (search?.Length > 200)
        {
            errors["search"] = ["Search text cannot exceed 200 characters."];
        }

        var provider = NormalizeOptional(query.Provider);
        if (provider?.Length > 200)
        {
            errors["provider"] = ["Provider cannot exceed 200 characters."];
        }

        string? currencyCode = null;
        if (!string.IsNullOrWhiteSpace(query.CurrencyCode))
        {
            currencyCode = NormalizeCurrency(query.CurrencyCode, errors);
        }

        ThrowIfInvalid(errors);
        return repository.SearchAsync(
            search,
            query.Type,
            currencyCode,
            provider,
            query.IncludeArchived,
            cancellationToken);
    }

    public async Task<InvestmentItem> GetAsync(
        Guid id,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id, "id", "investment item");
        return await repository.FindAsync(id, includeArchived, false, cancellationToken)
            ?? throw new ResourceNotFoundException("Investment item", id);
    }

    public async Task<InvestmentItem> CreateAsync(
        CreateInvestmentItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var details = ValidateDetails(
            command.Code,
            command.Name,
            command.Type,
            command.CurrencyCode,
            command.Provider,
            command.PricePrecision,
            command.Notes);

        if (!await repository.CurrencyExistsAsync(details.CurrencyCode, cancellationToken))
        {
            throw Validation("currencyCode", "The specified currency does not exist.");
        }

        var item = new InvestmentItem(
            Guid.NewGuid(),
            details.Code,
            details.Name,
            command.Type,
            details.CurrencyCode,
            details.Provider,
            command.PricePrecision,
            details.Notes);

        repository.Add(item);
        await repository.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<InvestmentItem> UpdateAsync(
        Guid id,
        UpdateInvestmentItemCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id, "id", "investment item");
        var item = await FindForLifecycleAsync(id, cancellationToken);
        if (item.IsArchived)
        {
            throw Validation("investmentItem", "Reactivate the investment item before updating it.");
        }

        var details = ValidateDetails(
            command.Code,
            command.Name,
            command.Type,
            command.CurrencyCode,
            command.Provider,
            command.PricePrecision,
            command.Notes);

        if (!await repository.CurrencyExistsAsync(details.CurrencyCode, cancellationToken))
        {
            throw Validation("currencyCode", "The specified currency does not exist.");
        }

        item.UpdateDetails(
            details.Code,
            details.Name,
            command.Type,
            details.CurrencyCode,
            details.Provider,
            command.PricePrecision,
            details.Notes);
        await repository.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ValidateId(id, "id", "investment item");
        var item = await FindForLifecycleAsync(id, cancellationToken);
        item.Archive();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        ValidateId(id, "id", "investment item");
        var item = await FindForLifecycleAsync(id, cancellationToken);
        item.Reactivate();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InvestmentItem>> ListForAccountAsync(
        Guid accountId,
        bool includeArchived,
        CancellationToken cancellationToken = default)
    {
        ValidateId(accountId, "accountId", "investment account");
        if (!await repository.AccountExistsAsync(accountId, true, cancellationToken))
        {
            throw new ResourceNotFoundException("Investment account", accountId);
        }

        return await repository.ListForAccountAsync(accountId, includeArchived, cancellationToken);
    }

    public async Task<InvestmentAssignmentResult> AssignToAccountAsync(
        Guid accountId,
        Guid investmentItemId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(accountId, "accountId", "investment account");
        ValidateId(investmentItemId, "investmentItemId", "investment item");

        var item = await repository.FindAsync(investmentItemId, true, false, cancellationToken)
            ?? throw new ResourceNotFoundException("Investment item", investmentItemId);
        if (item.IsArchived)
        {
            throw Validation("investmentItemId", "An archived investment item cannot be assigned.");
        }

        if (!await repository.AccountExistsAsync(accountId, false, cancellationToken))
        {
            throw new ResourceNotFoundException("Investment account", accountId);
        }

        var existing = await repository.FindAssignmentAsync(
            accountId,
            investmentItemId,
            cancellationToken);
        if (existing is not null)
        {
            return new InvestmentAssignmentResult(existing, false);
        }

        var assignment = new AccountInvestment(Guid.NewGuid(), accountId, investmentItemId);
        repository.Add(assignment);
        await repository.SaveChangesAsync(cancellationToken);
        return new InvestmentAssignmentResult(assignment, true);
    }

    private async Task<InvestmentItem> FindForLifecycleAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        await repository.FindAsync(id, true, true, cancellationToken)
        ?? throw new ResourceNotFoundException("Investment item", id);

    private static InvestmentItemDetails ValidateDetails(
        string? code,
        string? name,
        InvestmentType type,
        string? currencyCode,
        string? provider,
        int pricePrecision,
        string? notes)
    {
        var errors = new Dictionary<string, string[]>();
        var normalizedCode = code?.Trim() ?? string.Empty;
        if (normalizedCode.Length == 0)
        {
            errors["code"] = ["An investment code is required."];
        }
        else if (normalizedCode.Length > 50)
        {
            errors["code"] = ["Investment code cannot exceed 50 characters."];
        }

        var normalizedName = name?.Trim() ?? string.Empty;
        if (normalizedName.Length == 0)
        {
            errors["name"] = ["An investment name is required."];
        }
        else if (normalizedName.Length > 200)
        {
            errors["name"] = ["Investment name cannot exceed 200 characters."];
        }

        if (!Enum.IsDefined(type))
        {
            errors["type"] = ["The investment type is not supported."];
        }

        var normalizedCurrency = NormalizeCurrency(currencyCode, errors);
        var normalizedProvider = NormalizeOptional(provider);
        if (normalizedProvider?.Length > 200)
        {
            errors["provider"] = ["Provider cannot exceed 200 characters."];
        }

        if (pricePrecision is < 0 or > 8)
        {
            errors["pricePrecision"] = ["Price precision must be between zero and eight decimal places."];
        }

        var normalizedNotes = NormalizeOptional(notes);
        if (normalizedNotes?.Length > 2000)
        {
            errors["notes"] = ["Notes cannot exceed 2000 characters."];
        }

        ThrowIfInvalid(errors);
        return new InvestmentItemDetails(
            normalizedCode,
            normalizedName,
            normalizedCurrency,
            normalizedProvider,
            normalizedNotes);
    }

    private static string NormalizeCurrency(
        string? currencyCode,
        IDictionary<string, string[]> errors)
    {
        var normalized = currencyCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalized.Length != 3 || normalized.Any(character => !char.IsAsciiLetterUpper(character)))
        {
            errors["currencyCode"] = ["A currency code must contain exactly three ASCII letters."];
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateId(Guid id, string key, string resourceName)
    {
        if (id == Guid.Empty)
        {
            throw Validation(key, $"A non-empty {resourceName} identifier is required.");
        }
    }

    private static void ThrowIfInvalid(Dictionary<string, string[]> errors)
    {
        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }
    }

    private static ApplicationValidationException Validation(string key, string message) =>
        new(new Dictionary<string, string[]> { [key] = [message] });

    private sealed record InvestmentItemDetails(
        string Code,
        string Name,
        string CurrencyCode,
        string? Provider,
        string? Notes);
}

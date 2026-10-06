using InvestManage.Application.Common;
using InvestManage.Application.Investments;
using InvestManage.Domain.Accounts;
using InvestManage.Domain.Investments;

namespace InvestManage.Application.Tests.Investments;

public sealed class InvestmentItemServiceTests
{
    private static readonly Guid AccountId = Guid.Parse("2948144e-c9f1-4f62-bb3d-e90936017765");
    private static readonly Guid SecondAccountId = Guid.Parse("c24dbf25-ec62-4d22-9d10-1c70c6be3343");

    [Theory]
    [MemberData(nameof(InvalidCreateCommands))]
    public async Task Create_InvalidDetails_ThrowsValidationException(
        CreateInvestmentItemCommand command,
        string expectedError)
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.CreateAsync(command));

        Assert.Contains(expectedError, exception.Errors.Keys);
    }

    [Fact]
    public async Task Create_UnknownCurrency_ThrowsValidationException()
    {
        var repository = CreateRepository();
        var service = new InvestmentItemService(repository);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.CreateAsync(ValidCreateCommand() with { CurrencyCode = "USD" }));

        Assert.Contains("currencyCode", exception.Errors.Keys);
        Assert.Empty(repository.Items);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Create_ValidCommand_NormalizesAndPersistsItem()
    {
        var repository = CreateRepository();
        var service = new InvestmentItemService(repository);

        var item = await service.CreateAsync(ValidCreateCommand());

        Assert.Equal("tdb3046", item.Code);
        Assert.Equal("TDB3046", item.NormalizedCode);
        Assert.Equal("TD mutual fund", item.Name);
        Assert.Equal("CAD", item.CurrencyCode);
        Assert.Equal("TD", item.Provider);
        Assert.Equal(4, item.PricePrecision);
        Assert.Equal("Core holding", item.Notes);
        Assert.Same(item, Assert.Single(repository.Items));
        Assert.Equal(1, repository.SaveCount);
    }

    [Theory]
    [MemberData(nameof(InvalidSearches))]
    public void Search_InvalidFilter_ThrowsValidationException(
        InvestmentItemSearch query,
        string expectedError)
    {
        var service = CreateService();

        var exception = Assert.Throws<ApplicationValidationException>(() =>
        {
            _ = service.SearchAsync(query);
        });

        Assert.Contains(expectedError, exception.Errors.Keys);
    }

    [Fact]
    public async Task Search_ValidFilters_ReturnsMatchingItems()
    {
        var repository = CreateRepository();
        repository.Items.AddRange(
        [
            CreateItem("TDB3046", InvestmentType.MutualFund, "TD"),
            CreateItem("AAPL", InvestmentType.Stock, null)
        ]);
        var service = new InvestmentItemService(repository);

        var result = await service.SearchAsync(
            new InvestmentItemSearch("tdb", InvestmentType.MutualFund, "cad", "TD", false));

        Assert.Equal("TDB3046", Assert.Single(result).Code);
    }

    [Fact]
    public async Task Update_ArchivedItem_RequiresReactivation()
    {
        var repository = CreateRepository();
        var item = CreateItem();
        item.Archive();
        repository.Items.Add(item);
        var service = new InvestmentItemService(repository);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.UpdateAsync(item.Id, ValidUpdateCommand()));

        Assert.Contains("investmentItem", exception.Errors.Keys);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Update_ValidCommand_ChangesAndPersistsItem()
    {
        var repository = CreateRepository();
        var item = CreateItem();
        repository.Items.Add(item);
        var service = new InvestmentItemService(repository);

        var result = await service.UpdateAsync(item.Id, ValidUpdateCommand());

        Assert.Same(item, result);
        Assert.Equal("ML1436", item.NormalizedCode);
        Assert.Equal(InvestmentType.SegregatedFund, item.Type);
        Assert.Equal(6, item.PricePrecision);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Get_MissingItem_ThrowsNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            service.GetAsync(Guid.NewGuid(), false));
    }

    [Fact]
    public async Task ArchiveAndReactivate_PersistEachStateChange()
    {
        var repository = CreateRepository();
        var item = CreateItem();
        repository.Items.Add(item);
        var service = new InvestmentItemService(repository);

        await service.ArchiveAsync(item.Id);
        Assert.True(item.IsArchived);

        await service.ReactivateAsync(item.Id);
        Assert.False(item.IsArchived);
        Assert.Equal(2, repository.SaveCount);
    }

    [Fact]
    public async Task AssignToAccount_ReusesExistingIdentityAndAssignment()
    {
        var repository = CreateRepository();
        var item = CreateItem();
        repository.Items.Add(item);
        var service = new InvestmentItemService(repository);

        var first = await service.AssignToAccountAsync(AccountId, item.Id);
        var repeated = await service.AssignToAccountAsync(AccountId, item.Id);
        var secondAccount = await service.AssignToAccountAsync(SecondAccountId, item.Id);

        Assert.True(first.Created);
        Assert.False(repeated.Created);
        Assert.Same(first.Assignment, repeated.Assignment);
        Assert.True(secondAccount.Created);
        Assert.Equal(2, repository.Assignments.Count);
        Assert.Single(repository.Items);
    }

    [Fact]
    public async Task AssignToAccount_ArchivedItem_ThrowsValidationException()
    {
        var repository = CreateRepository();
        var item = CreateItem();
        item.Archive();
        repository.Items.Add(item);
        var service = new InvestmentItemService(repository);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.AssignToAccountAsync(AccountId, item.Id));

        Assert.Contains("investmentItemId", exception.Errors.Keys);
    }

    [Fact]
    public async Task AssignToAccount_MissingAccount_ThrowsNotFoundException()
    {
        var repository = CreateRepository();
        var item = CreateItem();
        repository.Items.Add(item);
        var service = new InvestmentItemService(repository);

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            service.AssignToAccountAsync(Guid.NewGuid(), item.Id));
    }

    [Fact]
    public async Task ListForAccount_MissingAccount_ThrowsNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            service.ListForAccountAsync(Guid.NewGuid(), false));
    }

    public static TheoryData<CreateInvestmentItemCommand, string> InvalidCreateCommands =>
        new()
        {
            { ValidCreateCommand() with { Code = " " }, "code" },
            { ValidCreateCommand() with { Code = new string('A', 51) }, "code" },
            { ValidCreateCommand() with { Name = " " }, "name" },
            { ValidCreateCommand() with { Name = new string('A', 201) }, "name" },
            { ValidCreateCommand() with { Type = (InvestmentType)0 }, "type" },
            { ValidCreateCommand() with { CurrencyCode = "CA" }, "currencyCode" },
            { ValidCreateCommand() with { Provider = new string('A', 201) }, "provider" },
            { ValidCreateCommand() with { PricePrecision = -1 }, "pricePrecision" },
            { ValidCreateCommand() with { PricePrecision = 9 }, "pricePrecision" },
            { ValidCreateCommand() with { Notes = new string('A', 2001) }, "notes" }
        };

    public static TheoryData<InvestmentItemSearch, string> InvalidSearches =>
        new()
        {
            { new InvestmentItemSearch(new string('A', 201), null, null, null, false), "search" },
            { new InvestmentItemSearch(null, (InvestmentType)0, null, null, false), "type" },
            { new InvestmentItemSearch(null, null, "CA", null, false), "currencyCode" },
            { new InvestmentItemSearch(null, null, null, new string('A', 201), false), "provider" }
        };

    private static InvestmentItemService CreateService() => new(CreateRepository());

    private static FakeInvestmentItemRepository CreateRepository() =>
        new(["CAD"], [AccountId, SecondAccountId]);

    private static CreateInvestmentItemCommand ValidCreateCommand() =>
        new(
            " tdb3046 ",
            " TD mutual fund ",
            InvestmentType.MutualFund,
            " cad ",
            " TD ",
            4,
            " Core holding ");

    private static UpdateInvestmentItemCommand ValidUpdateCommand() =>
        new(
            "ml1436",
            "Manulife investment",
            InvestmentType.SegregatedFund,
            "CAD",
            "Manulife",
            6,
            "Updated");

    private static InvestmentItem CreateItem(
        string code = "TDB3046",
        InvestmentType type = InvestmentType.MutualFund,
        string? provider = "TD") =>
        new(Guid.NewGuid(), code, $"{code} investment", type, "CAD", provider);

    private sealed class FakeInvestmentItemRepository(
        IEnumerable<string> currencyCodes,
        IEnumerable<Guid> accountIds)
        : IInvestmentItemRepository
    {
        private readonly HashSet<string> currencies = [.. currencyCodes];
        private readonly HashSet<Guid> accounts = [.. accountIds];

        public List<InvestmentItem> Items { get; } = [];

        public List<AccountInvestment> Assignments { get; } = [];

        public int SaveCount { get; private set; }

        public Task<IReadOnlyList<InvestmentItem>> SearchAsync(
            string? search,
            InvestmentType? type,
            string? currencyCode,
            string? provider,
            bool includeArchived,
            CancellationToken cancellationToken)
        {
            IEnumerable<InvestmentItem> query = Items;
            if (!includeArchived)
            {
                query = query.Where(item => !item.IsArchived);
            }

            if (search is not null)
            {
                query = query.Where(item =>
                    item.NormalizedCode.Contains(search.ToUpperInvariant()) ||
                    item.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (type.HasValue)
            {
                query = query.Where(item => item.Type == type.Value);
            }

            if (currencyCode is not null)
            {
                query = query.Where(item => item.CurrencyCode == currencyCode);
            }

            if (provider is not null)
            {
                query = query.Where(item => item.Provider == provider);
            }

            return Task.FromResult<IReadOnlyList<InvestmentItem>>(query.ToList());
        }

        public Task<IReadOnlyList<InvestmentItem>> ListForAccountAsync(
            Guid accountId,
            bool includeArchived,
            CancellationToken cancellationToken)
        {
            var itemIds = Assignments
                .Where(assignment => assignment.InvestmentAccountId == accountId)
                .Select(assignment => assignment.InvestmentItemId)
                .ToHashSet();
            IReadOnlyList<InvestmentItem> result = Items
                .Where(item => itemIds.Contains(item.Id))
                .Where(item => includeArchived || !item.IsArchived)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<InvestmentItem?> FindAsync(
            Guid id,
            bool includeArchived,
            bool trackChanges,
            CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(item =>
                item.Id == id && (includeArchived || !item.IsArchived)));

        public Task<bool> CurrencyExistsAsync(
            string currencyCode,
            CancellationToken cancellationToken) =>
            Task.FromResult(currencies.Contains(currencyCode));

        public Task<bool> AccountExistsAsync(
            Guid accountId,
            bool includeArchived,
            CancellationToken cancellationToken) =>
            Task.FromResult(accounts.Contains(accountId));

        public Task<AccountInvestment?> FindAssignmentAsync(
            Guid accountId,
            Guid investmentItemId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Assignments.SingleOrDefault(assignment =>
                assignment.InvestmentAccountId == accountId &&
                assignment.InvestmentItemId == investmentItemId));

        public void Add(InvestmentItem investmentItem) => Items.Add(investmentItem);

        public void Add(AccountInvestment assignment) => Assignments.Add(assignment);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}

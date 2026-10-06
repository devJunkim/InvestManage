using InvestManage.Application.Accounts;
using InvestManage.Application.Common;
using InvestManage.Domain.Accounts;

namespace InvestManage.Application.Tests.Accounts;

public sealed class InvestmentAccountServiceTests
{
    private static readonly Guid UserId = Guid.Parse("02c4042e-4c54-4d4e-b5a1-8f728233f335");

    [Fact]
    public void List_EmptyUserId_ThrowsValidationException()
    {
        var service = CreateService();

        var exception = Assert.Throws<ApplicationValidationException>(() =>
        {
            _ = service.ListAsync(Guid.Empty, false);
        });

        Assert.Contains("userId", exception.Errors.Keys);
    }

    [Fact]
    public async Task List_ValidUser_ReturnsRepositoryResults()
    {
        var repository = CreateRepository();
        var account = CreateAccount();
        repository.Accounts.Add(account);
        var service = new InvestmentAccountService(repository);

        var result = await service.ListAsync(UserId, false);

        Assert.Same(account, Assert.Single(result));
    }

    [Theory]
    [MemberData(nameof(InvalidCreateCommands))]
    public async Task Create_InvalidDetails_ThrowsValidationException(
        CreateInvestmentAccountCommand command,
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
        var service = new InvestmentAccountService(repository);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.CreateAsync(ValidCreateCommand() with { CurrencyCode = "USD" }));

        Assert.Contains("currencyCode", exception.Errors.Keys);
        Assert.Empty(repository.Accounts);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Create_UnknownUser_ThrowsValidationException()
    {
        var repository = CreateRepository();
        var service = new InvestmentAccountService(repository);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.CreateAsync(ValidCreateCommand() with { UserId = Guid.NewGuid() }));

        Assert.Contains("userId", exception.Errors.Keys);
        Assert.Empty(repository.Accounts);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Create_ValidCommand_NormalizesAndPersistsAccount()
    {
        var repository = CreateRepository();
        var service = new InvestmentAccountService(repository);

        var account = await service.CreateAsync(
            ValidCreateCommand() with { Name = "  Retirement  ", CurrencyCode = " cad " });

        Assert.Equal("Retirement", account.Name);
        Assert.Equal("CAD", account.CurrencyCode);
        Assert.False(account.IsArchived);
        Assert.Same(account, Assert.Single(repository.Accounts));
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Update_ArchivedAccount_RequiresReactivation()
    {
        var repository = CreateRepository();
        var account = CreateAccount();
        account.Archive();
        repository.Accounts.Add(account);
        var service = new InvestmentAccountService(repository);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.UpdateAsync(account.Id, ValidUpdateCommand()));

        Assert.Contains("account", exception.Errors.Keys);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Update_UnknownCurrency_ThrowsValidationException()
    {
        var repository = CreateRepository();
        var account = CreateAccount();
        repository.Accounts.Add(account);
        var service = new InvestmentAccountService(repository);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.UpdateAsync(account.Id, ValidUpdateCommand() with { CurrencyCode = "USD" }));

        Assert.Contains("currencyCode", exception.Errors.Keys);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Update_ValidCommand_ChangesAndPersistsAccount()
    {
        var repository = CreateRepository();
        var account = CreateAccount();
        repository.Accounts.Add(account);
        var service = new InvestmentAccountService(repository);

        var result = await service.UpdateAsync(account.Id, ValidUpdateCommand());

        Assert.Same(account, result);
        Assert.Equal("Updated retirement", account.Name);
        Assert.Equal(InvestmentAccountType.RegisteredRetirementIncomeFund, account.Type);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Get_EmptyId_ThrowsValidationException()
    {
        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.GetAsync(Guid.Empty, false));

        Assert.Contains("id", exception.Errors.Keys);
    }

    [Fact]
    public async Task Get_MissingAccount_ThrowsNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            service.GetAsync(Guid.NewGuid(), false));
    }

    [Fact]
    public async Task Get_ExistingAccount_ReturnsAccount()
    {
        var repository = CreateRepository();
        var account = CreateAccount();
        repository.Accounts.Add(account);
        var service = new InvestmentAccountService(repository);

        var result = await service.GetAsync(account.Id, false);

        Assert.Same(account, result);
    }

    [Fact]
    public async Task ArchiveAndReactivate_PersistEachStateChange()
    {
        var repository = CreateRepository();
        var account = CreateAccount();
        repository.Accounts.Add(account);
        var service = new InvestmentAccountService(repository);

        await service.ArchiveAsync(account.Id);
        Assert.True(account.IsArchived);

        await service.ReactivateAsync(account.Id);
        Assert.False(account.IsArchived);
        Assert.Equal(2, repository.SaveCount);
    }

    public static TheoryData<CreateInvestmentAccountCommand, string> InvalidCreateCommands =>
        new()
        {
            { ValidCreateCommand() with { UserId = Guid.Empty }, "userId" },
            { ValidCreateCommand() with { Name = " " }, "name" },
            { ValidCreateCommand() with { Name = new string('A', 201) }, "name" },
            { ValidCreateCommand() with { Type = (InvestmentAccountType)0 }, "type" },
            { ValidCreateCommand() with { CurrencyCode = "CA" }, "currencyCode" }
        };

    private static InvestmentAccountService CreateService() => new(CreateRepository());

    private static FakeInvestmentAccountRepository CreateRepository() =>
        new([UserId], ["CAD"]);

    private static CreateInvestmentAccountCommand ValidCreateCommand() =>
        new(
            UserId,
            "Retirement",
            InvestmentAccountType.RegisteredRetirementSavingsPlan,
            "CAD");

    private static UpdateInvestmentAccountCommand ValidUpdateCommand() =>
        new(
            "Updated retirement",
            InvestmentAccountType.RegisteredRetirementIncomeFund,
            "CAD");

    private static InvestmentAccount CreateAccount() =>
        new(
            Guid.NewGuid(),
            UserId,
            "Retirement",
            InvestmentAccountType.RegisteredRetirementSavingsPlan,
            "CAD");

    private sealed class FakeInvestmentAccountRepository(
        IEnumerable<Guid> userIds,
        IEnumerable<string> currencyCodes)
        : IInvestmentAccountRepository
    {
        private readonly HashSet<Guid> users = [.. userIds];
        private readonly HashSet<string> currencies = [.. currencyCodes];

        public List<InvestmentAccount> Accounts { get; } = [];

        public int SaveCount { get; private set; }

        public Task<IReadOnlyList<InvestmentAccount>> ListAsync(
            Guid? userId,
            bool includeArchived,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<InvestmentAccount> result = Accounts
                .Where(account => !userId.HasValue || account.UserId == userId.Value)
                .Where(account => includeArchived || !account.IsArchived)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<InvestmentAccount?> FindAsync(
            Guid id,
            bool includeArchived,
            bool trackChanges,
            CancellationToken cancellationToken)
        {
            var account = Accounts.SingleOrDefault(item =>
                item.Id == id && (includeArchived || !item.IsArchived));
            return Task.FromResult(account);
        }

        public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult(users.Contains(userId));

        public Task<bool> CurrencyExistsAsync(
            string currencyCode,
            CancellationToken cancellationToken) =>
            Task.FromResult(currencies.Contains(currencyCode));

        public void Add(InvestmentAccount account) => Accounts.Add(account);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}

using InvestManage.Application.Common;
using InvestManage.Application.Transactions;
using InvestManage.Domain.Accounts;
using InvestManage.Domain.Transactions;

namespace InvestManage.Application.Tests.Transactions;

public sealed class TransactionServiceTests
{
    private static readonly Guid AccountId = Guid.Parse("23bcd69b-6806-4981-a4fc-0ee93a27c123");
    private static readonly Guid ItemId = Guid.Parse("1fe4ca47-03e6-4be9-87de-178b66e3e550");
    private static readonly Guid AssignmentId = Guid.Parse("9ca83866-463a-4e91-a5d4-8c4f6c8a3f01");

    [Theory]
    [InlineData("quantity")]
    [InlineData("unitPrice")]
    [InlineData("fees")]
    [InlineData("settlementDate")]
    [InlineData("currencyCode")]
    public async Task Create_InvalidDetails_ReturnsFieldValidation(string field)
    {
        var command = ValidCommand() with
        {
            Quantity = field == "quantity" ? 0 : 10,
            UnitPrice = field == "unitPrice" ? 0 : 20.0812m,
            Fees = field == "fees" ? -1 : 0,
            SettlementDate = field == "settlementDate" ? new DateOnly(2026, 9, 30) : null,
            CurrencyCode = field == "currencyCode" ? "CA" : "CAD"
        };

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            new TransactionService(new FakeRepository()).CreateAsync(command));

        Assert.Contains(field, exception.Errors.Keys);
    }

    [Fact]
    public async Task Create_Buy_PreservesFractionalValuesAndPersists()
    {
        var repository = new FakeRepository();
        var service = new TransactionService(repository);

        var result = await service.CreateAsync(ValidCommand() with
        {
            Quantity = 12.345678m,
            Notes = "  Initial purchase  "
        });

        Assert.Equal(12.345678m, result.Transaction.Quantity);
        Assert.Equal(20.0812m, result.Transaction.UnitPrice);
        Assert.Equal("Initial purchase", result.Transaction.Notes);
        Assert.Same(result.Transaction, Assert.Single(repository.Transactions));
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Create_SellGreaterThanUnitsHeld_IsRejected()
    {
        var repository = new FakeRepository();
        repository.Transactions.Add(Transaction(TransactionType.Buy, new DateOnly(2026, 10, 1), 10));
        var service = new TransactionService(repository);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.CreateAsync(ValidCommand() with
            {
                Type = TransactionType.Sell,
                TradeDate = new DateOnly(2026, 10, 2),
                Quantity = 10.00000001m
            }));

        Assert.Contains("quantity", exception.Errors.Keys);
        Assert.Single(repository.Transactions);
        Assert.Equal(0, repository.SaveCount);
    }

    [Fact]
    public async Task Create_BackdatedSellThatMakesLaterBalanceNegative_IsRejected()
    {
        var repository = new FakeRepository();
        repository.Transactions.Add(Transaction(TransactionType.Buy, new DateOnly(2026, 10, 1), 10));
        repository.Transactions.Add(Transaction(TransactionType.Sell, new DateOnly(2026, 10, 3), 8));
        var service = new TransactionService(repository);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() =>
            service.CreateAsync(ValidCommand() with
            {
                Type = TransactionType.Sell,
                TradeDate = new DateOnly(2026, 10, 2),
                Quantity = 3
            }));

        Assert.Contains("quantity", exception.Errors.Keys);
        Assert.Equal(2, repository.Transactions.Count);
    }

    [Fact]
    public async Task Create_UnassignedInvestment_IsNotFound()
    {
        var repository = new FakeRepository { Assignment = null };

        await Assert.ThrowsAsync<ResourceNotFoundException>(() =>
            new TransactionService(repository).CreateAsync(ValidCommand()));
    }

    private static CreateTransactionCommand ValidCommand() =>
        new(
            AccountId,
            ItemId,
            TransactionType.Buy,
            new DateOnly(2026, 10, 1),
            null,
            10,
            20.0812m,
            0,
            "cad",
            null);

    private static Transaction Transaction(TransactionType type, DateOnly date, decimal quantity) =>
        new(
            Guid.NewGuid(),
            AssignmentId,
            type,
            date,
            quantity,
            20.0812m,
            0,
            "CAD",
            createdAtUtc: new DateTimeOffset(date.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero));

    private sealed class FakeRepository : ITransactionRepository
    {
        public AccountInvestment? Assignment { get; set; } = new(AssignmentId, AccountId, ItemId);
        public List<Transaction> Transactions { get; } = [];
        public int SaveCount { get; private set; }

        public Task<AccountInvestment?> FindActiveAssignmentAsync(Guid accountId, Guid investmentItemId, CancellationToken cancellationToken) =>
            Task.FromResult(Assignment);

        public Task<bool> CurrencyExistsAsync(string currencyCode, CancellationToken cancellationToken) =>
            Task.FromResult(currencyCode == "CAD");

        public Task<IReadOnlyList<Transaction>> ListForAssignmentAsync(Guid accountInvestmentId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Transaction>>(Transactions.ToList());

        public void Add(Transaction transaction) => Transactions.Add(transaction);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}

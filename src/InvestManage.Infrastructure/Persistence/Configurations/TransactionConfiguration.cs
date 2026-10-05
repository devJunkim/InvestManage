using InvestManage.Domain.Accounts;
using InvestManage.Domain.Currencies;
using InvestManage.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestManage.Infrastructure.Persistence.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Transactions_Quantity_Positive", "[Quantity] > 0");
            tableBuilder.HasCheckConstraint("CK_Transactions_UnitPrice_Positive", "[UnitPrice] > 0");
            tableBuilder.HasCheckConstraint("CK_Transactions_Fees_NotNegative", "[Fees] >= 0");
            tableBuilder.HasCheckConstraint(
                "CK_Transactions_SettlementDate",
                "[SettlementDate] IS NULL OR [SettlementDate] >= [TradeDate]");
        });

        builder.HasKey(transaction => transaction.Id);
        builder.Property(transaction => transaction.Id).ValueGeneratedNever();
        builder.Property(transaction => transaction.Type).HasConversion<int>().IsRequired();
        builder.Property(transaction => transaction.TradeDate).HasColumnType("date").IsRequired();
        builder.Property(transaction => transaction.SettlementDate).HasColumnType("date");
        builder.Property(transaction => transaction.Quantity).HasPrecision(28, 8).IsRequired();
        builder.Property(transaction => transaction.UnitPrice).HasPrecision(19, 8).IsRequired();
        builder.Property(transaction => transaction.Fees).HasPrecision(19, 4).IsRequired();
        builder.Property(transaction => transaction.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();

        builder.HasIndex(transaction => new { transaction.AccountInvestmentId, transaction.TradeDate });

        builder.HasOne<AccountInvestment>()
            .WithMany()
            .HasForeignKey(transaction => transaction.AccountInvestmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(transaction => transaction.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

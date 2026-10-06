using InvestManage.Domain.Accounts;
using InvestManage.Domain.Investments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestManage.Infrastructure.Persistence.Configurations;

internal sealed class AccountInvestmentConfiguration : IEntityTypeConfiguration<AccountInvestment>
{
    public void Configure(EntityTypeBuilder<AccountInvestment> builder)
    {
        builder.ToTable("AccountInvestments");
        builder.HasKey(accountInvestment => accountInvestment.Id);
        builder.Property(accountInvestment => accountInvestment.Id).ValueGeneratedNever();

        builder.HasIndex(accountInvestment => new
        {
            accountInvestment.InvestmentAccountId,
            accountInvestment.InvestmentItemId
        })
            .IsUnique();

        builder.HasOne<InvestmentAccount>()
            .WithMany()
            .HasForeignKey(accountInvestment => accountInvestment.InvestmentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<InvestmentItem>()
            .WithMany()
            .HasForeignKey(accountInvestment => accountInvestment.InvestmentItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

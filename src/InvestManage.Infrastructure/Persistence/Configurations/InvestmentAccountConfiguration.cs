using InvestManage.Domain.Accounts;
using InvestManage.Domain.Currencies;
using InvestManage.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestManage.Infrastructure.Persistence.Configurations;

internal sealed class InvestmentAccountConfiguration : IEntityTypeConfiguration<InvestmentAccount>
{
    public void Configure(EntityTypeBuilder<InvestmentAccount> builder)
    {
        builder.ToTable("InvestmentAccounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Id).ValueGeneratedNever();
        builder.Property(account => account.Name).HasMaxLength(200).IsRequired();
        builder.Property(account => account.Type).HasConversion<int>().IsRequired();
        builder.Property(account => account.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(account => account.IsArchived).HasDefaultValue(false).IsRequired();

        builder.HasIndex(account => new { account.UserId, account.IsArchived, account.Name });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(account => account.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(account => account.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using InvestManage.Domain.Currencies;
using InvestManage.Domain.Investments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestManage.Infrastructure.Persistence.Configurations;

internal sealed class InvestmentItemConfiguration : IEntityTypeConfiguration<InvestmentItem>
{
    public void Configure(EntityTypeBuilder<InvestmentItem> builder)
    {
        builder.ToTable("InvestmentItems");
        builder.HasKey(investment => investment.Id);
        builder.Property(investment => investment.Id).ValueGeneratedNever();
        builder.Property(investment => investment.Code).HasMaxLength(50).IsRequired();
        builder.Property(investment => investment.NormalizedCode).HasMaxLength(50).IsRequired();
        builder.Property(investment => investment.Name).HasMaxLength(200).IsRequired();
        builder.Property(investment => investment.Provider).HasMaxLength(200);
        builder.Property(investment => investment.Type).HasConversion<int>().IsRequired();
        builder.Property(investment => investment.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();

        builder.HasIndex(investment => investment.NormalizedCode);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(investment => investment.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

using InvestManage.Domain.Currencies;
using InvestManage.Domain.Investments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestManage.Infrastructure.Persistence.Configurations;

internal sealed class InvestmentItemConfiguration : IEntityTypeConfiguration<InvestmentItem>
{
    public void Configure(EntityTypeBuilder<InvestmentItem> builder)
    {
        builder.ToTable("InvestmentItems", table =>
            table.HasCheckConstraint(
                "CK_InvestmentItems_PricePrecision",
                "[PricePrecision] BETWEEN 0 AND 8"));
        builder.HasKey(investment => investment.Id);
        builder.Property(investment => investment.Id).ValueGeneratedNever();
        builder.Property(investment => investment.Code).HasMaxLength(50).IsRequired();
        builder.Property(investment => investment.NormalizedCode).HasMaxLength(50).IsRequired();
        builder.Property(investment => investment.Name).HasMaxLength(200).IsRequired();
        builder.Property(investment => investment.Provider).HasMaxLength(200);
        builder.Property(investment => investment.PricePrecision).HasDefaultValue(4).IsRequired();
        builder.Property(investment => investment.Notes).HasMaxLength(2000);
        builder.Property(investment => investment.IsArchived).HasDefaultValue(false).IsRequired();
        builder.Property(investment => investment.Type).HasConversion<int>().IsRequired();
        builder.Property(investment => investment.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();

        builder.HasIndex(investment => new { investment.IsArchived, investment.NormalizedCode });
        builder.HasIndex(investment => new
        {
            investment.IsArchived,
            investment.Type,
            investment.CurrencyCode
        });

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(investment => investment.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

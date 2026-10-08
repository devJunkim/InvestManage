using InvestManage.Domain.Currencies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestManage.Infrastructure.Persistence.Configurations;

internal sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");
        builder.HasKey(currency => currency.Code);
        builder.Property(currency => currency.Code).HasMaxLength(3).IsFixedLength().ValueGeneratedNever();
        builder.Property(currency => currency.Name).HasMaxLength(100).IsRequired();
        builder.HasData(
            new Currency("CAD", "Canadian dollar"),
            new Currency("USD", "United States dollar"));
    }
}

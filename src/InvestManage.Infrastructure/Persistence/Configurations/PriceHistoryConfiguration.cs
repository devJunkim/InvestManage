using InvestManage.Domain.Investments;
using InvestManage.Domain.Prices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvestManage.Infrastructure.Persistence.Configurations;

internal sealed class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.ToTable("PriceHistory", tableBuilder =>
            tableBuilder.HasCheckConstraint("CK_PriceHistory_Price_Positive", "[Price] > 0"));

        builder.HasKey(price => price.Id);
        builder.Property(price => price.Id).ValueGeneratedNever();
        builder.Property(price => price.PriceDate).HasColumnType("date").IsRequired();
        builder.Property(price => price.Price).HasPrecision(19, 8).IsRequired();
        builder.Property(price => price.Type).HasConversion<int>().IsRequired();
        builder.Property(price => price.Source).HasMaxLength(100).IsRequired();

        builder.HasIndex(price => new
        {
            price.InvestmentItemId,
            price.PriceDate,
            price.Type,
            price.Source
        })
            .IsUnique();

        builder.HasOne<InvestmentItem>()
            .WithMany()
            .HasForeignKey(price => price.InvestmentItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

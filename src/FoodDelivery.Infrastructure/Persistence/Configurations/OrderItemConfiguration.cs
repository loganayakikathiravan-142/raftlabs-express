using FoodDelivery.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodDelivery.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ItemNameSnapshot)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.UnitPriceSnapshot)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Ignore(i => i.LineTotal);
    }
}

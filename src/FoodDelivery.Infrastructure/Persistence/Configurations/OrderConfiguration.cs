using FoodDelivery.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodDelivery.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.DeliveryAddress)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(o => o.PhoneNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.DeliveryFee)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasIndex(o => o.CreatedAtUtc);
        builder.HasIndex(o => o.Status);

        builder.HasMany(o => o.OrderItems)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

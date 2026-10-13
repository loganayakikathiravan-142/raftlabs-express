using FoodDelivery.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodDelivery.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(m => m.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(m => m.Category);
        builder.HasIndex(m => m.Name);
    }
}

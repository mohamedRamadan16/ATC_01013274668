using EventHorizon.Models.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace EventHorizon.Models.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.Venue)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Price)
            .HasColumnType("decimal(10, 2)");

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETDATE()")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETDATE()")
            .IsRequired();

        builder.Property(e => e.EventDate)
            .IsRequired();

        builder.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // indexes for filtering/sorting
        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.EventDate);
        builder.HasIndex(e => e.Price);
    }
}

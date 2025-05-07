using EventHorizon.Models.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventHorizon.DataAccess.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext() { }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    public DbSet<Event> Events { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Event>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("getdate()");

        builder.Entity<Event>()
            .Property(p => p.UpdatedAt)
            .HasDefaultValueSql("getdate()");

        builder.Entity<Event>()
            .Property(p => p.EventDate)
            .HasDefaultValueSql("getdate()");

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Events)
            .WithOne(e => e.Owner)
            .HasForeignKey(e => e.OwnerId);
    }

}

using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.Event.Domain;

namespace ModularMonolithSample.Event.Infrastructure;

public class EventDbContext : DbContext
{
    public DbSet<Event> Events => Set<Event>();

    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });
    }
} 
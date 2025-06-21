using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.Event.Domain;
using EventEntity = ModularMonolithSample.Event.Domain.Event;

namespace ModularMonolithSample.Event.Infrastructure;

public class EventDbContext : DbContext
{
    public DbSet<EventEntity> Events => Set<EventEntity>();

    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            
            // Ignore domain events - they should not be persisted
            entity.Ignore(e => e.DomainEvents);
        });
    }
} 
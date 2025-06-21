using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.Attendee.Domain;
using AttendeeEntity = ModularMonolithSample.Attendee.Domain.Attendee;

namespace ModularMonolithSample.Attendee.Infrastructure;

public class AttendeeDbContext : DbContext
{
    public DbSet<AttendeeEntity> Attendees => Set<AttendeeEntity>();

    public AttendeeDbContext(DbContextOptions<AttendeeDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttendeeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.EventId).IsRequired();
            entity.Property(e => e.RegistrationDate).IsRequired();
            
            // Ignore domain events - they should not be persisted
            entity.Ignore(e => e.DomainEvents);
        });
    }
} 
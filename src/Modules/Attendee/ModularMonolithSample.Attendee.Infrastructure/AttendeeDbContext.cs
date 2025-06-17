using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.Attendee.Domain;

namespace ModularMonolithSample.Attendee.Infrastructure;

public class AttendeeDbContext : DbContext
{
    public DbSet<Attendee> Attendees => Set<Attendee>();

    public AttendeeDbContext(DbContextOptions<AttendeeDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.EventId).IsRequired();
            entity.Property(e => e.RegistrationDate).IsRequired();
        });
    }
} 
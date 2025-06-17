using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.Ticket.Domain;

namespace ModularMonolithSample.Ticket.Infrastructure;

public class TicketDbContext : DbContext
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TicketNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EventId).IsRequired();
            entity.Property(e => e.AttendeeId).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.IssueDate).IsRequired();
            entity.Property(e => e.Status).IsRequired();

            entity.HasIndex(e => e.TicketNumber).IsUnique();
        });
    }
} 
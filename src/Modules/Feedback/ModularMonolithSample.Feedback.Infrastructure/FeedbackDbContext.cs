using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.Feedback.Domain;
using FeedbackEntity = ModularMonolithSample.Feedback.Domain.Feedback;

namespace ModularMonolithSample.Feedback.Infrastructure;

public class FeedbackDbContext : DbContext
{
    public DbSet<FeedbackEntity> Feedbacks => Set<FeedbackEntity>();

    public FeedbackDbContext(DbContextOptions<FeedbackDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeedbackEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventId).IsRequired();
            entity.Property(e => e.AttendeeId).IsRequired();
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Comment).IsRequired();
            entity.Property(e => e.SubmissionDate).IsRequired();

            // Ensure one feedback per attendee per event
            entity.HasIndex(e => new { e.EventId, e.AttendeeId }).IsUnique();
        });
    }
} 
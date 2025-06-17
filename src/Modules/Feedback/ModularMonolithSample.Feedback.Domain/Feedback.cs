using System;

namespace ModularMonolithSample.Feedback.Domain;

public class Feedback
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid AttendeeId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; }
    public DateTime SubmissionDate { get; private set; }

    private Feedback() { } // For EF Core

    public Feedback(Guid eventId, Guid attendeeId, int rating, string comment)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));
        }

        Id = Guid.NewGuid();
        EventId = eventId;
        AttendeeId = attendeeId;
        Rating = rating;
        Comment = comment;
        SubmissionDate = DateTime.UtcNow;
    }

    public void UpdateFeedback(int rating, string comment)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.", nameof(rating));
        }

        Rating = rating;
        Comment = comment;
    }
} 
namespace ModularMonolithSample.Feedback.Application.Queries.GetFeedback;

public record FeedbackDto(
    Guid Id,
    Guid EventId,
    Guid AttendeeId,
    int Rating,
    string Comment,
    DateTime SubmissionDate); 
using MediatR;

namespace ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;

public record SubmitFeedbackCommand(
    Guid EventId,
    Guid AttendeeId,
    int Rating,
    string Comment) : IRequest<Guid>; 

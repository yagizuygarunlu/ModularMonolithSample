using MediatR;

namespace ModularMonolithSample.Feedback.Application.Queries.GetFeedback;

public record GetFeedbackQuery(Guid Id) : IRequest<FeedbackDto?>; 
using MediatR;
using ModularMonolithSample.Feedback.Domain;

namespace ModularMonolithSample.Feedback.Application.Queries.GetFeedback;

public class GetFeedbackQueryHandler : IRequestHandler<GetFeedbackQuery, FeedbackDto?>
{
    private readonly IFeedbackRepository _feedbackRepository;

    public GetFeedbackQueryHandler(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<FeedbackDto?> Handle(GetFeedbackQuery request, CancellationToken cancellationToken)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (feedback == null)
            return null;

        return new FeedbackDto(
            feedback.Id,
            feedback.EventId,
            feedback.AttendeeId,
            feedback.Rating,
            feedback.Comment,
            feedback.SubmissionDate);
    }
} 
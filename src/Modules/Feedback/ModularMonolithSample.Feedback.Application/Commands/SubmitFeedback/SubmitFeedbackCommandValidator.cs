using FluentValidation;

namespace ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;

public class SubmitFeedbackCommandValidator : AbstractValidator<SubmitFeedbackCommand>
{
    public SubmitFeedbackCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required");

        RuleFor(x => x.AttendeeId)
            .NotEmpty()
            .WithMessage("Attendee ID is required");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .NotEmpty()
            .WithMessage("Comment is required")
            .MaximumLength(1000)
            .WithMessage("Comment cannot exceed 1000 characters");
    }
} 
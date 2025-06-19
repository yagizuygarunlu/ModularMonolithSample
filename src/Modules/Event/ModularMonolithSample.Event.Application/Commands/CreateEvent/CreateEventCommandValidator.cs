using FluentValidation;

namespace ModularMonolithSample.Event.Application.Commands.CreateEvent;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Event name is required")
            .MaximumLength(200)
            .WithMessage("Event name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Event description is required")
            .MaximumLength(1000)
            .WithMessage("Event description cannot exceed 1000 characters");

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.Now)
            .WithMessage("Event start date must be in the future");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("Event end date must be after start date");

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage("Event location is required")
            .MaximumLength(200)
            .WithMessage("Event location cannot exceed 200 characters");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Event capacity must be greater than 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("Event capacity cannot exceed 10,000");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Event price cannot be negative");
    }
} 
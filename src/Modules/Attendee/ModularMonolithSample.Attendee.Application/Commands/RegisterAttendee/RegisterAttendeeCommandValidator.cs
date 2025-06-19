using FluentValidation;

namespace ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;

public class RegisterAttendeeCommandValidator : AbstractValidator<RegisterAttendeeCommand>
{
    public RegisterAttendeeCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(100)
            .WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(100)
            .WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .MaximumLength(200)
            .WithMessage("Email cannot exceed 200 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be a valid format");

        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required");
    }
} 
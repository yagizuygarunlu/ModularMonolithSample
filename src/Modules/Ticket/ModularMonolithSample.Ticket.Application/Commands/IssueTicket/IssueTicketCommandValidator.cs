using FluentValidation;

namespace ModularMonolithSample.Ticket.Application.Commands.IssueTicket;

public class IssueTicketCommandValidator : AbstractValidator<IssueTicketCommand>
{
    public IssueTicketCommandValidator()
    {
        RuleFor(x => x.EventId)
            .NotEmpty()
            .WithMessage("Event ID is required");

        RuleFor(x => x.AttendeeId)
            .NotEmpty()
            .WithMessage("Attendee ID is required");
    }
} 
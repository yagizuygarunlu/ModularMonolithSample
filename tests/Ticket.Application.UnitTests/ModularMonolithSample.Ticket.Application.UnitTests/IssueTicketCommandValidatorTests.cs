using FluentValidation.TestHelper;
using ModularMonolithSample.Ticket.Application.Commands.IssueTicket;
using Shouldly;
using Xunit;

namespace ModularMonolithSample.Ticket.Application.UnitTests;

public class IssueTicketCommandValidatorTests
{
    private readonly IssueTicketCommandValidator _validator;

    public IssueTicketCommandValidatorTests()
    {
        _validator = new IssueTicketCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new IssueTicketCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEventId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new IssueTicketCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventId)
            .WithErrorMessage("Event ID is required");
    }

    [Fact]
    public void Validate_EmptyAttendeeId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new IssueTicketCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AttendeeId)
            .WithErrorMessage("Attendee ID is required");
    }

    [Fact]
    public void Validate_BothIdsEmpty_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new IssueTicketCommand(Guid.Empty, Guid.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventId)
            .WithErrorMessage("Event ID is required");
        result.ShouldHaveValidationErrorFor(x => x.AttendeeId)
            .WithErrorMessage("Attendee ID is required");
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Guid.Empty
    [InlineData("11111111-1111-1111-1111-111111111111")] // Valid GUID
    [InlineData("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")] // Valid GUID
    public void Validate_EventIdValues_ShouldValidateCorrectly(string guidString)
    {
        // Arrange
        var eventId = Guid.Parse(guidString);
        var command = new IssueTicketCommand(eventId, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (eventId == Guid.Empty)
        {
            result.ShouldHaveValidationErrorFor(x => x.EventId);
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(x => x.EventId);
        }
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Guid.Empty
    [InlineData("22222222-2222-2222-2222-222222222222")] // Valid GUID
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")] // Valid GUID
    public void Validate_AttendeeIdValues_ShouldValidateCorrectly(string guidString)
    {
        // Arrange
        var attendeeId = Guid.Parse(guidString);
        var command = new IssueTicketCommand(Guid.NewGuid(), attendeeId);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        if (attendeeId == Guid.Empty)
        {
            result.ShouldHaveValidationErrorFor(x => x.AttendeeId);
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(x => x.AttendeeId);
        }
    }

    [Fact]
    public void Validate_AllPropertiesValid_ShouldPassValidation()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new IssueTicketCommand(eventId, attendeeId);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_SameEventAndAttendeeId_ShouldStillBeValid()
    {
        // Arrange
        var sameId = Guid.NewGuid();
        var command = new IssueTicketCommand(sameId, sameId);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }
} 

using System;
using FluentValidation.TestHelper;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using Xunit;

namespace ModularMonolithSample.Attendee.Application.UnitTests.Commands.RegisterAttendee;

public class RegisterAttendeeCommandValidatorTests
{
    private readonly RegisterAttendeeCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Given_InvalidName_Should_HaveValidationError(string name)
    {
        var command = new RegisterAttendeeCommand(name, "test@test.com", Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    public void Given_InvalidEmail_Should_HaveValidationError(string email)
    {
        var command = new RegisterAttendeeCommand("Test Name", email, Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Given_ValidCommand_Should_NotHaveValidationError()
    {
        var command = new RegisterAttendeeCommand("Test Name", "test@test.com", Guid.NewGuid());
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
} 
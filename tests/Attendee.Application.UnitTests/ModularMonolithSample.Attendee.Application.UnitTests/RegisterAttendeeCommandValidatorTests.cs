using System;
using FluentValidation.TestHelper;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using Shouldly;
using Xunit;

namespace ModularMonolithSample.Attendee.Application.UnitTests;

public class RegisterAttendeeCommandValidatorTests
{
    private readonly RegisterAttendeeCommandValidator _validator;

    public RegisterAttendeeCommandValidatorTests()
    {
        _validator = new RegisterAttendeeCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            "john.doe@email.com",
            "+1234567890",
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Validate_EmptyFirstName_ShouldHaveValidationError(string? firstName)
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            firstName,
            "Doe",
            "john.doe@email.com",
            "+1234567890",
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name is required");
    }

    [Fact]
    public void Validate_FirstNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longFirstName = new string('A', 101); // Exceeds 100 character limit
        var command = new RegisterAttendeeCommand(
            longFirstName,
            "Doe",
            "john.doe@email.com",
            "+1234567890",
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("First name cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Validate_EmptyLastName_ShouldHaveValidationError(string? lastName)
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            "John",
            lastName,
            "john.doe@email.com",
            "+1234567890",
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name is required");
    }

    [Fact]
    public void Validate_LastNameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longLastName = new string('B', 101); // Exceeds 100 character limit
        var command = new RegisterAttendeeCommand(
            "John",
            longLastName,
            "john.doe@email.com",
            "+1234567890",
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Last name cannot exceed 100 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Validate_EmptyEmail_ShouldHaveValidationError(string? email)
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            email,
            "+1234567890",
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@invalid.com")]
    [InlineData("invalid.email")]
    public void Validate_InvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: email,
            PhoneNumber: "+1234567890",
            EventId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_EmailTooLong_ShouldHaveValidationError()
    {
        // Arrange - 201 characters to exceed 200 limit
        var longEmail = new string('a', 185) + "@example.com"; // 185 + 12 = 197, let's make it longer
        longEmail = new string('a', 188) + "@example.com"; // 188 + 12 = 200, still not enough
        longEmail = new string('a', 189) + "@example.com"; // 189 + 12 = 201, this should work
        var command = new RegisterAttendeeCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: longEmail,
            PhoneNumber: "+1234567890",
            EventId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Validate_EmptyPhoneNumber_ShouldHaveValidationError(string? phoneNumber)
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            "john.doe@email.com",
            phoneNumber,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("Phone number is required");
    }

    [Theory]
    [InlineData("0123456789")] // starts with 0
    [InlineData("abc123")] // contains letters
    [InlineData("+0123456789")] // starts with +0
    [InlineData("1")] // too short
    [InlineData("+12345678901234567")] // too long (16 digits)
    public void Validate_InvalidPhoneNumberFormat_ShouldHaveValidationError(string phoneNumber)
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@email.com",
            PhoneNumber: phoneNumber,
            EventId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("1234567890")]
    [InlineData("+1234567890123")]
    [InlineData("123456789012345")]
    [InlineData("+44123456789")]
    public void Validate_ValidPhoneNumber_ShouldNotHaveValidationError(string phoneNumber)
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            "john.doe@email.com",
            phoneNumber,
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Validate_EmptyEventId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            "john.doe@email.com",
            "+1234567890",
            Guid.Empty
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventId)
            .WithErrorMessage("Event ID is required");
    }

    [Theory]
    [InlineData("john@example.com")]
    [InlineData("jane.doe@company.co.uk")]
    [InlineData("user123@test-domain.org")]
    [InlineData("test.email+tag@domain.com")]
    public void Validate_ValidEmail_ShouldNotHaveValidationError(string email)
    {
        // Arrange
        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            email,
            "+1234567890",
            Guid.NewGuid()
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
} 
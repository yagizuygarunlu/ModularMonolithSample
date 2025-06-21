using System;
using FluentValidation.TestHelper;
using ModularMonolithSample.Event.Application.Commands.CreateEvent;
using Shouldly;
using Xunit;

namespace ModularMonolithSample.Event.Application.UnitTests;

public class CreateEventCommandValidatorTests
{
    private readonly CreateEventCommandValidator _validator;

    public CreateEventCommandValidatorTests()
    {
        _validator = new CreateEventCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateEventCommand(
            "Tech Conference 2024",
            "Annual technology conference for developers",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(32),
            "Convention Center Downtown",
            500,
            150.00m
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
    public void Validate_EmptyName_ShouldHaveValidationError(string? name)
    {
        // Arrange
        var command = new CreateEventCommand(
            name,
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            100,
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Event name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', 201); // Exceeds 200 character limit
        var command = new CreateEventCommand(
            longName,
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            100,
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Event name cannot exceed 200 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Validate_EmptyDescription_ShouldHaveValidationError(string? description)
    {
        // Arrange
        var command = new CreateEventCommand(
            "Test Event",
            description,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            100,
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Event description is required");
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longDescription = new string('A', 1001); // Exceeds 1000 character limit
        var command = new CreateEventCommand(
            "Test Event",
            longDescription,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            100,
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Event description cannot exceed 1000 characters");
    }

    [Fact]
    public void Validate_StartDateInPast_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            DateTime.Now.AddDays(-1), // Past date
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            100,
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorMessage("Event start date must be in the future");
    }

    [Fact]
    public void Validate_EndDateBeforeStartDate_ShouldHaveValidationError()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(2);
        var endDate = DateTime.UtcNow.AddDays(1); // Before start date
        
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            startDate,
            endDate,
            "Test Location",
            100,
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorMessage("Event end date must be after start date");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Validate_EmptyLocation_ShouldHaveValidationError(string? location)
    {
        // Arrange
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            location,
            100,
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Event location is required");
    }

    [Fact]
    public void Validate_LocationTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var longLocation = new string('A', 201); // Exceeds 200 character limit
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            longLocation,
            100,
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Event location cannot exceed 200 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_InvalidCapacity_ShouldHaveValidationError(int capacity)
    {
        // Arrange
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            capacity,
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Capacity)
            .WithErrorMessage("Event capacity must be greater than 0");
    }

    [Fact]
    public void Validate_CapacityTooHigh_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            10001, // Exceeds 10,000 limit
            50.00m
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Capacity)
            .WithErrorMessage("Event capacity cannot exceed 10,000");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1.00)]
    [InlineData(-100.00)]
    public void Validate_NegativePrice_ShouldHaveValidationError(decimal price)
    {
        // Arrange
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            100,
            price
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Event price cannot be negative");
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(0.01)]
    [InlineData(100.00)]
    public void Validate_ValidPrice_ShouldNotHaveValidationError(decimal price)
    {
        // Arrange
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            100,
            price
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }
} 
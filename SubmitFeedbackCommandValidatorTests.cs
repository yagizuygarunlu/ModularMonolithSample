using System;
using FluentValidation.TestHelper;
using ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;
using Shouldly;
using Xunit;

namespace ModularMonolithSample.Feedback.Application.UnitTests;

public class SubmitFeedbackCommandValidatorTests
{
    private readonly SubmitFeedbackCommandValidator _validator;

    public SubmitFeedbackCommandValidatorTests()
    {
        _validator = new SubmitFeedbackCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), 5, "Great event!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEventId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.Empty, Guid.NewGuid(), 5, "Great event!");

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
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.Empty, 5, "Great event!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AttendeeId)
            .WithErrorMessage("Attendee ID is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public void Validate_InvalidRating_ShouldHaveValidationError(int rating)
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), rating, "Great event!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rating)
            .WithErrorMessage("Rating must be between 1 and 5");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Validate_ValidRating_ShouldNotHaveValidationError(int rating)
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), rating, "Great event!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Validate_EmptyComment_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), 5, "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Comment)
            .WithErrorMessage("Comment is required");
    }

    [Fact]
    public void Validate_NullComment_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), 5, null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Comment)
            .WithErrorMessage("Comment is required");
    }

    [Fact]
    public void Validate_WhitespaceComment_ShouldHaveValidationError()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), 5, "   ");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Comment)
            .WithErrorMessage("Comment is required");
    }

    [Fact]
    public void Validate_CommentExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longComment = new string('A', 1001); // 1001 characters
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), 5, longComment);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Comment)
            .WithErrorMessage("Comment cannot exceed 1000 characters");
    }

    [Fact]
    public void Validate_CommentAtMaxLength_ShouldNotHaveValidationError()
    {
        // Arrange
        var maxLengthComment = new string('A', 1000); // Exactly 1000 characters
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), 5, maxLengthComment);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Validate_BothIdsEmpty_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.Empty, Guid.Empty, 5, "Great event!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventId)
            .WithErrorMessage("Event ID is required");
        result.ShouldHaveValidationErrorFor(x => x.AttendeeId)
            .WithErrorMessage("Attendee ID is required");
    }

    [Fact]
    public void Validate_AllInvalidValues_ShouldHaveMultipleValidationErrors()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.Empty, Guid.Empty, 0, "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EventId);
        result.ShouldHaveValidationErrorFor(x => x.AttendeeId);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
        result.ShouldHaveValidationErrorFor(x => x.Comment);
    }

    [Theory]
    [InlineData("Valid comment")]
    [InlineData("A")]
    [InlineData("This is a longer comment that should still be valid as long as it's under 1000 characters")]
    public void Validate_ValidComments_ShouldNotHaveValidationError(string comment)
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), 5, comment);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Validate_ValidCommandWithSpecialCharacters_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            3, 
            "Great event! 🎉 Had a wonderful time. Some feedback: improve the food & drinks.");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommandWithMinimumValues_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), 1, "X");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ValidCommandWithMaximumValues_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var maxComment = new string('X', 1000);
        var command = new SubmitFeedbackCommand(Guid.NewGuid(), Guid.NewGuid(), 5, maxComment);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
        result.IsValid.ShouldBeTrue();
    }
} 
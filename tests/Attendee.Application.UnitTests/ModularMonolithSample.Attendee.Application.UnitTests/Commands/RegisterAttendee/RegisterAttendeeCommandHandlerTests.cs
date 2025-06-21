using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using ModularMonolithSample.Attendee.Domain;
using ModularMonolithSample.Attendee.Domain.Events;
using ModularMonolithSample.Event.Domain;
using NSubstitute;
using Shouldly;
using Xunit;
using AttendeeEntity = ModularMonolithSample.Attendee.Domain.Attendee;
using EventEntity = ModularMonolithSample.Event.Domain.Event;

namespace ModularMonolithSample.Attendee.Application.UnitTests.Commands.RegisterAttendee;

public class RegisterAttendeeCommandHandlerTests
{
    private readonly IAttendeeRepository _attendeeRepository;
    private readonly IEventRepository _eventRepository;
    private readonly RegisterAttendeeCommandHandler _handler;

    public RegisterAttendeeCommandHandlerTests()
    {
        _attendeeRepository = Substitute.For<IAttendeeRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _handler = new RegisterAttendeeCommandHandler(_attendeeRepository, _eventRepository);
    }

    [Fact]
    public async Task Handle_Should_Return_New_Attendee_Id()
    {
        // Arrange
        var mockEvent = new EventEntity(
            "Test Event",
            "A test event",
            DateTime.Now,
            DateTime.Now.AddHours(2),
            "Test Location",
            100,
            50m
        );
        var command = new RegisterAttendeeCommand("Test User", "test@test.com", mockEvent.Id);

        _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>()).Returns(mockEvent);
        _attendeeRepository.GetAttendeeCountForEventAsync(command.EventId, Arg.Any<CancellationToken>()).Returns(10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        await _attendeeRepository.Received(1).AddAsync(Arg.Is<AttendeeEntity>(a => a.Email == command.Email), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperationException_When_Event_Not_Found()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var command = new RegisterAttendeeCommand("Test User", "test@test.com", eventId);
        _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>()).Returns((EventEntity)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_Should_Throw_InvalidOperationException_When_Event_Is_At_Capacity()
    {
        // Arrange
        var mockEvent = new EventEntity(
            "Test Event",
            "A test event",
            DateTime.Now,
            DateTime.Now.AddHours(2),
            "Test Location",
            10,
            50m
        );
        var command = new RegisterAttendeeCommand("Test User", "test@test.com", mockEvent.Id);

        _eventRepository.GetByIdAsync(command.EventId, Arg.Any<CancellationToken>()).Returns(mockEvent);
        _attendeeRepository.GetAttendeeCountForEventAsync(command.EventId, Arg.Any<CancellationToken>()).Returns(10);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
} 
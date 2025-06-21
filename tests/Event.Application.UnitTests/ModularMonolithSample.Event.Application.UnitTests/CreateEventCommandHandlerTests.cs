using System;
using System.Threading;
using System.Threading.Tasks;
using ModularMonolithSample.Event.Application.Commands.CreateEvent;
using ModularMonolithSample.Event.Domain;
using NSubstitute;
using Shouldly;
using Xunit;
using EventEntity = ModularMonolithSample.Event.Domain.Event;

namespace ModularMonolithSample.Event.Application.UnitTests;

public class CreateEventCommandHandlerTests
{
    private readonly IEventRepository _eventRepository;
    private readonly CreateEventCommandHandler _handler;

    public CreateEventCommandHandlerTests()
    {
        _eventRepository = Substitute.For<IEventRepository>();
        _handler = new CreateEventCommandHandler(_eventRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateEventSuccessfully()
    {
        // Arrange
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            100,
            50.00m
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        await _eventRepository.Received(1).AddAsync(Arg.Any<EventEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPassCorrectValuesToRepository()
    {
        // Arrange
        var eventName = "Tech Conference 2024";
        var description = "Annual technology conference";
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = DateTime.UtcNow.AddDays(32);
        var location = "Convention Center";
        var capacity = 500;
        var price = 150.00m;

        var command = new CreateEventCommand(
            eventName,
            description,
            startDate,
            endDate,
            location,
            capacity,
            price
        );

        EventEntity? capturedEvent = null;
        await _eventRepository.AddAsync(Arg.Do<EventEntity>(e => capturedEvent = e), Arg.Any<CancellationToken>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedEvent.ShouldNotBeNull();
        capturedEvent.Name.ShouldBe(eventName);
        capturedEvent.Description.ShouldBe(description);
        capturedEvent.StartDate.ShouldBe(startDate);
        capturedEvent.EndDate.ShouldBe(endDate);
        capturedEvent.Location.ShouldBe(location);
        capturedEvent.Capacity.ShouldBe(capacity);
        capturedEvent.Price.ShouldBe(price);
        capturedEvent.Id.ShouldBe(result);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateEventCommand(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2),
            "Test Location",
            100,
            50.00m
        );

        _eventRepository.When(x => x.AddAsync(Arg.Any<EventEntity>(), Arg.Any<CancellationToken>()))
            .Do(x => throw new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe("Database error");
    }
} 
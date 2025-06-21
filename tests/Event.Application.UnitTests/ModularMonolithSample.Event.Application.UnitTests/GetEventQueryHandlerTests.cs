using System;
using System.Threading;
using System.Threading.Tasks;
using ModularMonolithSample.Event.Application.Queries.GetEvent;
using ModularMonolithSample.Event.Domain;
using NSubstitute;
using Shouldly;
using Xunit;
using EventEntity = ModularMonolithSample.Event.Domain.Event;

namespace ModularMonolithSample.Event.Application.UnitTests;

public class GetEventQueryHandlerTests
{
    private readonly IEventRepository _eventRepository;
    private readonly GetEventQueryHandler _handler;

    public GetEventQueryHandlerTests()
    {
        _eventRepository = Substitute.For<IEventRepository>();
        _handler = new GetEventQueryHandler(_eventRepository);
    }

    [Fact]
    public async Task Handle_ExistingEvent_ShouldReturnEventDto()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new EventEntity(
            "Tech Conference",
            "Annual tech event",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(32),
            "Convention Center",
            500,
            100.00m
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);

        var query = new GetEventQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(eventEntity.Id);
        result.Name.ShouldBe(eventEntity.Name);
        result.Description.ShouldBe(eventEntity.Description);
        result.StartDate.ShouldBe(eventEntity.StartDate);
        result.EndDate.ShouldBe(eventEntity.EndDate);
        result.Location.ShouldBe(eventEntity.Location);
        result.Capacity.ShouldBe(eventEntity.Capacity);
        result.Price.ShouldBe(eventEntity.Price);
    }

    [Fact]
    public async Task Handle_NonExistingEvent_ShouldReturnNull()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns((EventEntity?)null);

        var query = new GetEventQuery(eventId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
        await _eventRepository.Received(1).GetByIdAsync(eventId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns<EventEntity?>(_ => throw new InvalidOperationException("Database connection failed"));

        var query = new GetEventQuery(eventId);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(query, CancellationToken.None)
        );
        
        exception.Message.ShouldBe("Database connection failed");
    }
} 
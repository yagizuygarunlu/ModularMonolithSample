using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ModularMonolithSample.Attendee.Application.Queries.GetAttendee;
using ModularMonolithSample.Attendee.Domain;
using NSubstitute;
using Xunit;
using AttendeeEntity = ModularMonolithSample.Attendee.Domain.Attendee;

namespace ModularMonolithSample.Attendee.Application.UnitTests.Queries.GetAttendee;

public class GetAttendeeQueryHandlerTests
{
    private readonly IAttendeeRepository _attendeeRepository;
    private readonly GetAttendeeQueryHandler _handler;

    public GetAttendeeQueryHandlerTests()
    {
        _attendeeRepository = Substitute.For<IAttendeeRepository>();
        _handler = new GetAttendeeQueryHandler(_attendeeRepository);
    }

    [Fact]
    public async Task Handle_Should_Return_AttendeeDto()
    {
        // Arrange
        var attendeeId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var query = new GetAttendeeQuery(attendeeId);
        var attendee = new AttendeeEntity("Test User", "test@test.com", eventId);

        _attendeeRepository.GetByIdAsync(attendeeId, CancellationToken.None).Returns(attendee);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(attendee.Id);
        result.Name.Should().Be(attendee.Name);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Attendee_Not_Found()
    {
        // Arrange
        var attendeeId = Guid.NewGuid();
        var query = new GetAttendeeQuery(attendeeId);
        _attendeeRepository.GetByIdAsync(attendeeId, CancellationToken.None).Returns((AttendeeEntity)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
} 
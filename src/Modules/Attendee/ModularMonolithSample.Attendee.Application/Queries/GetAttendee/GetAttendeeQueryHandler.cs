using MediatR;
using ModularMonolithSample.Attendee.Domain;

namespace ModularMonolithSample.Attendee.Application.Queries.GetAttendee;

public class GetAttendeeQueryHandler : IRequestHandler<GetAttendeeQuery, AttendeeDto?>
{
    private readonly IAttendeeRepository _attendeeRepository;

    public GetAttendeeQueryHandler(IAttendeeRepository attendeeRepository)
    {
        _attendeeRepository = attendeeRepository;
    }

    public async Task<AttendeeDto?> Handle(GetAttendeeQuery request, CancellationToken cancellationToken)
    {
        var attendee = await _attendeeRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (attendee == null)
            return null;

        return new AttendeeDto(
            attendee.Id,
            attendee.Name,
            attendee.Email,
            attendee.EventId,
            attendee.RegistrationDate);
    }
} 
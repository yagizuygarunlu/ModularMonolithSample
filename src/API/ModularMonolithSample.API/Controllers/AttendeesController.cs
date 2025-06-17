using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;

namespace ModularMonolithSample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> RegisterAttendee(RegisterAttendeeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var attendeeId = await _mediator.Send(command, cancellationToken);
            return Ok(attendeeId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 
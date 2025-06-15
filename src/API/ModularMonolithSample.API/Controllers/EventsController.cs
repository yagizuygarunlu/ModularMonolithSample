using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.Event.Application.Commands.CreateEvent;

namespace ModularMonolithSample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateEvent(CreateEventCommand command, CancellationToken cancellationToken)
    {
        var eventId = await _mediator.Send(command, cancellationToken);
        return Ok(eventId);
    }
} 
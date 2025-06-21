using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.Event.Application.Commands.CreateEvent;
using ModularMonolithSample.Event.Application.Queries.GetEvent;
using ModularMonolithSample.BuildingBlocks.Exceptions;
using ModularMonolithSample.BuildingBlocks.Controllers;
using ModularMonolithSample.BuildingBlocks.Models;

namespace ModularMonolithSample.API.Controllers;

public class EventsController : BaseApiController
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<EventDto>>> GetEvent(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetEventQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
        {
            return NotFound<EventDto>($"Event with ID {id} was not found.");
        }

        return Success(result, "Event retrieved successfully");
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateEvent(CreateEventCommand command, CancellationToken cancellationToken)
    {
        var eventId = await _mediator.Send(command, cancellationToken);
        return Created(eventId, "Event created successfully");
    }
} 
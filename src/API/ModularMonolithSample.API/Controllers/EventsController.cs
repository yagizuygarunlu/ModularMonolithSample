using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.Event.Application.Commands.CreateEvent;
using ModularMonolithSample.Event.Application.Queries.GetEvent;

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

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetEventQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
            return NotFound($"Event with ID {id} not found.");
            
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateEvent(CreateEventCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var eventId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetEvent), new { id = eventId }, eventId);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
} 
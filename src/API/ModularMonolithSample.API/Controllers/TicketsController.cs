using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.Ticket.Application.Commands.IssueTicket;

namespace ModularMonolithSample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> IssueTicket(IssueTicketCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var ticketId = await _mediator.Send(command, cancellationToken);
            return Ok(ticketId);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
} 
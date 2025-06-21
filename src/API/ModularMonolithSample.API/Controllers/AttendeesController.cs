using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using ModularMonolithSample.BuildingBlocks.Controllers;
using ModularMonolithSample.BuildingBlocks.Models;

namespace ModularMonolithSample.API.Controllers;

public class AttendeesController : BaseApiController
{
    private readonly IMediator _mediator;

    public AttendeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<Guid>>> RegisterAttendee(RegisterAttendeeCommand command, CancellationToken cancellationToken)
    {
        var attendeeId = await _mediator.Send(command, cancellationToken);
        return Created(attendeeId, "Attendee registered successfully");
    }
} 
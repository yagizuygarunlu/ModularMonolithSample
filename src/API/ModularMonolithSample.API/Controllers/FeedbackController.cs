using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;

namespace ModularMonolithSample.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeedbackController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> SubmitFeedback(SubmitFeedbackCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var feedbackId = await _mediator.Send(command, cancellationToken);
            return Ok(feedbackId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 
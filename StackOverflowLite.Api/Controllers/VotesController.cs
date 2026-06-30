using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Votes.Commands;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Api.Controllers;

[ApiController]
[Route("api/votes")]
[Authorize]
public class VotesController : ControllerBase
{
    private readonly IMediator _mediator;
    public VotesController(IMediator mediator) => _mediator = mediator;

    /// Cast or toggle a vote on a question or answer
    /// Calling again with same vote type removes the vote
    /// Calling with opposite type switches direction
    /// 
    [HttpPost]
    public async Task<IActionResult> Vote([FromBody] VoteRequest req)
    {
        if (!Enum.TryParse<VoteType>(req.VoteType, true, out var voteType))
            return BadRequest(new { error = "VoteType must be 'Upvote' or 'Downvote'." });

        if (!Enum.TryParse<VoteTarget>(req.Target, true, out var target))
            return BadRequest(new { error = "Target must be 'Question' or 'Answer'." });

        var result = await _mediator.Send(new CastVoteCommand(target, req.TargetId, voteType));
        if (!result.Succeeded) return BadRequest(new { error = result.Error });

        return Ok(new { newScore = result.Data });
    }
}

public record VoteRequest(string Target, Guid TargetId, string VoteType);

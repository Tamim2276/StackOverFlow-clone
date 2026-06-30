using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Answers.Commands;

namespace StackOverflowLite.Api.Controllers;

[ApiController]
[Route("api/questions/{questionId:guid}/answers")]
public class AnswersController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnswersController(IMediator mediator) => _mediator = mediator;

    ///Post an answer to a question
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Post(Guid questionId, [FromBody] PostAnswerRequest req)
    {
        var result = await _mediator.Send(new PostAnswerCommand(questionId, req.Body));
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return StatusCode(201, new { id = result.Data });
    }

    //Update answer
    [Authorize]
    [HttpPut("{answerId:guid}")]
    public async Task<IActionResult> Update(Guid questionId, Guid answerId, [FromBody] UpdateAnswerRequest req)
    {
        var result = await _mediator.Send(new UpdateAnswerCommand(answerId, req.Body));
        if (!result.Succeeded) return result.Error == "Forbidden." ? Forbid() : NotFound(new { error = result.Error });
        return NoContent();
    }

    /// Delete  answer
    [Authorize]
    [HttpDelete("{answerId:guid}")]
    public async Task<IActionResult> Delete(Guid questionId, Guid answerId)
    {
        var result = await _mediator.Send(new DeleteAnswerCommand(answerId));
        if (!result.Succeeded) return result.Error == "Forbidden." ? Forbid() : NotFound(new { error = result.Error });
        return NoContent();
    }

    //Accept an answer. Only the question author can do this
    [Authorize]
    [HttpPost("{answerId:guid}/accept")]
    public async Task<IActionResult> Accept(Guid questionId, Guid answerId)
    {
        var result = await _mediator.Send(new AcceptAnswerCommand(questionId, answerId));
        if (!result.Succeeded) return result.Error!.Contains("Forbidden") || result.Error.Contains("Only")
            ? Forbid() : NotFound(new { error = result.Error });
        return Ok(new { message = "Answer accepted." });
    }
}

public record PostAnswerRequest(string Body);
public record UpdateAnswerRequest(string Body);

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Questions.Commands;
using StackOverflowLite.Application.Features.Questions.Queries;

namespace StackOverflowLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public QuestionsController(IMediator mediator) => _mediator = mediator;

    //List questions with optional pagination, tag filter, and keyword search
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? tag = null,
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetQuestionsQuery(page, pageSize, tag, search));
        return Ok(result.Data);
    }

    //Get a single question with all answers
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetQuestionByIdQuery(id));
        if (!result.Succeeded) return NotFound(new { error = result.Error });
        return Ok(result.Data);
    }

    //Post a new question. Requires authentication.
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequest req)
    {
        var result = await _mediator.Send(new CreateQuestionCommand(req.Title, req.Body, req.Tags));
        if (!result.Succeeded) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, new { id = result.Data });
    }

    //Update question. Author only
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuestionRequest req)
    {
        var result = await _mediator.Send(new UpdateQuestionCommand(id, req.Title, req.Body, req.Tags));
        if (!result.Succeeded) return result.Error == "Forbidden." ? Forbid() : NotFound(new { error = result.Error });
        return NoContent();
    }

    //Delete question. Author only
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteQuestionCommand(id));
        if (!result.Succeeded) return result.Error == "Forbidden." ? Forbid() : NotFound(new { error = result.Error });
        return NoContent();
    }
}

public record CreateQuestionRequest(string Title, string Body, List<string> Tags);
public record UpdateQuestionRequest(string Title, string Body, List<string> Tags);

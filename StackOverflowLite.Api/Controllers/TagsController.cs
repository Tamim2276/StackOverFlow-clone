using MediatR;
using Microsoft.AspNetCore.Mvc;
using StackOverflowLite.Application.Features.Tags.Queries;

namespace StackOverflowLite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TagsController(IMediator mediator) => _mediator = mediator;

    //Get all tags ordered by usage count
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllTagsQuery());
        return Ok(result.Data);
    }
}

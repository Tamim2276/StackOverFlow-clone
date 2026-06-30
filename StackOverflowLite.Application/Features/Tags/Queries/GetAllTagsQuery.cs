using MediatR;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Application.Common.Models;

namespace StackOverflowLite.Application.Features.Tags.Queries;

public record TagDto(Guid Id, string Name, int QuestionCount);

public record GetAllTagsQuery : IRequest<Result<List<TagDto>>>;

public class GetAllTagsQueryHandler : IRequestHandler<GetAllTagsQuery, Result<List<TagDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAllTagsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<List<TagDto>>> Handle(GetAllTagsQuery request, CancellationToken ct)
    {
        var tags = await _db.Tags
            .Include(t => t.QuestionTags)
            .OrderByDescending(t => t.QuestionTags.Count)
            .Select(t => new TagDto(t.Id, t.Name, t.QuestionTags.Count))
            .ToListAsync(ct);

        return Result<List<TagDto>>.Success(tags);
    }
}

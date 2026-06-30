using MediatR;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Application.Common.Models;
using StackOverflowLite.Application.Features.Questions;

namespace StackOverflowLite.Application.Features.Questions.Queries;

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public record GetQuestionsQuery(int Page = 1, int PageSize = 10, string? Tag = null, string? Search = null)
    : IRequest<Result<PagedResult<QuestionSummaryDto>>>;

public class GetQuestionsQueryHandler : IRequestHandler<GetQuestionsQuery, Result<PagedResult<QuestionSummaryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetQuestionsQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<PagedResult<QuestionSummaryDto>>> Handle(GetQuestionsQuery request, CancellationToken ct)
    {
        var query = _db.Questions
            .Include(q => q.Author)
            .Include(q => q.Answers)
            .Include(q => q.Votes)
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Tag))
            query = query.Where(q => q.QuestionTags.Any(qt => qt.Tag.Name == request.Tag));

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(q => q.Title.Contains(request.Search) || q.Body.Contains(request.Search));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(q => new QuestionSummaryDto(
                q.Id,
                q.Title,
                q.Author.DisplayName,
                q.Votes.Sum(v => (int)v.VoteType),
                q.Answers.Count,
                q.ViewCount,
                q.QuestionTags.Select(qt => qt.Tag.Name).ToList(),
                q.CreatedAt))
            .ToListAsync(ct);

        return Result<PagedResult<QuestionSummaryDto>>.Success(
            new PagedResult<QuestionSummaryDto>(items, total, request.Page, request.PageSize));
    }
}

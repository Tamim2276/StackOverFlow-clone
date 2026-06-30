using MediatR;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Application.Common.Models;

namespace StackOverflowLite.Application.Features.Questions.Queries;

public record GetQuestionByIdQuery(Guid QuestionId) : IRequest<Result<QuestionDetailDto>>;

public class GetQuestionByIdQueryHandler : IRequestHandler<GetQuestionByIdQuery, Result<QuestionDetailDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICacheService _cache;

    public GetQuestionByIdQueryHandler(IApplicationDbContext db, ICacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<Result<QuestionDetailDto>> Handle(GetQuestionByIdQuery request, CancellationToken ct)
    {
        // Increment view count in Redis (non-blocking)
        _ = _cache.IncrementViewAsync($"view:question:{request.QuestionId}", ct);

        var q = await _db.Questions
            .Include(q => q.Author)
            .Include(q => q.Votes)
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Answers).ThenInclude(a => a.Author)
            .Include(q => q.Answers).ThenInclude(a => a.Votes)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct);

        if (q == null)
            return Result<QuestionDetailDto>.Failure("Question not found.");

        var dto = new QuestionDetailDto(
            q.Id,
            q.Title,
            q.Body,
            q.AuthorId,
            q.Author.DisplayName,
            q.Votes.Sum(v => (int)v.VoteType),
            q.ViewCount,
            q.AcceptedAnswerId,
            q.QuestionTags.Select(qt => qt.Tag.Name).ToList(),
            q.Answers.OrderByDescending(a => a.IsAccepted).ThenByDescending(a => a.Votes.Sum(v => (int)v.VoteType))
                .Select(a => new AnswerDto(
                    a.Id, a.Body, a.AuthorId, a.Author.DisplayName,
                    a.Votes.Sum(v => (int)v.VoteType), a.IsAccepted, a.CreatedAt))
                .ToList(),
            q.CreatedAt);

        return Result<QuestionDetailDto>.Success(dto);
    }
}

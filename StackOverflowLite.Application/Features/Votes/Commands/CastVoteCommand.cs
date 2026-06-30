using MediatR;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Application.Common.Models;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Votes.Commands;

public record CastVoteCommand(VoteTarget Target, Guid TargetId, VoteType VoteType) : IRequest<Result<int>>;

public class CastVoteCommandHandler : IRequestHandler<CastVoteCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CastVoteCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(CastVoteCommand request, CancellationToken ct)
    {
        if (_currentUser.UserId == null) return Result<int>.Failure("Unauthorized.");

        // Verify target exists
        if (request.Target == VoteTarget.Question)
        {
            var q = await _db.Questions.FindAsync(new object[] { request.TargetId }, ct);
            if (q == null) return Result<int>.Failure("Question not found.");
            if (q.AuthorId == _currentUser.UserId) return Result<int>.Failure("Cannot vote on your own question.");
        }
        else
        {
            var a = await _db.Answers.FindAsync(new object[] { request.TargetId }, ct);
            if (a == null) return Result<int>.Failure("Answer not found.");
            if (a.AuthorId == _currentUser.UserId) return Result<int>.Failure("Cannot vote on your own answer.");
        }

        // Find existing vote
        var existing = await _db.Votes.FirstOrDefaultAsync(v =>
            v.UserId == _currentUser.UserId &&
            v.Target == request.Target &&
            (request.Target == VoteTarget.Question ? v.QuestionId == request.TargetId : v.AnswerId == request.TargetId),
            ct);

        if (existing != null)
        {
            if (existing.VoteType == request.VoteType)
            {
                // Toggle off (remove vote)
                _db.Votes.Remove(existing);
            }
            else
            {
                // Change vote direction
                existing.VoteType = request.VoteType;
            }
        }
        else
        {
            var vote = new Vote
            {
                UserId = _currentUser.UserId,
                VoteType = request.VoteType,
                Target = request.Target,
                QuestionId = request.Target == VoteTarget.Question ? request.TargetId : null,
                AnswerId = request.Target == VoteTarget.Answer ? request.TargetId : null
            };
            _db.Votes.Add(vote);
        }

        await _db.SaveChangesAsync(ct);

        // Return new score
        var score = await _db.Votes
            .Where(v => v.Target == request.Target &&
                        (request.Target == VoteTarget.Question
                            ? v.QuestionId == request.TargetId
                            : v.AnswerId == request.TargetId))
            .SumAsync(v => (int)v.VoteType, ct);

        return Result<int>.Success(score);
    }
}

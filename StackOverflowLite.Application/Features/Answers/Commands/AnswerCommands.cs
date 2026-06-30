using MediatR;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Application.Common.Models;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Answers.Commands;

// ── Post Answer ──────────────────────────────────────────────────────────────
public record PostAnswerCommand(Guid QuestionId, string Body) : IRequest<Result<Guid>>;

public class PostAnswerCommandHandler : IRequestHandler<PostAnswerCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public PostAnswerCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(PostAnswerCommand request, CancellationToken ct)
    {
        if (_currentUser.UserId == null) return Result<Guid>.Failure("Unauthorized.");

        var question = await _db.Questions.FindAsync(new object[] { request.QuestionId }, ct);
        if (question == null) return Result<Guid>.Failure("Question not found.");

        var answer = new Answer
        {
            Body = request.Body,
            QuestionId = request.QuestionId,
            AuthorId = _currentUser.UserId
        };

        _db.Answers.Add(answer);
        await _db.SaveChangesAsync(ct);
        return Result<Guid>.Success(answer.Id);
    }
}

// ── Update Answer ─────────────────────────────────────────────────────────────
public record UpdateAnswerCommand(Guid AnswerId, string Body) : IRequest<Result>;

public class UpdateAnswerCommandHandler : IRequestHandler<UpdateAnswerCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateAnswerCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateAnswerCommand request, CancellationToken ct)
    {
        var answer = await _db.Answers.FindAsync(new object[] { request.AnswerId }, ct);
        if (answer == null) return Result.Failure("Answer not found.");
        if (answer.AuthorId != _currentUser.UserId) return Result.Failure("Forbidden.");

        answer.Body = request.Body;
        answer.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Delete Answer ─────────────────────────────────────────────────────────────
public record DeleteAnswerCommand(Guid AnswerId) : IRequest<Result>;

public class DeleteAnswerCommandHandler : IRequestHandler<DeleteAnswerCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteAnswerCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteAnswerCommand request, CancellationToken ct)
    {
        var answer = await _db.Answers.FindAsync(new object[] { request.AnswerId }, ct);
        if (answer == null) return Result.Failure("Answer not found.");
        if (answer.AuthorId != _currentUser.UserId) return Result.Failure("Forbidden.");

        _db.Answers.Remove(answer);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// ── Accept Answer ─────────────────────────────────────────────────────────────
public record AcceptAnswerCommand(Guid QuestionId, Guid AnswerId) : IRequest<Result>;

public class AcceptAnswerCommandHandler : IRequestHandler<AcceptAnswerCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AcceptAnswerCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(AcceptAnswerCommand request, CancellationToken ct)
    {
        var question = await _db.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct);

        if (question == null) return Result.Failure("Question not found.");
        if (question.AuthorId != _currentUser.UserId) return Result.Failure("Only the question author can accept an answer.");

        var answer = question.Answers.FirstOrDefault(a => a.Id == request.AnswerId);
        if (answer == null) return Result.Failure("Answer not found for this question.");

        // Unaccept previous
        foreach (var a in question.Answers)
            a.IsAccepted = false;

        answer.IsAccepted = true;
        question.AcceptedAnswerId = answer.Id;

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

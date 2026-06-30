using MediatR;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Application.Common.Models;

namespace StackOverflowLite.Application.Features.Questions.Commands;

public record DeleteQuestionCommand(Guid QuestionId) : IRequest<Result>;

public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteQuestionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteQuestionCommand request, CancellationToken ct)
    {
        var question = await _db.Questions.FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct);
        if (question == null) return Result.Failure("Question not found.");
        if (question.AuthorId != _currentUser.UserId) return Result.Failure("Forbidden.");

        _db.Questions.Remove(question);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

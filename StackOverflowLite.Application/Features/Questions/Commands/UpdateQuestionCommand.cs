using MediatR;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Application.Common.Models;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Questions.Commands;

public record UpdateQuestionCommand(Guid QuestionId, string Title, string Body, List<string> Tags) : IRequest<Result>;

public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateQuestionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateQuestionCommand request, CancellationToken ct)
    {
        var question = await _db.Questions
            .Include(q => q.QuestionTags)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct);

        if (question == null) return Result.Failure("Question not found.");
        if (question.AuthorId != _currentUser.UserId) return Result.Failure("Forbidden.");

        question.Title = request.Title;
        question.Body = request.Body;
        question.UpdatedAt = DateTime.UtcNow;

        // Replace tags
        foreach (var qt in question.QuestionTags.ToList())
            _db.QuestionTags.Remove(qt);

        foreach (var tagName in request.Tags.Distinct())
        {
            var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == tagName.ToLower(), ct)
                      ?? new Tag { Name = tagName.ToLower() };
            if (tag.Id == Guid.Empty) { tag.Id = Guid.NewGuid(); _db.Tags.Add(tag); }
            _db.QuestionTags.Add(new QuestionTag { QuestionId = question.Id, TagId = tag.Id });
        }

        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

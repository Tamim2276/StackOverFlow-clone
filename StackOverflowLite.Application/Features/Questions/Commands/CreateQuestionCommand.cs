using MediatR;
using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Application.Common.Interfaces;
using StackOverflowLite.Application.Common.Models;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Questions.Commands;

public record CreateQuestionCommand(string Title, string Body, List<string> Tags) : IRequest<Result<Guid>>;

public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateQuestionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateQuestionCommand request, CancellationToken ct)
    {
        if (_currentUser.UserId == null)
            return Result<Guid>.Failure("Unauthorized.");

        var question = new Question
        {
            Title = request.Title,
            Body = request.Body,
            AuthorId = _currentUser.UserId
        };

        _db.Questions.Add(question);

        foreach (var tagName in request.Tags.Distinct())
        {
            var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == tagName.ToLower(), ct)
                      ?? new Tag { Name = tagName.ToLower() };

            if (tag.Id == Guid.Empty)
            {
                tag.Id = Guid.NewGuid();
                _db.Tags.Add(tag);
            }

            _db.QuestionTags.Add(new QuestionTag { QuestionId = question.Id, TagId = tag.Id });
        }

        await _db.SaveChangesAsync(ct);
        return Result<Guid>.Success(question.Id);
    }
}

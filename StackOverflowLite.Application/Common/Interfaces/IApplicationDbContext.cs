using Microsoft.EntityFrameworkCore;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Question> Questions { get; }
    DbSet<Answer> Answers { get; }
    DbSet<Vote> Votes { get; }
    DbSet<Tag> Tags { get; }
    DbSet<QuestionTag> QuestionTags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

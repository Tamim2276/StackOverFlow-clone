using StackOverflowLite.Domain.Common;

namespace StackOverflowLite.Domain.Entities;

public class Question : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public int ViewCount { get; set; } = 0;
    public Guid? AcceptedAnswerId { get; set; }

    public ApplicationUser Author { get; set; } = null!;
    public Answer? AcceptedAnswer { get; set; }
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
}

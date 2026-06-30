using StackOverflowLite.Domain.Common;

namespace StackOverflowLite.Domain.Entities;

public class Answer : BaseEntity
{
    public string Body { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public Guid QuestionId { get; set; }
    public bool IsAccepted { get; set; } = false;

    public ApplicationUser Author { get; set; } = null!;
    public Question Question { get; set; } = null!;
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}

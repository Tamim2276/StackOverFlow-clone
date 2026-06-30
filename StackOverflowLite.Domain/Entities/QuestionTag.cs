namespace StackOverflowLite.Domain.Entities;

public class QuestionTag
{
    public Guid QuestionId { get; set; }
    public Guid TagId { get; set; }

    public Question Question { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}

using StackOverflowLite.Domain.Common;

namespace StackOverflowLite.Domain.Entities;

public enum VoteType { Upvote = 1, Downvote = -1 }
public enum VoteTarget { Question, Answer }

public class Vote : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public VoteType VoteType { get; set; }
    public VoteTarget Target { get; set; }
    public Guid? QuestionId { get; set; }
    public Guid? AnswerId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Question? Question { get; set; }
    public Answer? Answer { get; set; }
}

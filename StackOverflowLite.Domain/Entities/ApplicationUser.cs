using Microsoft.AspNetCore.Identity;

namespace StackOverflowLite.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public int Reputation { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}

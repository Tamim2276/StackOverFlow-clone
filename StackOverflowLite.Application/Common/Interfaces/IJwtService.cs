using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}

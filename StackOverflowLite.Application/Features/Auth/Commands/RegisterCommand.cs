using MediatR;
using Microsoft.AspNetCore.Identity;
using StackOverflowLite.Application.Common.Models;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Auth.Commands;

public record RegisterCommand(string Email, string UserName, string Password, string DisplayName)
    : IRequest<Result<string>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtServiceRef _jwtService;

    public RegisterCommandHandler(UserManager<ApplicationUser> userManager, IJwtServiceRef jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return Result<string>.Failure("Email already in use.");

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.UserName,
            DisplayName = request.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result<string>.Failure(string.Join("; ", result.Errors.Select(e => e.Description)));

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);
        return Result<string>.Success(token);
    }
}

// Ref interface to avoid circular dependency at compile time — replaced in DI
public interface IJwtServiceRef
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}

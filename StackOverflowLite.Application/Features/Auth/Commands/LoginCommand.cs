using MediatR;
using Microsoft.AspNetCore.Identity;
using StackOverflowLite.Application.Common.Models;
using StackOverflowLite.Application.Features.Auth.Commands;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<Result<string>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtServiceRef _jwtService;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtServiceRef jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<string>.Failure("Invalid credentials.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Result<string>.Failure("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);
        return Result<string>.Success(token);
    }
}

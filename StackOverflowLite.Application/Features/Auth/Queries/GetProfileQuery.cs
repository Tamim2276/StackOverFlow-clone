using MediatR;
using Microsoft.AspNetCore.Identity;
using StackOverflowLite.Application.Common.Models;
using StackOverflowLite.Domain.Entities;

namespace StackOverflowLite.Application.Features.Auth.Queries;

public record ProfileDto(string Id, string Email, string UserName, string DisplayName, int Reputation, DateTime MemberSince);

public record GetProfileQuery(string UserId) : IRequest<Result<ProfileDto>>;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<ProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetProfileQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<ProfileDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result<ProfileDto>.Failure("User not found.");

        return Result<ProfileDto>.Success(new ProfileDto(
            user.Id, user.Email!, user.UserName!, user.DisplayName, user.Reputation, user.CreatedAt));
    }
}

using Tripcare360.Application.Dtos.Auth;
using Tripcare360.Domain.Entities.Admin;

namespace Tripcare360.Application.Features.Auth;

public static class AuthMapper
{
    public static AdminLoginResponse ToAdminLoginResponse(
        this AdminUserEntity user, string token, DateTimeOffset expiresAt)
        => new(token, user.Email, user.Role, expiresAt);
}

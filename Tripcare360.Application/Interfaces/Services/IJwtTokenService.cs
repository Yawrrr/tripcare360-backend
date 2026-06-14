using Tripcare360.Domain.Entities.Admin;

namespace Tripcare360.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(AdminUserEntity user, out DateTimeOffset expiresAt);
}

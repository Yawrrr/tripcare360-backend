using Microsoft.AspNetCore.Identity;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Admin;

namespace Tripcare360.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<AdminUserEntity> _hasher = new();
    private readonly AdminUserEntity _dummy = new() { Email = "", PasswordHash = "" };

    public string Hash(string password)
        => _hasher.HashPassword(_dummy, password);

    public bool Verify(string hashedPassword, string providedPassword)
    {
        _dummy.PasswordHash = hashedPassword;
        var result = _hasher.VerifyHashedPassword(_dummy, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}

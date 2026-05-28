using Microsoft.Extensions.Configuration;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Services;

public class AdminCredentials(IConfiguration config) : IAdminCredentials
{
    public string Username { get; } = config["AdminCredentials:Username"] ?? string.Empty;
    public string Password { get; } = config["AdminCredentials:Password"] ?? string.Empty;
}

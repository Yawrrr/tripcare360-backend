namespace Tripcare360.Application.Dtos.Auth;

public record AdminLoginResponse(string Token, string Email, string Role, DateTimeOffset ExpiresAt);

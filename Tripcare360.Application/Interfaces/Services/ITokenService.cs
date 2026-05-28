namespace Tripcare360.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(string username);
}

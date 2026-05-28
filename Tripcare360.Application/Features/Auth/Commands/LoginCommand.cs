using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Auth;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Errors;

namespace Tripcare360.Application.Features.Auth.Commands;

public class LoginCommand(LoginRequest request) : IRequest<LoginResponse>
{
    public LoginRequest Request { get; } = request;

    public class Validator : AbstractValidator<LoginCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Request.Username).NotEmpty();
            RuleFor(c => c.Request.Password).NotEmpty();
        }
    }

    public class Handler(IAdminCredentials adminCredentials, ITokenService tokenService)
        : IRequestHandler<LoginCommand, LoginResponse>
    {
        public Task<LoginResponse> Handle(LoginCommand command, CancellationToken ct)
        {
            var req = command.Request;

            var usernameMatch = string.Equals(req.Username, adminCredentials.Username, StringComparison.OrdinalIgnoreCase);
            var passwordMatch = string.Equals(req.Password, adminCredentials.Password, StringComparison.Ordinal);

            if (!usernameMatch || !passwordMatch)
                throw new ApiException(ErrorCode.InvalidCredentials);

            var token = tokenService.GenerateToken(req.Username);
            return Task.FromResult(new LoginResponse(token));
        }
    }
}

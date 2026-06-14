using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Auth;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Errors;

namespace Tripcare360.Application.Features.Auth.Commands;

public class AdminLoginCommand(AdminLoginRequest request) : IRequest<AdminLoginResponse>
{
    public AdminLoginRequest Request { get; } = request;

    public class Validator : AbstractValidator<AdminLoginCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Request.Email).NotEmpty().EmailAddress();
            RuleFor(c => c.Request.Password).NotEmpty().MinimumLength(8);
        }
    }

    public class Handler(
        IAdminUserRepository repo,
        IJwtTokenService jwtService,
        IPasswordHasher hasher)
        : IRequestHandler<AdminLoginCommand, AdminLoginResponse>
    {
        public async Task<AdminLoginResponse> Handle(
            AdminLoginCommand command, CancellationToken ct)
        {
            var user = await repo.GetByEmailAsync(command.Request.Email, ct);
            if (user is null) throw new ApiException(ErrorCode.InvalidCredentials);
            if (!user.IsActive) throw new ApiException(ErrorCode.AccountDisabled);

            if (!hasher.Verify(user.PasswordHash, command.Request.Password))
                throw new ApiException(ErrorCode.InvalidCredentials);

            var token = jwtService.GenerateToken(user, out var expiresAt);
            return user.ToAdminLoginResponse(token, expiresAt);
        }
    }
}

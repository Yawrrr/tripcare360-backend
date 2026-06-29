using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Features.Claim.Events;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Mappers;
using Tripcare360.Domain.Entities.Errors;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Claim.Commands;

public class FinalizeClaimCommand(FinalizeClaimRequest request) : IRequest<FinalizeClaimResponse>
{
    public FinalizeClaimRequest Request { get; } = request;

    public class Validator : AbstractValidator<FinalizeClaimCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Request.ClaimCode).NotEmpty();
            RuleFor(c => c.Request.AgreesToTermsAndConditions).Equal(true)
                .WithMessage("Terms and conditions must be accepted.");
        }
    }

    public class Handler(IClaimRepository claimRepository, IServiceScopeFactory scopeFactory)
        : IRequestHandler<FinalizeClaimCommand, FinalizeClaimResponse>
    {
        private static readonly TimeSpan ReservationWindow = TimeSpan.FromMinutes(10);

        public async Task<FinalizeClaimResponse> Handle(
            FinalizeClaimCommand command, CancellationToken cancellationToken)
        {
            var req = command.Request;
            var claim = await claimRepository.GetByClaimCodeAsync(req.ClaimCode);

            if (claim is null || claim.Status != ClaimStatus.Pending)
                throw new ApiException(ErrorCode.ClaimNotFound);

            if (DateTimeOffset.UtcNow - claim.CreatedAt > ReservationWindow)
                throw new ApiException(ErrorCode.ClaimExpired);

            claim.Status = ClaimStatus.Submitted;
            claim.ProcessedAt = DateTimeOffset.UtcNow;

            await claimRepository.UpdateAsync(claim);

            var claimCode = claim.ClaimCode;
            _ = Task.Run(async () =>
            {
                using var scope = scopeFactory.CreateScope();
                var pub = scope.ServiceProvider.GetRequiredService<IPublisher>();
                await pub.Publish(new ClaimFinalizedNotification(claimCode), CancellationToken.None);
            });

            return claim.ToFinalizeResponse();
        }
    }
}

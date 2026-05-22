using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Admin;
using Tripcare360.Application.Interfaces.Repositories;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Application.Mappers;
using Tripcare360.Domain.Entities.Errors;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Claim.Commands;

public class UpdateClaimStatusCommand(string claimCode, UpdateClaimStatusRequest request)
    : IRequest<UpdateClaimStatusResponse>
{
    public string ClaimCode { get; } = claimCode;
    public UpdateClaimStatusRequest Request { get; } = request;

    public class Validator : AbstractValidator<UpdateClaimStatusCommand>
    {
        public Validator()
        {
            RuleFor(c => c.ClaimCode).NotEmpty();
            RuleFor(c => c.Request.Action)
                .Must(a => a == "Approve" || a == "Reject")
                .WithMessage("Action must be 'Approve' or 'Reject'.");
        }
    }

    public class Handler(
        IClaimRepository repo,
        ISseEventBroadcaster sseBroadcaster)
        : IRequestHandler<UpdateClaimStatusCommand, UpdateClaimStatusResponse>
    {
        private static readonly HashSet<ClaimStatus> TerminalStatuses =
            [ClaimStatus.StpApproved, ClaimStatus.Rejected, ClaimStatus.AdminApproved];

        public async Task<UpdateClaimStatusResponse> Handle(
            UpdateClaimStatusCommand command, CancellationToken ct)
        {
            var claim = await repo.GetByClaimCodeAsync(command.ClaimCode);
            if (claim is null) throw new ApiException(ErrorCode.ClaimNotFound);

            if (TerminalStatuses.Contains(claim.Status))
                throw new ApiException(ErrorCode.ClaimAlreadyProcessed);

            claim.Status = command.Request.Action == "Approve"
                ? ClaimStatus.AdminApproved
                : ClaimStatus.Rejected;

            claim.AdminComments = command.Request.AdminComments;
            claim.ProcessedAt = DateTime.UtcNow;

            await repo.UpdateAsync(claim);

            await sseBroadcaster.BroadcastStateAsync(
                claim.ClaimCode,
                claim.Status.ToString(),
                new { claimCode = claim.ClaimCode, status = claim.Status.ToString() });

            return claim.ToUpdateStatusResponse();
        }
    }
}

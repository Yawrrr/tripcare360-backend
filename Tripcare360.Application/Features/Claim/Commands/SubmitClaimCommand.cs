using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Application.Mappers;
using Tripcare360.Domain.Entities.Claim;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Application.Features.Claim.Commands;

public class SubmitClaimCommand(SubmitClaimRequest request) : IRequest<SubmitClaimResponse>
{
    public SubmitClaimRequest Request { get; } = request;

    public class Validator : AbstractValidator<SubmitClaimCommand>
    {
        public Validator()
        {
            // Add validation rules here
        }
    }

    public class Handler(IRojakkkService rojakkkService)
        : IRequestHandler<SubmitClaimCommand, SubmitClaimResponse>
    {
        public async Task<SubmitClaimResponse> Handle(
            SubmitClaimCommand command, CancellationToken cancellationToken)
        {
            var req = command.Request;
            bool isDelayed = await rojakkkService.VerifyFlightDelayAsync(req.FlightNumber, req.FlightDate);

            var claim = new ClaimEntity
            {
                PolicyNumber = req.PolicyNumber,
                IdentityNumber = req.IdentityNumber,
                FlightNumber = req.FlightNumber,
                Type = req.ClaimType,
                Status = isDelayed ? ClaimStatus.StpApproved : ClaimStatus.ManualReview
            };

            return claim.ToResponse();
        }
    }
}

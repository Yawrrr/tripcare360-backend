using FluentValidation;
using MediatR;
using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Errors;

namespace Tripcare360.Application.Features.Policy.Commands;

public class VerifyPolicyCommand(VerifyPolicyRequest request) : IRequest<VerifyPolicyResponse>
{
    public VerifyPolicyRequest Request { get; } = request;

    public class Validator : AbstractValidator<VerifyPolicyCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Request.PolicyNumber).NotEmpty();
            RuleFor(c => c.Request.IdentityNumber).NotEmpty();
        }
    }

    public class Handler(IEtiqaBackendClient etiqaClient)
        : IRequestHandler<VerifyPolicyCommand, VerifyPolicyResponse>
    {
        public async Task<VerifyPolicyResponse> Handle(
            VerifyPolicyCommand command, CancellationToken cancellationToken)
        {
            var req = command.Request;
            var policy = await etiqaClient.VerifyPolicyAsync(req.PolicyNumber, req.IdentityNumber);

            if (policy is null)
                throw new ApiException(ErrorCode.PolicyNotFound);

            return policy;
        }
    }
}

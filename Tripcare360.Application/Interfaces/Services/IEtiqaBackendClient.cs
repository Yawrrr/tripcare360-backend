using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Dtos.Policy;

namespace Tripcare360.Application.Interfaces.Services;

public interface IEtiqaBackendClient
{
    Task<VerifyPolicyResponse?> VerifyPolicyAsync(string policyNumber, string identityNumber);

    Task<string?> ProcessClaimDocumentsAsync(string claimType, IReadOnlyList<DocumentPayload> documents);
}

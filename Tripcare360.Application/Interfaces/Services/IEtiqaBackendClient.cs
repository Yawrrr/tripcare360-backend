using Tripcare360.Application.Dtos.Policy;

namespace Tripcare360.Application.Interfaces.Services;

public interface IEtiqaBackendClient
{
    Task<VerifyPolicyResponse?> VerifyPolicyAsync(string policyNumber, string identityNumber);
}

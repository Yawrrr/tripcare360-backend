using System.Net.Http.Json;
using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Clients;

public class EtiqaBackendClient(HttpClient httpClient) : IEtiqaBackendClient
{
    public async Task<VerifyPolicyResponse?> VerifyPolicyAsync(string policyNumber, string identityNumber)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/v1/policies/verify", new
            {
                policyNumber,
                identityNumber
            });

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<VerifyPolicyResponse>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Etiqa core policy registry is currently unreachable.", ex);
        }
    }
}

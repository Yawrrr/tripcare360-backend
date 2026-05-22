using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Clients;

public class EtiqaBackendClient(HttpClient httpClient, ILogger<EtiqaBackendClient> logger)
    : IEtiqaBackendClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<VerifyPolicyResponse?> VerifyPolicyAsync(string policyNumber, string identityNumber)
    {
        logger.LogInformation("[EtiqaBackend] POST policy verify — policyNumber={PolicyNumber} identityNumber={IdentityNumber}",
            policyNumber, identityNumber);
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/v1/policies/verify", new
            {
                policyNumber,
                identityNumber
            });

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning("[EtiqaBackend] 404 — policy not found for policyNumber={PolicyNumber}", policyNumber);
                return null;
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<VerifyPolicyResponse>(JsonOptions);
            logger.LogInformation(
                "[EtiqaBackend] 200 — policyNumber={PolicyNumber} insuredName={InsuredName} route={Route} tier={Tier} eligibleTypes={EligibleTypes}",
                result?.PolicyNumber, result?.InsuredName, result?.Route, result?.Tier,
                result?.EligibleClaimTypes is { } types ? string.Join(",", types) : "none");
            return result;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "[EtiqaBackend] Network failure for policyNumber={PolicyNumber} — service unreachable", policyNumber);
            throw new Exception("Etiqa core policy registry is currently unreachable.", ex);
        }
    }
}

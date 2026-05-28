using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Dtos.Policy;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Clients;

public class EtiqaBackendClient(
    HttpClient httpClient,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<EtiqaBackendClient> logger)
    : IEtiqaBackendClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── Fields expected per claim type for structured extraction ──────────────
    private static readonly Dictionary<string, string[]> DocumentAiSchemas = new()
    {
        { "MedicalExpenses",               ["TreatmentDate", "FacilityName", "BillAmount", "IsCovidRelated"] },
        { "FollowUpTreatment",             ["TreatmentDate", "FacilityName", "BillAmount"] },
        { "AlternativeTreatment",          ["TreatmentDate", "FacilityName", "BillAmount"] },
        { "HospitalConfinement",           ["AdmissionDate", "DischargeDate", "FacilityName"] },
        { "TripCancellation",              ["CancellationDate", "Reason", "TripCost"] },
        { "TripCurtailment",               ["ReturnDate", "Reason"] },
        { "FlightDelay",                   ["AirlineName", "FlightNumber", "DepartureDate", "DelayHours"] },
        { "BaggageDelay",                  ["AirlineName", "DelayHours"] },
        { "MissedConnection",              ["AirlineName", "FlightNumber", "ConnectionDate"] },
        { "HijackInconvenience",           ["AirlineName", "FlightNumber", "StartDate", "EndDate"] },
        { "HomeCare",                      ["ServiceProvider", "ServiceDate", "ServiceDescription"] },
        { "BaggageLossAndPersonalEffects", ["IncidentDate", "LostItems", "EstimatedValue"] },
        { "PersonalMoneyLoss",             ["IncidentDate", "AmountLost", "Currency"] },
        { "TravelDocumentsLoss",           ["IncidentDate", "DocumentType", "ReplacementCost"] },
        { "PersonalLiability",             ["IncidentDate", "Description", "ClaimAmount"] },
        { "EmergencyEvacuation",           ["IncidentDate", "FacilityName", "EvacuationService"] },
        { "RepatriationOfRemains",         ["DeceasedName", "DateOfDeath", "RepatriationService"] },
        { "DeathOrPermanentDisability",    ["IncidentDate", "Description", "CertificateType"] },
        { "AdventurousActivities",         ["ActivityName", "IncidentDate", "InjuryDescription", "FacilityName", "BillAmount"] },
        { "GolfCover",                     ["SubType", "IncidentDate", "Description", "AmountClaimed"] },
        { "ExtendedHomeCare",              ["DamageType", "IncidentDate", "Description", "AmountClaimed"] },
    };

    private const string SystemPrompt =
        "You are an automated insurance claims processor. Extract the requested fields from the " +
        "provided document content and return them strictly as a valid JSON object matching the " +
        "requested schema. Use null for any field you cannot find. Do not include any text outside the JSON object.";

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

    public async Task<string?> ProcessClaimDocumentsAsync(string claimType, IReadOnlyList<DocumentPayload> documents)
    {
        if (documents.Count == 0) return null;

        var apiKey = configuration["AiProcessing:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("[DocumentProcessing] API key not configured — skipping document analysis");
            return null;
        }

        var fields = DocumentAiSchemas.TryGetValue(claimType, out var schema)
            ? schema
            : ["Date", "Description", "Amount"];

        var fieldList = string.Join(", ", fields);
        var jsonSchema = "{" + string.Join(", ", fields.Select(f => $"\"{f}\": null")) + "}";

        var textDocs  = documents.Where(d => !string.IsNullOrWhiteSpace(d.Text)).ToList();
        var imageDocs = documents.Where(d => string.IsNullOrWhiteSpace(d.Text) && d.ImageBytes is { Length: > 0 }).ToList();

        string? textResult  = null;
        string? visionResult = null;

        if (textDocs.Count > 0)
            textResult = await CallTextModelAsync(apiKey, fieldList, jsonSchema, textDocs);

        if (imageDocs.Count > 0)
            visionResult = await CallVisionModelAsync(apiKey, fieldList, jsonSchema, imageDocs);

        return MergeResults(textResult, visionResult);
    }

    private async Task<string?> CallTextModelAsync(string apiKey, string fieldList, string jsonSchema, List<DocumentPayload> docs)
    {
        var combinedText = string.Join("\n\n---\n\n", docs.Select(d => d.Text));
        var userContent = $"Extract the following fields: {fieldList}.\n" +
                          $"Return strictly this JSON schema (use null for missing fields): {jsonSchema}\n\n" +
                          $"Document content:\n{combinedText}";

        var body = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content = userContent }
            },
            response_format = new { type = "json_object" }
        };

        return await PostToProcessingApiAsync(apiKey, body);
    }

    private async Task<string?> CallVisionModelAsync(string apiKey, string fieldList, string jsonSchema, List<DocumentPayload> docs)
    {
        var contentParts = new List<object>
        {
            new { type = "text", text =
                $"Extract the following fields from the document image(s): {fieldList}.\n" +
                $"Return strictly this JSON schema (use null for missing fields): {jsonSchema}" }
        };

        foreach (var doc in docs)
        {
            var mime = doc.ContentType.StartsWith("image/") ? doc.ContentType : "image/jpeg";
            var b64  = Convert.ToBase64String(doc.ImageBytes!);
            contentParts.Add(new
            {
                type = "image_url",
                image_url = new { url = $"data:{mime};base64,{b64}" }
            });
        }

        var body = new
        {
            model = "deepseek-vl2",
            messages = new[]
            {
                new { role = "system", content = (object)SystemPrompt },
                new { role = "user",   content = (object)contentParts }
            },
            response_format = new { type = "json_object" }
        };

        return await PostToProcessingApiAsync(apiKey, body);
    }

    private async Task<string?> PostToProcessingApiAsync(string apiKey, object body)
    {
        try
        {
            var baseUrl = configuration["AiProcessing:BaseUrl"] ?? "https://api.deepseek.com";
            var client  = httpClientFactory.CreateClient("AiProcessor");
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var json    = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("[DocumentProcessing] API returned {StatusCode}", response.StatusCode);
                return null;
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc    = await JsonDocument.ParseAsync(stream);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[DocumentProcessing] Request failed — document analysis skipped");
            return null;
        }
    }

    private static string? MergeResults(string? textResult, string? visionResult)
    {
        if (textResult is null && visionResult is null) return null;
        if (textResult is null) return visionResult;
        if (visionResult is null) return textResult;

        try
        {
            var textObj   = JsonDocument.Parse(textResult).RootElement;
            var visionObj = JsonDocument.Parse(visionResult).RootElement;

            var merged = new Dictionary<string, JsonElement>();

            // Seed from vision first, then overwrite with text (text is more reliable)
            foreach (var prop in visionObj.EnumerateObject())
                merged[prop.Name] = prop.Value;
            foreach (var prop in textObj.EnumerateObject())
                if (prop.Value.ValueKind != JsonValueKind.Null)
                    merged[prop.Name] = prop.Value;

            return JsonSerializer.Serialize(merged);
        }
        catch
        {
            return textResult ?? visionResult;
        }
    }
}

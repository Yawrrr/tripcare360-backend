using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Tripcare360.Application.Dtos.Flight;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Clients;

public class TravelStatusRegistryClient(HttpClient httpClient, ILogger<TravelStatusRegistryClient> logger)
    : ITravelStatusRegistryClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<FlightStatusResponse?> GetFlightStatusAsync(string flightNumber, DateTime date)
    {
        var formattedDate = date.ToString("yyyy-MM-dd");
        logger.LogInformation("[FlightRegistry] GET flight status — flightNumber={FlightNumber} departureDate={DepartureDate}",
            flightNumber, formattedDate);
        try
        {
            var route = $"/api/v1/flights/status?flightNumber={flightNumber}&departureDate={formattedDate}";
            var response = await httpClient.GetAsync(route);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.LogWarning("[FlightRegistry] 404 — flight record not found for {FlightNumber} on {DepartureDate}",
                    flightNumber, formattedDate);
                return null;
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<FlightStatusResponse>(JsonOptions);
            logger.LogInformation(
                "[FlightRegistry] 200 — flightNumber={FlightNumber} isDelayed={IsDelayed} actualDelayHours={ActualDelayHours} isBaggageDelayed={IsBaggageDelayed} baggageDelayHours={BaggageDelayHours}",
                result?.FlightNumber, result?.IsDelayed, result?.ActualDelayHours,
                result?.IsBaggageDelayed, result?.BaggageDelayHours);
            return result;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "[FlightRegistry] Network failure for {FlightNumber} on {DepartureDate} — treating as outage bypass",
                flightNumber, formattedDate);
            throw new Exception("Global flight status registry network timed out.", ex);
        }
    }
}

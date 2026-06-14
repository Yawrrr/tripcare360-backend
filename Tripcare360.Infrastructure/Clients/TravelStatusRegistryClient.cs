using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Tripcare360.Application.Dtos.Flight;
using Tripcare360.Application.Interfaces.Services;
using Tripcare360.Domain.Entities.Errors;

namespace Tripcare360.Infrastructure.Clients;

public class TravelStatusRegistryClient(HttpClient httpClient, ILogger<TravelStatusRegistryClient> logger)
    : ITravelStatusRegistryClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<FlightStatusResponse?> GetFlightStatusAsync(string flightNumber)
    {
        logger.LogInformation("[FlightRegistry] GET flight status — flightNumber={FlightNumber}", flightNumber);

        var route = $"/api/v1/flights/status?flightNumber={Uri.EscapeDataString(flightNumber)}";
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(route);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "[FlightRegistry] Network failure for {FlightNumber} — treating as outage", flightNumber);
            throw new Exception("Global flight status registry network timed out.", ex);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("[FlightRegistry] 404 — flight record not found for {FlightNumber}", flightNumber);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("[FlightRegistry] {StatusCode} — error response for {FlightNumber}", (int)response.StatusCode, flightNumber);
            throw new ApiException(ErrorCode.VerificationServiceError,
                details: $"Flight status registry returned {(int)response.StatusCode} for flight {flightNumber}.");
        }

        var result = await response.Content.ReadFromJsonAsync<FlightStatusResponse>(JsonOptions);
        logger.LogInformation(
            "[FlightRegistry] 200 — flightNumber={FlightNumber} isDelayed={IsDelayed} actualDelayHours={ActualDelayHours}",
            result?.FlightNumber, result?.IsDelayed, result?.ActualDelayHours);
        return result;
    }

    public async Task<BookingStatusResponse?> GetBookingAsync(string flightNumber, string bookingNumber)
    {
        logger.LogInformation("[FlightRegistry] GET booking — flightNumber={FlightNumber} bookingNumber={BookingNumber}",
            flightNumber, bookingNumber);

        var route = $"/api/v1/flights/booking?flightNumber={Uri.EscapeDataString(flightNumber)}&bookingNumber={Uri.EscapeDataString(bookingNumber)}";
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(route);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "[FlightRegistry] Network failure for {FlightNumber}/{BookingNumber} — treating as outage",
                flightNumber, bookingNumber);
            throw new Exception("Global flight status registry network timed out.", ex);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            logger.LogWarning("[FlightRegistry] 404 — no boarding record for {FlightNumber}/{BookingNumber}",
                flightNumber, bookingNumber);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("[FlightRegistry] {StatusCode} — error response for {FlightNumber}/{BookingNumber}",
                (int)response.StatusCode, flightNumber, bookingNumber);
            throw new ApiException(ErrorCode.VerificationServiceError,
                details: $"Flight status registry returned {(int)response.StatusCode} for booking {bookingNumber}.");
        }

        var result = await response.Content.ReadFromJsonAsync<BookingStatusResponse>(JsonOptions);
        logger.LogInformation(
            "[FlightRegistry] 200 — flightNumber={FlightNumber} isBaggageDelayed={IsBaggageDelayed} baggageDelayHours={BaggageDelayHours}",
            result?.FlightNumber, result?.IsBaggageDelayed, result?.BaggageDelayHours);
        return result;
    }
}

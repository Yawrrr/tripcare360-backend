using System.Net.Http.Json;
using Tripcare360.Application.Dtos.Flight;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Clients;

public class TravelStatusRegistryClient(HttpClient httpClient) : ITravelStatusRegistryClient
{
    public async Task<FlightStatusResponse?> GetFlightStatusAsync(string flightNumber, DateTime date)
    {
        try
        {
            var formattedDate = date.ToString("yyyy-MM-dd");
            var route = $"/api/v1/flights/status?flightNumber={flightNumber}&departureDate={formattedDate}";
            var response = await httpClient.GetAsync(route);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FlightStatusResponse>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Global flight status registry network timed out.", ex);
        }
    }
}

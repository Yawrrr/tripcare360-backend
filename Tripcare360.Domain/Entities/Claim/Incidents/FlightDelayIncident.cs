namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class FlightDelayIncident
{
    public required string AirlineName { get; set; }
    public required string FlightNumber { get; set; }
    public required string BookingNumber { get; set; }
    public DateTime DepartureDate { get; set; }
    public required string DepartureLocation { get; set; }
}

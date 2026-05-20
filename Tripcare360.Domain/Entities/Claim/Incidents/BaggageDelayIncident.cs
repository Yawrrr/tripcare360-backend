namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class BaggageDelayIncident
{
    public required string AirlineName { get; set; }
    public required string FlightNumber { get; set; }
    public DateTime ArrivalDate { get; set; }
    public required string DelayLocation { get; set; }
}

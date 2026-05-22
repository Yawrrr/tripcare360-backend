namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class HijackIncident
{
    public required string AirlineName { get; set; }
    public required string FlightNumber { get; set; }
    public DateTime HijackDate { get; set; }
    public int DurationDays { get; set; }
}

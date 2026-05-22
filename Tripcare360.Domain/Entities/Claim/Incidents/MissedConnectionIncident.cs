namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class MissedConnectionIncident
{
    public required string AirlineName { get; set; }
    public required string FlightNumber { get; set; }
    public DateTime MissedDate { get; set; }
    public required string ConnectionLocation { get; set; }
}

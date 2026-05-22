namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class TripCurtailmentIncident
{
    public DateTime CurtailmentDate { get; set; }
    public required string Reason { get; set; }
    public DateTime EarlyReturnDate { get; set; }
}

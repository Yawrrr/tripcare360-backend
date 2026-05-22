namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class TripCancellationIncident
{
    public DateTime CancellationDate { get; set; }
    public required string Reason { get; set; }
    public DateTime OriginalTravelDate { get; set; }
}

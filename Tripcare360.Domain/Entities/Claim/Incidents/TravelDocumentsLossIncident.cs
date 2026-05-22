namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class TravelDocumentsLossIncident
{
    public DateTime LossDate { get; set; }
    public required string LossLocation { get; set; }
    public required string DocumentType { get; set; }
}

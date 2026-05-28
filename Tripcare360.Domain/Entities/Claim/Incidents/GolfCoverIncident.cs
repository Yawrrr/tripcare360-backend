namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class GolfCoverIncident
{
    /// <summary>"EquipmentLoss" or "UnusedGreenFees"</summary>
    public required string SubType { get; set; }
    public DateTime IncidentDate { get; set; }
    public required string Description { get; set; }
    public decimal AmountClaimed { get; set; }
}

namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class EmergencyEvacuationIncident
{
    public DateTime EvacuationDate { get; set; }
    public required string FacilityName { get; set; }
    public required string EvacuationReason { get; set; }
}

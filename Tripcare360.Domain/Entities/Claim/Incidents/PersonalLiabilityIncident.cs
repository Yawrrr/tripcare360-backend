namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class PersonalLiabilityIncident
{
    public DateTime IncidentDate { get; set; }
    public required string IncidentLocation { get; set; }
    public required string Description { get; set; }
}

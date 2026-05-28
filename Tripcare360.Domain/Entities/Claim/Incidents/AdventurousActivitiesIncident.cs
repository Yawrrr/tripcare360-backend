namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class AdventurousActivitiesIncident
{
    public required string ActivityName { get; set; }
    public DateTime IncidentDate { get; set; }
    public required string InjuryDescription { get; set; }
    public string? FacilityName { get; set; }
    public decimal BillAmount { get; set; }
}

namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class AlternativeTreatmentIncident
{
    public DateTime TreatmentDate { get; set; }
    public required string FacilityName { get; set; }
    public required string TreatmentType { get; set; }
    public decimal BillAmount { get; set; }
}

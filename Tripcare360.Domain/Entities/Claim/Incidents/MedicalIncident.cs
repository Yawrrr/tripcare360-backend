namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class MedicalIncident
{
    public DateTime TreatmentDate { get; set; }
    public required string FacilityName { get; set; }
    public decimal BillAmount { get; set; }
    public bool IsCovidRelated { get; set; }
}

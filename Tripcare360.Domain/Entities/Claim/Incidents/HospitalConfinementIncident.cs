namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class HospitalConfinementIncident
{
    public required string HospitalName { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime DischargeDate { get; set; }
    public int NumberOfDays { get; set; }
}

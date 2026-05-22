namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class HomeCareIncident
{
    public required string ServiceProvider { get; set; }
    public DateTime ServiceDate { get; set; }
    public required string ServiceType { get; set; }
    public decimal Amount { get; set; }
}

namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class BaggageLossIncident
{
    public required string ItemDescription { get; set; }
    public int ItemAgeMonths { get; set; }
    public decimal PurchaseValue { get; set; }
    public required string LossLocation { get; set; }
}

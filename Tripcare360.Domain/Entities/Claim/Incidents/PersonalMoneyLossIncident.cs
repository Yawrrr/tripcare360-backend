namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class PersonalMoneyLossIncident
{
    public DateTime LossDate { get; set; }
    public required string LossLocation { get; set; }
    public decimal Amount { get; set; }
}

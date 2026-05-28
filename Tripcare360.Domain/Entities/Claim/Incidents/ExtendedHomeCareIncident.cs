namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class ExtendedHomeCareIncident
{
    /// <summary>"Burglary", "Fire", or "WaterDamage"</summary>
    public required string DamageType { get; set; }
    public DateTime IncidentDate { get; set; }
    public required string Description { get; set; }
    public decimal AmountClaimed { get; set; }
}

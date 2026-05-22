namespace Tripcare360.Domain.Entities.Claim.Incidents;

public class RepatriationOfRemainsIncident
{
    public DateTime DateOfDeath { get; set; }
    public required string LocationOfDeath { get; set; }
    public required string NextOfKinContact { get; set; }
}

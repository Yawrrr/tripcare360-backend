using Tripcare360.Domain.Entities.Common;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Domain.Entities.Claim;

public class ClaimEntity : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string PolicyNumber { get; set; }
    public required string IdentityNumber { get; set; }
    public required string FlightNumber { get; set; }
    public ClaimType Type { get; set; }
    public decimal EstimatedPayout { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
}

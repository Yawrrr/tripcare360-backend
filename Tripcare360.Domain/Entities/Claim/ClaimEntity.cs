using Tripcare360.Domain.Enums;

namespace Tripcare360.Domain.Entities.Claim;

public class ClaimEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string PolicyNumber { get; set; }
    public required string IdentityNumber { get; set; }
    public required string FlightNumber { get; set; }
    public ClaimType Type { get; set; }
    public decimal EstimatedPayout { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

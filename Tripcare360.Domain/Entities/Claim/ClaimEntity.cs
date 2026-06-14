using Tripcare360.Domain.Entities.Common;
using Tripcare360.Domain.Enums;

namespace Tripcare360.Domain.Entities.Claim;

public class ClaimEntity : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string ClaimCode { get; set; }
    public required string PolicyNumber { get; set; }
    public required string IdentityNumber { get; set; }
    public required string InsuredName { get; set; }
    public TravelRoute Route { get; set; }
    public PolicyTier Tier { get; set; }
    public int InsuredAge { get; set; }
    public ClaimType Type { get; set; }
    public decimal SubmittedAmount { get; set; }
    public decimal CalculatedPayout { get; set; }
    public required string IncidentDetailsJson { get; set; }
    public List<string> FileObjectKeys { get; set; } = [];
    public List<string> FileLabels { get; set; } = [];
    public required string Country { get; set; }
    public bool IsPreValidationFailedDueToOutage { get; set; } = false;
    public string? AdminComments { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;
    public DateTimeOffset? ProcessedAt { get; set; }
}

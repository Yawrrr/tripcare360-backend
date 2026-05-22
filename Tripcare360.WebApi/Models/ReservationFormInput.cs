using Tripcare360.Domain.Enums;

namespace Tripcare360.WebApi.Models;

public class ReservationFormInput
{
    public string PolicyNumber { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string InsuredName { get; set; } = string.Empty;
    public TravelRoute Route { get; set; }
    public PolicyTier Tier { get; set; }
    public int InsuredAge { get; set; }
    public ClaimType ClaimType { get; set; }
    public decimal SubmittedAmount { get; set; }
    public string IncidentDetailsJson { get; set; } = string.Empty;
    public List<IFormFile>? SupportingFiles { get; set; }
    public List<string>? SupportingFileLabels { get; set; }
}

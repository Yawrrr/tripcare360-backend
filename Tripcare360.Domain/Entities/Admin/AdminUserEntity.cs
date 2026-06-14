using Tripcare360.Domain.Entities.Common;

namespace Tripcare360.Domain.Entities.Admin;

public class AdminUserEntity : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string Role { get; set; } = "ClaimsOfficer";
    public bool IsActive { get; set; } = true;
}

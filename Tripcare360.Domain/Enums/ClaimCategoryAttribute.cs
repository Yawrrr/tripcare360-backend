namespace Tripcare360.Domain.Enums;

[AttributeUsage(AttributeTargets.Field)]
public class ClaimCategoryAttribute(ClaimCategory category) : Attribute
{
    public ClaimCategory Category { get; } = category;
}

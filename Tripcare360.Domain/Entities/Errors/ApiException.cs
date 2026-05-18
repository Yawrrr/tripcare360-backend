namespace Tripcare360.Domain.Entities.Errors;

public class ApiException : Exception
{
    public ErrorCode ErrorCode { get; }
    public string? OverrideDetails { get; }

    public ApiException(ErrorCode errorCode, string? msg = null, string? details = null)
        : base(msg ?? errorCode.ErrorMsg)
    {
        ErrorCode = errorCode;
        OverrideDetails = details;
    }
}

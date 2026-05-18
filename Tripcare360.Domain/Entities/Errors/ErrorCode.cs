namespace Tripcare360.Domain.Entities.Errors;

public class ErrorCode
{
    public string Code { get; }
    public string ErrorMsg { get; }
    public string Details { get; }

    private ErrorCode(string code, string errorMsg, string details)
    {
        Code = code;
        ErrorMsg = errorMsg;
        Details = details;
    }

    public static readonly ErrorCode InvalidClaimType = new("CLM_001", "Invalid claim type provided.", "The claimType value does not match any known category.");
    public static readonly ErrorCode FlightVerificationFailed = new("CLM_002", "Flight verification failed.", "Unable to verify flight delay status.");
    public static readonly ErrorCode InternalServerError = new("SYS_001", "An unexpected error occurred.", "The server encountered an internal error. Please try again later.");
}

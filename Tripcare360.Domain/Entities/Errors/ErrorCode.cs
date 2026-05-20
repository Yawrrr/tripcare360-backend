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

    public static readonly ErrorCode PolicyNotFound = new("POL_001", "Policy not found.", "The policy number and identity number combination does not match any active policy.");
    public static readonly ErrorCode AutomatedCheckFailed = new("CLM_002", "Automated check failed.", "Flight operated on schedule or claim does not meet the minimum threshold.");
    public static readonly ErrorCode ClaimNotFound = new("CLM_003", "Claim not found.", "No pending claim exists for the provided claim code.");
    public static readonly ErrorCode ClaimExpired = new("CLM_004", "Claim reservation expired.", "The 10-minute reservation window has elapsed. Please restart the claim process.");
    public static readonly ErrorCode InternalServerError = new("SYS_001", "An unexpected error occurred.", "The server encountered an internal error. Please try again later.");
}

namespace Tripcare360.Domain.Entities.Errors;

public class ErrorCode
{
    public string Code { get; }
    public string ErrorMsg { get; }
    public string ErrDetail { get; }

    private ErrorCode(string code, string errorMsg, string errDetail)
    {
        Code = code;
        ErrorMsg = errorMsg;
        ErrDetail = errDetail;
    }

    public static readonly ErrorCode PolicyNotFound = new("POL_001", "Policy not found.", "The policy number and identity number combination does not match any active policy.");
    public static readonly ErrorCode AutomatedCheckFailed = new("CLM_002", "Automated check failed.", "Flight operated on schedule or claim does not meet the minimum threshold.");
    public static readonly ErrorCode ClaimNotFound = new("CLM_003", "Claim not found.", "No pending claim exists for the provided claim code.");
    public static readonly ErrorCode ClaimExpired = new("CLM_004", "Claim reservation expired.", "The 10-minute reservation window has elapsed. Please restart the claim process.");
    public static readonly ErrorCode ClaimAlreadyProcessed = new("CLM_005", "Claim already processed.", "This claim has already been approved or rejected and cannot be updated.");
    public static readonly ErrorCode InvalidAction = new("CLM_006", "Invalid action.", "The specified action is not valid. Use 'Approve' or 'Reject'.");
    public static readonly ErrorCode ExternalServiceOutage = new("CLM_007", "Verification service unavailable.", "The external flight verification service is temporarily unavailable. Please attach a supporting document and proceed for manual review.");
    public static readonly ErrorCode FlightNotFound = new("CLM_008", "Flight not found.", "No flight record matches the provided flight number.");
    public static readonly ErrorCode InternalServerError = new("SYS_001", "Internal Server Error", "The server encountered an internal error. Please try again later.");
    public static readonly ErrorCode BookingNotFound             = new("CLM_009", "Booking reference not found.", "No boarding record was found for your flight with the provided booking reference. Please check your booking number and try again.");
    public static readonly ErrorCode TravellerMismatch           = new("CLM_010", "Booking does not match the insured traveller.", "The booking reference is registered under a different identity number than the verified policyholder. Please ensure you are submitting a claim for your own travel.");
    public static readonly ErrorCode FlightRouteMismatch         = new("CLM_011", "Flight route does not match your policy coverage.", "The flight's travel route (Domestic or International) does not match the coverage area in your policy. Please review your policy plan and travel route.");
    public static readonly ErrorCode FlightDelayThresholdNotMet  = new("CLM_012", "Your flight delay does not qualify for a claim.", "The flight was on schedule or the delay did not meet the minimum hours required for a Flight Delay claim. Please check your policy for the minimum delay threshold.");
    public static readonly ErrorCode BaggageDelayThresholdNotMet = new("CLM_013", "Your baggage delay does not qualify for a claim.", "The baggage delay was not confirmed or did not meet the minimum hours required for a Baggage Delay claim. Please check your policy for the minimum delay threshold.");
    public static readonly ErrorCode VerificationServiceError    = new("CLM_014", "The verification service returned an error.", "The external flight registry returned an unexpected response. Please try again or attach a supporting document — your claim will be reviewed manually.");
    public static readonly ErrorCode PdfEncrypted                = new("CLM_015", "PDF file is password-protected.", "One or more uploaded files are password-protected PDF files. Please remove the password protection and re-upload.");
    public static readonly ErrorCode InvalidCredentials = new("AUTH_001", "Invalid credentials.", "The email or password is incorrect.");
    public static readonly ErrorCode AccountDisabled    = new("AUTH_002", "Account disabled.", "This admin account has been deactivated.");
}

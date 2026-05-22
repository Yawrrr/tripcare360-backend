namespace Tripcare360.WebApi.Responses;

public record SuccessResponse<T>(string Status, T Data, string Timestamp, string TraceId);

public record ErrorResponse(
    string Status,
    string ErrorCode,
    string ErrMsg,
    string ErrDetail,
    string Timestamp,
    string TraceId);

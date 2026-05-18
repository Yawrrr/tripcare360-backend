using System.Text.Json;
using Tripcare360.Domain.Entities.Errors;
using Tripcare360.WebApi.Responses;

namespace Tripcare360.WebApi.Middleware;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            var errorCode = ex.ErrorCode;
            int statusCode = errorCode.Code.StartsWith("SYS") ? 500 : 400;

            await WriteErrorResponse(context, statusCode, new ErrorResponse(
                Status: "Failed",
                ErrorCode: errorCode.Code,
                ErrMsg: ex.Message,
                Details: ex.OverrideDetails ?? errorCode.Details,
                Timestamp: DateTimeOffset.UtcNow.ToString("o"),
                TraceId: context.TraceIdentifier));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            var errorCode = ErrorCode.InternalServerError;
            await WriteErrorResponse(context, 500, new ErrorResponse(
                Status: "Failed",
                ErrorCode: errorCode.Code,
                ErrMsg: errorCode.ErrorMsg,
                Details: errorCode.Details,
                Timestamp: DateTimeOffset.UtcNow.ToString("o"),
                TraceId: context.TraceIdentifier));
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, ErrorResponse body)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}

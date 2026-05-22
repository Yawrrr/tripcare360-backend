using System.Text;
using System.Text.Json;
using Tripcare360.WebApi.Responses;

namespace Tripcare360.WebApi.Middleware;

public class ResponseWrapperMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        // SSE streams must flow directly to the socket — skip buffering entirely
        if (context.Request.Path.Value?.Contains("/sse/") == true)
        {
            await next(context);
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);
        }
        finally
        {
            // Always restore before the exception can escape to ExceptionHandlerMiddleware,
            // so that middleware can write the error body to the real response stream.
            context.Response.Body = originalBody;
        }

        buffer.Seek(0, SeekOrigin.Begin);

        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            var capturedJson = await new StreamReader(buffer).ReadToEndAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(capturedJson);

            var wrapped = new SuccessResponse<JsonElement>(
                Status: "Success",
                Data: data,
                Timestamp: DateTimeOffset.UtcNow.ToString("o"),
                TraceId: context.TraceIdentifier);

            var wrappedJson = JsonSerializer.Serialize(wrapped, JsonOptions);
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(wrappedJson);
            await context.Response.WriteAsync(wrappedJson);
        }
        else
        {
            await buffer.CopyToAsync(originalBody);
        }
    }
}

using System.Text;
using System.Text.Json;
using Tripcare360.WebApi.Responses;

namespace Tripcare360.WebApi.Middleware;

public class ResponseWrapperMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await next(context);

        buffer.Seek(0, SeekOrigin.Begin);
        context.Response.Body = originalBody;

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
            buffer.Seek(0, SeekOrigin.Begin);
            await buffer.CopyToAsync(originalBody);
        }
    }
}

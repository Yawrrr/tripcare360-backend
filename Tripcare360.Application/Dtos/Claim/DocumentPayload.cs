namespace Tripcare360.Application.Dtos.Claim;

public record DocumentPayload(string Text, byte[]? ImageBytes, string FileName, string ContentType);

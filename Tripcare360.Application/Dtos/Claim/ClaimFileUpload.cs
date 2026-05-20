namespace Tripcare360.Application.Dtos.Claim;

public record ClaimFileUpload(string FileName, string ContentType, Stream Content, long Length);

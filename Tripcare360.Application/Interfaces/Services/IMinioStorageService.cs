using Tripcare360.Application.Dtos.Claim;

namespace Tripcare360.Application.Interfaces.Services;

public interface IMinioStorageService
{
    Task<string> UploadFileAsync(string claimCode, ClaimFileUpload file);
}

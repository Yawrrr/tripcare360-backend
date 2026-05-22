using Minio;
using Minio.DataModel.Args;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Services;

public class MinioStorageService(IMinioClient minioClient) : IMinioStorageService
{
    private const string BucketName = "tripcare360-supporting-docs";

    public async Task<string> UploadFileAsync(string claimCode, ClaimFileUpload file)
    {
        bool bucketExists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(BucketName));

        if (!bucketExists)
            await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));

        var objectKey = $"{claimCode}/{Guid.NewGuid()}_{file.FileName}";

        await minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectKey)
            .WithStreamData(file.Content)
            .WithObjectSize(file.Length)
            .WithContentType(file.ContentType));

        return objectKey;
    }

    public async Task<string> GetPresignedUrlAsync(string objectKey, int expirySeconds = 3600)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(BucketName)
            .WithObject(objectKey)
            .WithExpiry(expirySeconds);
        return await minioClient.PresignedGetObjectAsync(args);
    }
}

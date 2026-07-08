using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;
using Tripcare360.Application.Dtos.Claim;
using Tripcare360.Application.Interfaces.Services;

namespace Tripcare360.Infrastructure.Services;

public class MinioStorageService : IMinioStorageService
{
    private const string BucketName = "tripcare360-supporting-docs";

    private readonly IMinioClient _minioClient;
    // Presigned URLs are signed client-side using whatever endpoint this
    // client is built with, so uploads/bucket checks use the fast internal
    // endpoint while presigning uses a client built against the public host
    // — MINIO_SERVER_URL on the server container has no effect on this SDK.
    private readonly IMinioClient _publicMinioClient;

    public MinioStorageService(IMinioClient minioClient, IConfiguration config)
    {
        _minioClient = minioClient;
        _publicMinioClient = new MinioClient()
            .WithEndpoint(config["MinIO:PublicEndpoint"] ?? config["MinIO:Endpoint"] ?? "localhost:9000")
            .WithCredentials(
                config["MinIO:AccessKey"] ?? "tripcareAdmin",
                config["MinIO:SecretKey"] ?? "tripcarePassword123")
            .WithSSL(config.GetValue("MinIO:PublicUseSSL", config.GetValue("MinIO:UseSSL", false)))
            .Build();
    }

    public async Task<string> UploadFileAsync(string claimCode, ClaimFileUpload file)
    {
        bool bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(BucketName));

        if (!bucketExists)
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));

        var objectKey = $"{claimCode}/{Guid.NewGuid()}_{file.FileName}";

        await _minioClient.PutObjectAsync(new PutObjectArgs()
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
        return await _publicMinioClient.PresignedGetObjectAsync(args);
    }
}

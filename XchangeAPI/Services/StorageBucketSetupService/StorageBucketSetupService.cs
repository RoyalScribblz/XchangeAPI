using Minio;
using Minio.DataModel.Args;
using XchangeAPI.Enums;

namespace XchangeAPI.Services.StorageBucketSetupService;

public class StorageBucketSetupService(IMinioClientFactory minioClientFactory, ILogger<StorageBucketSetupService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var minioClient = minioClientFactory.CreateClient();
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(Buckets.Xchange);
            
            var bucketExists = await minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

            if (bucketExists)
            {
                logger.LogInformation("Storage bucket: {BucketName} already exists", Buckets.Xchange);
                return;
            }

            var makeBucketArgs = new MakeBucketArgs().WithBucket(Buckets.Xchange);

            await minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
            
            logger.LogInformation("Successfully created storage bucket: {BucketName}", Buckets.Xchange);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured setting up the {BucketName} storage bucket", Buckets.Xchange);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
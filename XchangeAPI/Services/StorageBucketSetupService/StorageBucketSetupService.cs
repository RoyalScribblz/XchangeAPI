using Minio;
using Minio.DataModel.Args;
using XchangeAPI.Enums;

namespace XchangeAPI.Services.StorageBucketSetupService;

public class StorageBucketSetupService(IMinioClientFactory minioClientFactory, ILogger<StorageBucketSetupService> logger) : IHostedService, IDisposable
{
    private readonly IMinioClient _minioClient = minioClientFactory.CreateClient();
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(Buckets.Xchange);
            
            var bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

            if (bucketExists)
            {
                logger.LogInformation("Storage bucket: {BucketName} already exists", Buckets.Xchange);
                return;
            }

            var makeBucketArgs = new MakeBucketArgs().WithBucket(Buckets.Xchange);

            await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
            
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

    public void Dispose()
    {
        _minioClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
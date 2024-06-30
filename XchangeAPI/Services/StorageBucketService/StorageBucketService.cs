using Minio;
using Minio.DataModel.Args;
using XchangeAPI.Enums;

namespace XchangeAPI.Services.StorageBucketService;

public class StorageBucketService(IMinioClientFactory minioClientFactory) : IStorageBucketService
{
    private readonly IMinioClient _minioClient = minioClientFactory.CreateClient();
    
    public async Task Put(string id, Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(Buckets.Xchange)
            .WithObject(id)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
    }

    public async Task<(Stream Stream, string ContentType)> Get(string id, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();
        
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(Buckets.Xchange)
            .WithObject(id)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        var stat = await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);

        memoryStream.Position = 0;
        return (memoryStream, stat.ContentType);
    }

    public void Dispose()
    {
        _minioClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
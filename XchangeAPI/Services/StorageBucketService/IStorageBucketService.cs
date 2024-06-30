namespace XchangeAPI.Services.StorageBucketService;

public interface IStorageBucketService : IDisposable
{
    Task Put(string id, Stream stream, string contentType, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string ContentType)> Get(string id, CancellationToken cancellationToken = default);
}
namespace XchangeAPI.Services.StorageBucketService;

public interface IStorageBucketService : IDisposable
{
    Task<Guid> Put(Stream stream, string contentType, CancellationToken cancellationToken = default);
    Task<(Stream Stream, string ContentType)> Get(string id, CancellationToken cancellationToken = default);
}
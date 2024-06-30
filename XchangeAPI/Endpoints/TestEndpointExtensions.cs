using System.Text;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Services.StorageBucketService;

namespace XchangeAPI.Endpoints;

public static class TestEndpointExtensions
{
    public static WebApplication MapTestEndpoints(this WebApplication app)
    {
        app.MapPost("/bucket/put", async (
            IFormFile file,
            CancellationToken cancellationToken,
            IStorageBucketService storageBucketService) =>
        {
            var id = Guid.NewGuid().ToString();

            await storageBucketService.Put(id, file.OpenReadStream(), file.ContentType, cancellationToken);

            return TypedResults.Ok(id);
        }).WithTags("Bucket").DisableAntiforgery();  // TODO re-enable anti forgery

        app.MapGet("/bucket/get", async (
            [FromQuery] string id,
            CancellationToken cancellationToken,
            IStorageBucketService storageBucketService) =>
        {
            var data = await storageBucketService.Get(id, cancellationToken);

            return TypedResults.File(data.Stream, data.ContentType);
        }).WithTags("Bucket");

        return app;
    }
}
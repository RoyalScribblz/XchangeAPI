using XchangeAPI.Services.EvidenceRequestService;

namespace XchangeAPI.Endpoints;

public static class EvidenceRequestEndpointExtensions
{
    public static WebApplication MapEvidenceRequestEndpoints(this WebApplication app)
    {
        app.MapGet("/evidenceRequests", (IEvidenceRequestService evidenceRequestService) =>
            TypedResults.Ok(evidenceRequestService.GetEvidenceRequests()));
        
        return app;
    }
}
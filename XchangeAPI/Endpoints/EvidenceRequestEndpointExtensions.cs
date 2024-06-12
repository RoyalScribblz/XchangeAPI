using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Services.EvidenceRequestService;

namespace XchangeAPI.Endpoints;

public static class EvidenceRequestEndpointExtensions
{
    public static WebApplication MapEvidenceRequestEndpoints(this WebApplication app)
    {
        app.MapGet("/evidenceRequests", (IEvidenceRequestService evidenceRequestService) =>
            TypedResults.Ok(evidenceRequestService.GetEvidenceRequests())).WithTags("EvidenceRequest");

        app.MapPost("/evidenceRequest/{evidenceRequestId:Guid}/evidence", async (
            Guid evidenceRequestId,
            [FromQuery] string value,
            IEvidenceRequestService evidenceRequestService,
            CancellationToken cancellationToken) =>
        {
            await evidenceRequestService.SubmitEvidence(evidenceRequestId, value, cancellationToken);
            return TypedResults.Ok();
        }).WithTags("EvidenceRequest");

        app.MapPost("/evidenceRequest/{evidenceRequestId:Guid}/accept", async (
            Guid evidenceRequestId,
            IEvidenceRequestService evidenceRequestService,
            CancellationToken cancellationToken) =>
        {
            await evidenceRequestService.AcceptEvidence(evidenceRequestId, cancellationToken);
            return TypedResults.Ok();
        }).WithTags("EvidenceRequest");
        
        app.MapPost("/evidenceRequest/{evidenceRequestId:Guid}/reject", async (
            Guid evidenceRequestId,
            IEvidenceRequestService evidenceRequestService,
            CancellationToken cancellationToken) =>
        {
            await evidenceRequestService.RejectEvidence(evidenceRequestId, cancellationToken);
            return TypedResults.Ok();
        }).WithTags("EvidenceRequest");
        
        return app;
    }
}
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Endpoints.Contracts;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.EvidenceRequestService;
using XchangeAPI.Services.StorageBucketService;

namespace XchangeAPI.Endpoints;

public static class EvidenceRequestEndpointExtensions
{
    public static WebApplication MapEvidenceRequestEndpoints(this WebApplication app)
    {
        app.MapGet("/evidenceRequests", async (
            IEvidenceRequestService evidenceRequestService,
            ICurrencyService currencyService,
            CancellationToken cancellationToken) =>
        {
            List<GetEvidenceRequestResponse> response = [];
            
            foreach (var evidenceRequest in evidenceRequestService.GetEvidenceRequests())
            {
                response.Add(new GetEvidenceRequestResponse
                {
                    EvidenceRequestId = evidenceRequest.EvidenceRequestId,
                    UserId = evidenceRequest.UserId,
                    EvidenceIds = evidenceRequest.EvidenceIds,
                    Status = evidenceRequest.Status,
                    Currency = (await currencyService.GetCurrency(evidenceRequest.CurrencyId, cancellationToken))!,
                    Amount = evidenceRequest.Amount
                });
            }

            return TypedResults.Ok(response);
        }).WithTags("EvidenceRequest");

        app.MapPatch("/evidenceRequest/{evidenceRequestId:Guid}/evidence", async (
            Guid evidenceRequestId,
            IFormFileCollection files,
            IEvidenceRequestService evidenceRequestService,
            IStorageBucketService storageBucketService,
            CancellationToken cancellationToken) =>
        {
            foreach (var file in files)
            {
                var evidenceId = await storageBucketService.Put(file.OpenReadStream(), file.ContentType, cancellationToken);
                await evidenceRequestService.SubmitEvidence(evidenceRequestId, evidenceId, cancellationToken);
            }
            
            return TypedResults.Ok();
        }).WithTags("EvidenceRequest").DisableAntiforgery();  // TODO disable antiforgery
        
        app.MapGet("/evidence/{evidenceId:Guid}", async (
            Guid evidenceId,
            IStorageBucketService storageBucketService,
            CancellationToken cancellationToken) =>
        {
            var data = await storageBucketService.Get(evidenceId.ToString(), cancellationToken);

            return TypedResults.File(data.Stream, data.ContentType);
        }).WithTags("EvidenceRequest");
        
        app.MapGet("/evidenceRequest", async Task<Results<NotFound, Ok<GetEvidenceRequestResponse>>>(
            [FromQuery] string userId,
            IEvidenceRequestService evidenceRequestService,
            ICurrencyService currencyService,
            CancellationToken cancellationToken) =>
        {
            var evidenceRequest = await evidenceRequestService.GetEvidenceRequest(userId, cancellationToken);

            if (evidenceRequest == null)
            {
                return TypedResults.NotFound();
            }

            var response = new GetEvidenceRequestResponse
            {
                EvidenceRequestId = evidenceRequest.EvidenceRequestId,
                UserId = evidenceRequest.UserId,
                EvidenceIds = evidenceRequest.EvidenceIds,
                Status = evidenceRequest.Status,
                Currency = (await currencyService.GetCurrency(evidenceRequest.CurrencyId, cancellationToken))!,
                Amount = evidenceRequest.Amount
            };

            return TypedResults.Ok(response);
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

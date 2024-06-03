using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.EvidenceRequestService;

public interface IEvidenceRequestService
{
    List<EvidenceRequest> GetEvidenceRequests();

    Task SubmitEvidence(Guid evidenceRequestId, string evidence, CancellationToken cancellationToken);

    Task AcceptEvidence(Guid evidenceRequestId, CancellationToken cancellationToken);

    Task RejectEvidence(Guid evidenceRequestId, CancellationToken cancellationToken);
}
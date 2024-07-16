using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.EvidenceRequestService;

public interface IEvidenceRequestService
{
    IList<EvidenceRequest> GetEvidenceRequests();

    Task<EvidenceRequest?> GetEvidenceRequest(string userId, CancellationToken cancellationToken = default);
    
    Task<EvidenceRequest?> GetEvidenceRequest(Guid evidenceRequestId, CancellationToken cancellationToken);

    Task SubmitEvidence(Guid evidenceRequestId, Guid evidenceId, CancellationToken cancellationToken);

    Task AcceptEvidence(Guid evidenceRequestId, CancellationToken cancellationToken);

    Task RejectEvidence(Guid evidenceRequestId, CancellationToken cancellationToken);

    Task SetActive(Guid evidenceRequestId, CancellationToken cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.EvidenceRequestService;

public sealed class EvidenceRequestService(XchangeDatabase database) : IEvidenceRequestService
{
    public IList<EvidenceRequest> GetEvidenceRequests() => database.EvidenceRequests.ToList();

    public Task<EvidenceRequest?> GetEvidenceRequest(string userId, CancellationToken cancellationToken) =>
        database.EvidenceRequests.SingleOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    
    public Task<EvidenceRequest?> GetEvidenceRequest(Guid evidenceRequestId, CancellationToken cancellationToken) =>
        database.EvidenceRequests.SingleOrDefaultAsync(e => e.EvidenceRequestId == evidenceRequestId, cancellationToken);

    public async Task SubmitEvidence(Guid evidenceRequestId, Guid evidenceId, CancellationToken cancellationToken)
    {
        var evidenceRequest = await database.EvidenceRequests.SingleOrDefaultAsync(
            e => e.EvidenceRequestId == evidenceRequestId, cancellationToken);

        if (evidenceRequest == null)
        {
            return;
        }

        evidenceRequest.EvidenceIds.Add(evidenceId);

        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task AcceptEvidence(Guid evidenceRequestId, CancellationToken cancellationToken)
    {
        var evidenceRequest = await database.EvidenceRequests.SingleOrDefaultAsync(
            e => e.EvidenceRequestId == evidenceRequestId, cancellationToken);

        if (evidenceRequest == null)
        {
            return;
        }

        evidenceRequest.Status = EvidenceRequestStatus.Accepted;

        var user = await database.Users.SingleOrDefaultAsync(
            u => u.UserId == evidenceRequest.UserId, cancellationToken);

        if (user == null)
        {
            return;
        }

        user.IsFrozen = false;

        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectEvidence(Guid evidenceRequestId, CancellationToken cancellationToken)
    {
        var evidenceRequest = await database.EvidenceRequests.SingleOrDefaultAsync(
            e => e.EvidenceRequestId == evidenceRequestId, cancellationToken);

        if (evidenceRequest == null)
        {
            return;
        }

        evidenceRequest.Status = EvidenceRequestStatus.Rejected;

        var user = await database.Users.SingleOrDefaultAsync(
            u => u.UserId == evidenceRequest.UserId, cancellationToken);

        if (user == null)
        {
            return;
        }

        user.IsBanned = true;

        await database.SaveChangesAsync(cancellationToken);
    }
}

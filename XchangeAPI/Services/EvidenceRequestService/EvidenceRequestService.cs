using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.EvidenceRequestService;

public sealed class EvidenceRequestService(XchangeDatabase database) : IEvidenceRequestService
{
    public List<EvidenceRequest> GetEvidenceRequests() => database.EvidenceRequests.ToList();
}
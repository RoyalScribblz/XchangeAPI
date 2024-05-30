using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.EvidenceRequestService;

public interface IEvidenceRequestService
{
    List<EvidenceRequest> GetEvidenceRequests();
}
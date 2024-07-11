using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Endpoints.Contracts;

public class GetEvidenceRequestResponse
{
    public required Guid EvidenceRequestId { get; set; }
    public required string UserId { get; set; }
    public required List<Guid> EvidenceIds { get; set; }
    public required EvidenceRequestStatus Status { get; set; }
    public required Currency Currency { get; set; }
    public required double Amount { get; set; }
}
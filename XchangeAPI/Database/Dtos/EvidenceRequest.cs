namespace XchangeAPI.Database.Dtos;

public sealed record EvidenceRequest
{
    public required Guid EvidenceRequestId { get; set; }
    public required string UserId { get; set; }
    public required List<Guid> EvidenceIds { get; set; }
    public required EvidenceRequestStatus Status { get; set; }
}

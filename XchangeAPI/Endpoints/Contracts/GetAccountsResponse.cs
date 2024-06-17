using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Endpoints.Contracts;

public sealed class GetAccountsResponse
{
    public required Guid AccountId { get; init; }
    public required string UserId { get; init; }
    public required Currency Currency { get; init; }
    public required double Balance { get; init; }
    public double LocalValue { get; set; }
}

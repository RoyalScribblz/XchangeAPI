namespace XchangeAPI.Database.Dtos;

public sealed record Account
{
    public required Guid AccountId { get; set; }
    public required string UserId { get; set; }
    public required Guid CurrencyId { get; set; }
    public required double Balance { get; set; }
}
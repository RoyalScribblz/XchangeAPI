namespace XchangeAPI.Database.Dtos;

public sealed record User
{
    public required string UserId { get; set; }
    public required Guid MainCurrencyId { get; set; }
    public required bool IsFrozen { get; set; }
    public required bool IsBanned { get; set; }
}
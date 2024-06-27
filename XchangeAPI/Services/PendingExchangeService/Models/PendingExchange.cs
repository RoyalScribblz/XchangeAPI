namespace XchangeAPI.Services.PendingExchangeService.Models;

public sealed record PendingExchange()
{
    public required Guid PendingExchangeId { get; init; }
    public required Guid FromCurrencyId { get; init; }
    public required double FromAmount { get; init; }
    public required Guid ToCurrencyId { get; init; }
    public required double ToAmount { get; init; }
}
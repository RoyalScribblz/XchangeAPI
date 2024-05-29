namespace XchangeAPI.Database.Dtos;

public sealed record Currency
{
    public required Guid CurrencyId { get; set; }
    public required string Name { get; set; }
    public required string CurrencyCode { get; set; }
    public required string FlagImageUrl { get; set; }
    public required string Symbol { get; set; }
    public required double UsdValue { get; set; }
    public required double TransactionLimit { get; set; } = double.MaxValue;
}
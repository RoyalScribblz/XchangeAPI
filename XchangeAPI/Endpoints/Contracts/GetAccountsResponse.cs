namespace XchangeAPI.Endpoints.Contracts;

public sealed class GetAccountsResponse
{
    public required Guid AccountId { get; init; }
    public required string UserId { get; init; }
    public required LocalCurrency Currency { get; init; }
    public required double Balance { get; init; }
}

public sealed class LocalCurrency
{
    public required Guid CurrencyId { get; init; }
    public required string Name { get; init; }
    public required string CurrencyCode { get; init; }
    public required string FlagImageUrl { get; init; }
    public required string Symbol { get; init; }
    public double LocalValue { get; set; }
    public required double TransactionLimit { get; init; }
}
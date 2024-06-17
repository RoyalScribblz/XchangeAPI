using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Endpoints.Contracts;

public sealed class GetUserResponse
{
    public required string UserId { get; set; }

    public Currency LocalCurrency { get; set; } = new()
    {
        CurrencyId = Guid.Parse("6c84631c-838b-403e-8e2b-38614d2e907d"),
        Name = "United States Dollar",
        CurrencyCode = "USD",
        FlagImageUrl = "http://img.geonames.org/flags/x/us.gif",
        Symbol = "$",
        UsdValue = 1,
        TransactionLimit = double.MaxValue,
    };

    public required bool IsFrozen { get; set; }

    public required bool IsBanned { get; set; }
}

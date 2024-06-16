using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.CurrencyService;

public interface ICurrencyService
{
    Task<double?> GetExchangeRate(Guid fromCurrencyId, Guid toCurrencyId, CancellationToken cancellationToken);

    IList<Currency> GetCurrencies();

    Task<bool> UpdateTransactionLimit(Guid currencyId, double newLimit, CancellationToken cancellationToken);
    Task<Currency> GetCurrency(Guid currencyId, CancellationToken cancellationToken);
}

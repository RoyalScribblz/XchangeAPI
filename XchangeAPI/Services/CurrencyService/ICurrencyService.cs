using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.CurrencyService;

public interface ICurrencyService
{
    Task<double?> GetExchangeRate(Guid fromCurrencyId, Guid toCurrencyId, CancellationToken cancellationToken);

    List<Currency> GetCurrencies();

    Task<bool> UpdateTransactionLimit(Guid currencyId, double newLimit, CancellationToken cancellationToken);
}
namespace XchangeAPI.Services.CurrencyService;

public interface ICurrencyService
{
    Task<double?> GetExchangeRate(Guid fromCurrencyId, Guid toCurrencyId, CancellationToken cancellationToken);
}
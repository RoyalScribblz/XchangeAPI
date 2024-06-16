using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Options;

namespace XchangeAPI.Services.CurrencyService;

public sealed class CurrencyService(
    IOptions<OpenExchangeRatesOptions> openExchangeRatesOptions,
    XchangeDatabase database,
    IHttpClientFactory httpClientFactory)
    : ICurrencyService
{
    private static DateTime lastRefreshedTime;

    public async Task<double?> GetExchangeRate(Guid fromCurrencyId, Guid toCurrencyId, CancellationToken cancellationToken)
    {
        // TODO re-enable rate updating with: await UpdateExchangeRates(cancellationToken);

        var fromValue = (await database.Currencies.SingleOrDefaultAsync(
            c => c.CurrencyId == fromCurrencyId, cancellationToken))?.UsdValue;

        var toValue = (await database.Currencies.SingleOrDefaultAsync(
            c => c.CurrencyId == toCurrencyId, cancellationToken))?.UsdValue;

        return toValue / fromValue;
    }

    private async Task UpdateExchangeRates(CancellationToken cancellationToken)
    {
        using var httpClient = httpClientFactory.CreateClient();

        if (lastRefreshedTime.AddMinutes(1) < DateTime.UtcNow)
        {
            lastRefreshedTime = DateTime.UtcNow;
            var exchangeRatesResponse = await httpClient.GetFromJsonAsync<OpenExchangeRatesResponse>(
                $"https://openexchangerates.org/api/latest.json?app_id={openExchangeRatesOptions.Value.ApiKey}",
                cancellationToken);

            foreach (var currencyValue in exchangeRatesResponse?.Rates ?? [])
            {
                var currency = await database.Currencies.SingleOrDefaultAsync(
                    c => c.CurrencyCode == currencyValue.Key,
                    cancellationToken);

                if (currency != null)
                {
                    currency.UsdValue = currencyValue.Value;
                }
            }

            await database.SaveChangesAsync(cancellationToken);
        }
    }

    public IList<Currency> GetCurrencies() => database.Currencies.ToList();

    public async Task<bool> UpdateTransactionLimit(Guid currencyId, double newLimit, CancellationToken cancellationToken)
    {
        var currency = await database.Currencies.SingleOrDefaultAsync(
            c => c.CurrencyId == currencyId, cancellationToken);

        if (currency == null)
        {
            return false;
        }

        currency.TransactionLimit = newLimit;

        await database.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<Currency> GetCurrency(Guid currencyId, CancellationToken cancellationToken)
    {
        return database.Currencies.SingleAsync(c => c.CurrencyId == currencyId, cancellationToken);
    }
}

public class OpenExchangeRatesResponse
{
    public Dictionary<string, double> Rates { get; } = new();
}

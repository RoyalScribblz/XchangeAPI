using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using XchangeAPI.Database;
using XchangeAPI.Options;

namespace XchangeAPI.Services.CurrencyService;

public sealed class CurrencyService(
    IOptions<OpenExchangeRatesOptions> openExchangeRatesOptions,
    XchangeDatabase database,
    IHttpClientFactory httpClientFactory)
    : ICurrencyService
{
    private static DateTime _lastRefreshedTime;
    
    public async Task<double?> GetExchangeRate(Guid fromCurrencyId, Guid toCurrencyId, CancellationToken cancellationToken)
    {
        await UpdateExchangeRates(cancellationToken);
        
        double? fromValue = (await database.Currencies.SingleOrDefaultAsync(
            c => c.CurrencyId == fromCurrencyId))?.UsdValue;
        
        double? toValue = (await database.Currencies.SingleOrDefaultAsync(
            c => c.CurrencyId == toCurrencyId))?.UsdValue;

        return fromValue / toValue;
    }

    private async Task UpdateExchangeRates(CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient();
        
        if (_lastRefreshedTime.AddMinutes(1) < DateTime.UtcNow)
        {
            _lastRefreshedTime = DateTime.UtcNow;
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
}

public class OpenExchangeRatesResponse
{
    public Dictionary<string, double> Rates { get; set; } = new();
}
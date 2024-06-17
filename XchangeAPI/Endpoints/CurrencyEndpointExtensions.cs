using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Services.CurrencyService;

namespace XchangeAPI.Endpoints;

public static class CurrencyEndpointExtensions
{
    public static WebApplication MapCurrencyEndpoints(this WebApplication app)
    {
        app.MapGet("/currencies", (ICurrencyService currencyService) =>
            TypedResults.Ok(currencyService.GetCurrencies())).WithTags("Currency");

        app.MapPut("/currency/{currencyId:Guid}/limit", async (
            Guid currencyId,
            [FromQuery] double amount,
            ICurrencyService currencyService,
            CancellationToken cancellationToken) =>
        {
            await currencyService.UpdateTransactionLimit(currencyId, amount, cancellationToken);
            return TypedResults.Ok();
        }).WithTags("Currency");

        return app;
    }
}

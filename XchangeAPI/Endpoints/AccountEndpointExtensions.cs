using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Endpoints.Contracts;
using XchangeAPI.Services.AccountService;
using XchangeAPI.Services.UserService;

namespace XchangeAPI.Endpoints;

public static class AccountEndpointExtensions
{
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        app.MapGet("/accounts", async (
            [FromQuery] string userId,
            Guid? localCurrencyId,
            CancellationToken cancellationToken,
            IAccountService accountService) => TypedResults.Ok(await accountService.GetAccounts(userId,
            localCurrencyId ?? Guid.Parse("6c84631c-838b-403e-8e2b-38614d2e907d"), cancellationToken)));

        app.MapGet("/exchange", async Task<Results<Ok<List<GetAccountsResponse>>, BadRequest>>(
            [FromQuery] string userId,
            [FromQuery] double amount,
            [FromQuery] Guid fromCurrencyId,
            [FromQuery] Guid toCurrencyId,
            Guid? localCurrencyId,
            IAccountService accountService,
            IUserService userService,
            CancellationToken cancellationToken) =>
        {
            if (await userService.IsFrozen(userId, cancellationToken))
            {
                return TypedResults.BadRequest();
            }
            
            bool success = await accountService.Exchange(userId, amount, fromCurrencyId, toCurrencyId, cancellationToken);

            if (success)
            {
                return TypedResults.Ok(await accountService.GetAccounts(userId, localCurrencyId ?? Guid.Parse("6c84631c-838b-403e-8e2b-38614d2e907d"), cancellationToken));
            }

            return TypedResults.BadRequest();
        });
        
        return app;
    }
}
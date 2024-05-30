using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Services.AccountService;
using XchangeAPI.Services.UserService;

namespace XchangeAPI.Endpoints;

public static class AccountEndpointExtensions
{
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        app.MapGet("/accounts", ([FromQuery] string userId, IAccountService accountService)
            => TypedResults.Ok(accountService.GetAccounts(userId)));

        app.MapGet("/exchange", async Task<Results<Ok<List<Account>>, BadRequest>>(
            [FromQuery] string userId,
            [FromQuery] double amount,
            [FromQuery] Guid fromCurrencyId,
            [FromQuery] Guid toCurrencyId,
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
                return TypedResults.Ok(accountService.GetAccounts(userId));
            }

            return TypedResults.BadRequest();
        });
        
        return app;
    }
}
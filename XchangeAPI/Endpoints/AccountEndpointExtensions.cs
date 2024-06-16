using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Endpoints.Contracts;
using XchangeAPI.Services.AccountService;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.UserService;

namespace XchangeAPI.Endpoints;

public static class AccountEndpointExtensions
{
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        app.MapGet("/accounts", async Task<Results<BadRequest, Ok<List<GetAccountsResponse>>>>(
            [FromQuery] string userId,
            CancellationToken cancellationToken,
            IAccountService accountService,
            ICurrencyService currencyService,
            IUserService userService) =>
        {
            var user = await userService.GetUser(userId, cancellationToken);

            if (user == null)
            {
                return TypedResults.BadRequest();
            }
            
            
            var accounts = accountService.GetAccounts(userId);
            var response = new List<GetAccountsResponse>();
            
            foreach (var account in accounts)
            {
                var currency = await currencyService.GetCurrency(account.CurrencyId, cancellationToken);
                var exchangeRate = await currencyService.GetExchangeRate(
                    currency.CurrencyId,
                    user.LocalCurrencyId,
                    cancellationToken) ?? 0;
                
                response.Add(new GetAccountsResponse
                {
                    AccountId = account.AccountId,
                    UserId = account.UserId,
                    Currency = currency,
                    Balance = account.Balance,
                    LocalValue = exchangeRate * account.Balance
                });
            }
            
            return TypedResults.Ok(response);
        }).WithTags("Account");

        app.MapGet("/exchange", async Task<Results<Ok<List<GetAccountsResponse>>, BadRequest>>(
            [FromQuery] string userId,
            [FromQuery] double amount,
            [FromQuery] Guid fromCurrencyId,
            [FromQuery] Guid toCurrencyId,
            Guid? localCurrencyId,
            IAccountService accountService,
            IUserService userService,
            ICurrencyService currencyService,
            CancellationToken cancellationToken) =>
        {
            if (await userService.IsFrozen(userId, cancellationToken))
            {
                return TypedResults.BadRequest();
            }
            
            var success = await accountService.Exchange(userId, amount, fromCurrencyId, toCurrencyId, cancellationToken);

            if (!success)
            {
                return TypedResults.BadRequest();
            }

            var accounts = accountService.GetAccounts(userId);
            var response = new List<GetAccountsResponse>();
            
            foreach (var account in accounts)
            {
                var currency = await currencyService.GetCurrency(account.CurrencyId, cancellationToken);
                var exchangeRate = await currencyService.GetExchangeRate(
                    currency.CurrencyId,
                    localCurrencyId ?? Guid.Parse("6c84631c-838b-403e-8e2b-38614d2e907d"),  // TODO get from authed user
                    cancellationToken) ?? 0;
                
                response.Add(new GetAccountsResponse
                {
                    AccountId = account.AccountId,
                    UserId = account.UserId,
                    Currency = currency,
                    Balance = account.Balance,
                    LocalValue = exchangeRate * account.Balance
                });
            }
            
            return TypedResults.Ok(response);

        }).WithTags("Account");
        
        return app;
    }
}
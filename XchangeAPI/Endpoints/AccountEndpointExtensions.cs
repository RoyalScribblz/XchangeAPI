using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Endpoints.Contracts;
using XchangeAPI.Services.AccountService;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.PendingExchangeService;
using XchangeAPI.Services.PendingExchangeService.Models;
using XchangeAPI.Services.UserService;

namespace XchangeAPI.Endpoints;

public static class AccountEndpointExtensions
{
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        app.MapPost("/create", async Task<Results<BadRequest, Ok<GetAccountsResponse>>>(
            [FromQuery] string userId,
            [FromQuery] Guid currencyId,
            IAccountService accountService,
            IUserService userService,
            ICurrencyService currencyService,
            CancellationToken cancellationToken) =>
        {
            var account = await accountService.Create(userId, currencyId, cancellationToken);

            if (account == null)
            {
                return TypedResults.BadRequest();
            }

            var user = await userService.GetUser(account.UserId, cancellationToken);

            if (user == null)
            {
                return TypedResults.BadRequest();
            }
            
            var currency = await currencyService.GetCurrency(account.CurrencyId, cancellationToken);
            
            var exchangeRate = await currencyService.GetExchangeRate(
                currency.CurrencyId, user.LocalCurrencyId, cancellationToken) ?? 0;

            return TypedResults.Ok(new GetAccountsResponse
            {
                AccountId = account.AccountId,
                UserId = account.UserId,
                Currency = currency,
                Balance = account.Balance,
                LocalValue = exchangeRate * account.Balance,
            });
        }).WithTags("Account");
        
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
                    LocalValue = exchangeRate * account.Balance,
                });
            }

            return TypedResults.Ok(response);
        }).WithTags("Account");

        app.MapPost("/exchange/create", async Task<Results<Ok<PendingExchange>, BadRequest>>(
            [FromQuery] string userId,
            [FromQuery] double amount,
            [FromQuery] Guid fromCurrencyId,
            [FromQuery] Guid toCurrencyId,
            IUserService userService,
            ICurrencyService currencyService,
            IPendingExchangeService pendingExchangeService,
            CancellationToken cancellationToken) =>
        {
            if (await userService.IsFrozen(userId, cancellationToken))
            {
                return TypedResults.BadRequest();
            }
            
            var exchangeRate = await currencyService.GetExchangeRate(fromCurrencyId, toCurrencyId, cancellationToken);

            if (exchangeRate == null)
            {
                return TypedResults.BadRequest();
            }

            var toAmount = amount * (double)exchangeRate;

            var pendingExchange = pendingExchangeService.Create(userId, fromCurrencyId, amount, toCurrencyId, toAmount);

            if (pendingExchange == null)
            {
                return TypedResults.BadRequest();
            }

            return TypedResults.Ok(pendingExchange);
        }).WithTags("Account");
        
        app.MapPost("/exchange/complete/{pendingExchangeId:guid}", async Task<Results<Ok<List<GetAccountsResponse>>, BadRequest>>(
            Guid pendingExchangeId,
            [FromQuery] string userId,
            IAccountService accountService,
            IUserService userService,
            ICurrencyService currencyService,
            IPendingExchangeService pendingExchangeService,
            CancellationToken cancellationToken) =>
        {
            if (await userService.IsFrozen(userId, cancellationToken))
            {
                return TypedResults.BadRequest();
            }

            var pendingExchange = pendingExchangeService.Get(userId, pendingExchangeId);

            if (pendingExchange == null)
            {
                return TypedResults.BadRequest();
            }

            var success = await accountService.CompleteExchange(userId, pendingExchange, cancellationToken);

            if (!success)
            {
                return TypedResults.BadRequest();
            }

            pendingExchangeService.Remove(userId);

            var accounts = accountService.GetAccounts(userId);
            var response = new List<GetAccountsResponse>();

            var localCurrencyId = await userService.GetLocalCurrencyId(userId, cancellationToken);

            if (localCurrencyId == Guid.Empty)
            {
                return TypedResults.BadRequest();
            }
            
            foreach (var account in accounts)
            {
                var currency = await currencyService.GetCurrency(account.CurrencyId, cancellationToken);
                var exchangeRate = await currencyService.GetExchangeRate(
                    currency.CurrencyId,
                    localCurrencyId,
                    cancellationToken) ?? 0;

                response.Add(new GetAccountsResponse
                {
                    AccountId = account.AccountId,
                    UserId = account.UserId,
                    Currency = currency,
                    Balance = account.Balance,
                    LocalValue = exchangeRate * account.Balance,
                });
            }

            return TypedResults.Ok(response);
        }).WithTags("Account");

        app.MapPatch("/account/{accountId:Guid}/deposit", async Task<Results<BadRequest, Ok<GetAccountsResponse>>>(
            Guid accountId,
            [FromQuery] double amount,
            IAccountService accountService,
            ICurrencyService currencyService,
            IUserService userService,
            CancellationToken cancellationToken) =>
        {
            var account = await accountService.Deposit(accountId, amount, cancellationToken);

            if (account == null)
            {
                return TypedResults.BadRequest();
            }

            var user = await userService.GetUser(account.UserId, cancellationToken);

            if (user == null)
            {
                return TypedResults.BadRequest();
            }
            
            var currency = await currencyService.GetCurrency(account.CurrencyId, cancellationToken);
            
            var exchangeRate = await currencyService.GetExchangeRate(
                currency.CurrencyId, user.LocalCurrencyId, cancellationToken) ?? 0;

            return TypedResults.Ok(new GetAccountsResponse
            {
                AccountId = account.AccountId,
                UserId = account.UserId,
                Currency = currency,
                Balance = account.Balance,
                LocalValue = exchangeRate * account.Balance,
            });
        }).WithTags("Account");

        app.MapPatch("/account/{accountId:Guid}/withdraw", async Task<Results<BadRequest, Ok<GetAccountsResponse>>>(
            Guid accountId,
            [FromQuery] double amount,
            IAccountService accountService,
            ICurrencyService currencyService,
            IUserService userService,
            CancellationToken cancellationToken) =>
        {
            var account = await accountService.Withdraw(accountId, amount, cancellationToken);

            if (account == null)
            {
                return TypedResults.BadRequest();
            }

            var user = await userService.GetUser(account.UserId, cancellationToken);

            if (user == null)
            {
                return TypedResults.BadRequest();
            }
            
            var currency = await currencyService.GetCurrency(account.CurrencyId, cancellationToken);
            
            var exchangeRate = await currencyService.GetExchangeRate(
                currency.CurrencyId, user.LocalCurrencyId, cancellationToken) ?? 0;

            return TypedResults.Ok(new GetAccountsResponse
            {
                AccountId = account.AccountId,
                UserId = account.UserId,
                Currency = currency,
                Balance = account.Balance,
                LocalValue = exchangeRate * account.Balance,
            });
        }).WithTags("Account");
        
        return app;
    }
}

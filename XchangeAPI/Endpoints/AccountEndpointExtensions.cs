using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Endpoints.Contracts;
using XchangeAPI.Endpoints.Validation;
using XchangeAPI.Extensions;
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
        app.MapPost("/create", async Task<Results<UnauthorizedHttpResult, BadRequest, Ok<GetAccountsResponse>>>(
            [FromQuery] Guid currencyId,
            IAccountService accountService,
            IUserService userService,
            ICurrencyService currencyService,
            IValidator<CreateAccountRequest> validator,
            CancellationToken cancellationToken,
            HttpContext context) =>
        {
            var userId = context.GetUserId();

            if (userId == null)
            {
                return TypedResults.Unauthorized();
            }
            
            await validator.ValidateAndThrowAsync(new CreateAccountRequest(userId, currencyId), cancellationToken);
            
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
            
            if (currency == null)
            {
                return TypedResults.BadRequest();
            }
            
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
        }).RequireAuthorization().WithTags("Account");
        
        app.MapGet("/accounts", async Task<Results<UnauthorizedHttpResult, BadRequest, Ok<List<GetAccountsResponse>>>>(
            CancellationToken cancellationToken,
            IAccountService accountService,
            ICurrencyService currencyService,
            IUserService userService,
            HttpContext context) =>
        {
            var userId = context.GetUserId();

            if (userId == null)
            {
                return TypedResults.Unauthorized();
            }
            
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
                    currency!.CurrencyId,
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
        }).RequireAuthorization().WithTags("Account");

        app.MapPost("/exchange/create", async Task<Results<UnauthorizedHttpResult, Ok<PendingExchange>, BadRequest>>(
            [FromQuery] double amount,
            [FromQuery] Guid fromCurrencyId,
            [FromQuery] Guid toCurrencyId,
            IUserService userService,
            ICurrencyService currencyService,
            IPendingExchangeService pendingExchangeService,
            CancellationToken cancellationToken,
            HttpContext context) =>
        {
            var userId = context.GetUserId();

            if (userId == null)
            {
                return TypedResults.Unauthorized();
            }
            
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
        }).RequireAuthorization().WithTags("Account");
        
        app.MapPost("/exchange/complete/{pendingExchangeId:guid}", async Task<Results<UnauthorizedHttpResult, Ok<List<GetAccountsResponse>>, BadRequest>>(
            Guid pendingExchangeId,
            IAccountService accountService,
            IUserService userService,
            ICurrencyService currencyService,
            IPendingExchangeService pendingExchangeService,
            CancellationToken cancellationToken,
            HttpContext context) =>
        {
            var userId = context.GetUserId();

            if (userId == null)
            {
                return TypedResults.Unauthorized();
            }
            
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
                    currency!.CurrencyId,
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
        }).RequireAuthorization().WithTags("Account");

        app.MapPatch("/account/{accountId:Guid}/deposit", async Task<Results<UnauthorizedHttpResult, BadRequest, Ok<GetAccountsResponse>>>(
            Guid accountId,
            [FromQuery] double amount,
            IAccountService accountService,
            ICurrencyService currencyService,
            IUserService userService,
            CancellationToken cancellationToken,
            HttpContext context) =>
        {
            var account = await accountService.Deposit(accountId, amount, cancellationToken);

            if (account == null)
            {
                return TypedResults.BadRequest();
            }
            
            var userId = context.GetUserId();

            if (userId == null || account.UserId != userId)
            {
                return TypedResults.Unauthorized();
            }

            var user = await userService.GetUser(account.UserId, cancellationToken);

            if (user == null)
            {
                return TypedResults.BadRequest();
            }
            
            var currency = await currencyService.GetCurrency(account.CurrencyId, cancellationToken);
            
            if (currency == null)
            {
                return TypedResults.BadRequest();
            }
            
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
        }).RequireAuthorization().WithTags("Account");

        app.MapPatch("/account/{accountId:Guid}/withdraw", async Task<Results<UnauthorizedHttpResult, BadRequest, Ok<GetAccountsResponse>>>(
            Guid accountId,
            [FromQuery] double amount,
            IAccountService accountService,
            ICurrencyService currencyService,
            IUserService userService,
            CancellationToken cancellationToken,
            HttpContext context) =>
        {
            var account = await accountService.Withdraw(accountId, amount, cancellationToken);

            if (account == null)
            {
                return TypedResults.BadRequest();
            }
            
            var userId = context.GetUserId();

            if (userId == null || account.UserId != userId)
            {
                return TypedResults.Unauthorized();
            }

            var user = await userService.GetUser(account.UserId, cancellationToken);

            if (user == null)
            {
                return TypedResults.BadRequest();
            }
            
            var currency = await currencyService.GetCurrency(account.CurrencyId, cancellationToken);
            
            if (currency == null)
            {
                return TypedResults.BadRequest();
            }
            
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
        }).RequireAuthorization().WithTags("Account");
        
        return app;
    }
}

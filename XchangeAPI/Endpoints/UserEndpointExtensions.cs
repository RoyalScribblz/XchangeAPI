using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Endpoints.Contracts;
using XchangeAPI.Extensions;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.UserService;

namespace XchangeAPI.Endpoints;

public static class UserEndpointExtensions
{
    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/user", async Task<Results<UnauthorizedHttpResult, BadRequest, Ok<GetUserResponse>>>(
            CancellationToken cancellationToken,
            IUserService userService,
            ICurrencyService currencyService,
            HttpContext context) =>
        {
            var userId = context.GetUserId();

            if (userId == null)
            {
                return TypedResults.Unauthorized();
            }
            
            var user = await userService.CreateUser(userId, cancellationToken);

            var currency = await currencyService.GetCurrency(user.LocalCurrencyId, cancellationToken);

            if (currency == null)
            {
                return TypedResults.BadRequest();
            }
            
            var response = new GetUserResponse
            {
                UserId = user.UserId,
                LocalCurrency = currency,
                IsFrozen = user.IsFrozen,
                IsBanned = user.IsBanned,
            };

            return TypedResults.Ok(response);
        }).RequireAuthorization().WithTags("User");

        app.MapGet("/user", async Task<Results<UnauthorizedHttpResult, NotFound, BadRequest, Ok<GetUserResponse>>>(
            IUserService userService,
            ICurrencyService currencyService,
            CancellationToken cancellationToken,
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
                return TypedResults.NotFound();
            }

            var currency = await currencyService.GetCurrency(user.LocalCurrencyId, cancellationToken);

            if (currency == null)
            {
                return TypedResults.BadRequest();
            }

            var response = new GetUserResponse
            {
                UserId = user.UserId,
                LocalCurrency = currency,
                IsFrozen = user.IsFrozen,
                IsBanned = user.IsBanned,
            };

            return TypedResults.Ok(response);
        }).RequireAuthorization().WithTags("User");

        app.MapPatch("/user/localCurrency", async Task<Results<UnauthorizedHttpResult, BadRequest, Ok<Currency>>>(
            [FromQuery] Guid currencyId,
            IUserService userService,
            CancellationToken cancellationToken,
            HttpContext context) =>
        {
            var userId = context.GetUserId();

            if (userId == null)
            {
                return TypedResults.Unauthorized();
            }
            
            var currency = await userService.UpdateLocalCurrency(userId, currencyId, cancellationToken);

            if (currency == null)
            {
                return TypedResults.BadRequest();
            }

            return TypedResults.Ok(currency);
        }).RequireAuthorization().WithTags("User");

        return app;
    }
}

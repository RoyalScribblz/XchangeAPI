using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Endpoints.Contracts;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.UserService;

namespace XchangeAPI.Endpoints;

public static class UserEndpointExtensions
{
    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
        app.MapPost("/user", async (
            string userId,
            CancellationToken cancellationToken,
            IUserService userService,
            ICurrencyService currencyService) =>
        {
            var user = await userService.CreateUser(userId, cancellationToken);

            var currency = await currencyService.GetCurrency(user.LocalCurrencyId, cancellationToken);

            var response = new GetUserResponse
            {
                UserId = user.UserId,
                LocalCurrency = currency,
                IsFrozen = user.IsFrozen,
                IsBanned = user.IsBanned
            };

            return TypedResults.Ok(response);
        }).WithTags("User");
        
        app.MapGet("/user/{userId}", async Task<Results<NotFound, Ok<GetUserResponse>>>(
            string userId,
            IUserService userService,
            ICurrencyService currencyService,
            CancellationToken cancellationToken) =>
        {
            var user = await userService.GetUser(userId, cancellationToken);

            if (user == null)
            {
                return TypedResults.NotFound();
            }

            var currency = await currencyService.GetCurrency(user.LocalCurrencyId, cancellationToken);

            var response = new GetUserResponse
            {
                UserId = user.UserId,
                LocalCurrency = currency,
                IsFrozen = user.IsFrozen,
                IsBanned = user.IsBanned
            };

            return TypedResults.Ok(response);
        }).WithTags("User");

        app.MapPatch("/user/{userId}/localCurrency", async Task<Results<BadRequest, Ok<Currency>>>(
            string userId,
            [FromQuery] Guid currencyId,
            IUserService userService,
            CancellationToken cancellationToken) =>
        {
            var currency = await userService.UpdateLocalCurrency(userId, currencyId, cancellationToken);

            if (currency == null)
            {
                return TypedResults.BadRequest();
            }

            return TypedResults.Ok(currency);
        }).WithTags("User");

        app.MapGet("/users", (IUserService userService) =>
            TypedResults.Ok(userService.GetUsers())).WithTags("User");
        
        return app;
    }
}
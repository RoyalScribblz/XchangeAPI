using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Endpoints.Contracts;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.UserService;

namespace XchangeAPI.Endpoints;

public static class UserEndpointExtensions
{
    public static WebApplication MapUserEndpoints(this WebApplication app)
    {
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
        });

        app.MapPut("/user/{userId}/localCurrency", (
            string userId,
            [FromQuery] Guid currencyId,
            IUserService userService,
            CancellationToken cancellationToken) =>
        {
            userService.UpdateLocalCurrency(userId, currencyId, cancellationToken);
        });
        
        return app;
    }
}
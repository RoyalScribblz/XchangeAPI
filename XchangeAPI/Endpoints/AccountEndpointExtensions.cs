using Microsoft.AspNetCore.Mvc;
using XchangeAPI.Services.AccountService;

namespace XchangeAPI.Endpoints;

public static class AccountEndpointExtensions
{
    public static WebApplication MapAccountEndpoints(this WebApplication app)
    {
        app.MapGet("/accounts", ([FromQuery] string userId, IAccountService accountService)
            => TypedResults.Ok(accountService.GetAccounts(userId)));

        return app;
    }
}
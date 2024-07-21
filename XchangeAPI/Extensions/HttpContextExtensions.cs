using System.Security.Claims;

namespace XchangeAPI.Extensions;

public static class HttpContextExtensions
{
    public static string? GetUserId(this HttpContext context)
    {
        return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
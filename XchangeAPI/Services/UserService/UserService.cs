using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database;

namespace XchangeAPI.Services.UserService;

public sealed class UserService(XchangeDatabase database) : IUserService
{
    public async Task<bool> IsFrozen(string userId, CancellationToken cancellationToken)
    {
        var user = await database.Users.SingleOrDefaultAsync(
            u => u.UserId == userId, cancellationToken);

        return user is { IsFrozen: true };
    }
}
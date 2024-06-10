using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.UserService;

public sealed class UserService(XchangeDatabase database) : IUserService
{
    public Task<User?> GetUser(string userId, CancellationToken cancellationToken)
    {
        return database.Users.SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);
    }
    
    public async Task<bool> IsFrozen(string userId, CancellationToken cancellationToken)
    {
        var user = await database.Users.SingleOrDefaultAsync(
            u => u.UserId == userId, cancellationToken);

        return user is { IsFrozen: true };
    }

    public async Task UpdateLocalCurrency(string userId, Guid currencyId, CancellationToken cancellationToken)
    {
        var user = await database.Users.SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            return;
        }

        user.LocalCurrencyId = currencyId;

        await database.SaveChangesAsync(cancellationToken);
    }
}
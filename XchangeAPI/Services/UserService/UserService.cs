using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Services.CurrencyService;

namespace XchangeAPI.Services.UserService;

public sealed class UserService(XchangeDatabase database, ICurrencyService currencyService) : IUserService
{
    public async Task<User> CreateUser(string userId, CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserId = userId,
            LocalCurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
            IsFrozen = false,
            IsBanned = false
        };

        await database.Users.AddAsync(user, cancellationToken);

        return user;
    }
    
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

    public async Task<Currency?> UpdateLocalCurrency(string userId, Guid currencyId, CancellationToken cancellationToken)
    {
        var user = await database.Users.SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        user.LocalCurrencyId = currencyId;

        await database.SaveChangesAsync(cancellationToken);

        return await currencyService.GetCurrency(currencyId, cancellationToken);
    }
}
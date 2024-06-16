using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Extensions;

public static class XchangeDatabaseExtensions
{
    public static async Task Seed(this XchangeDatabase database, IEnumerable<Currency> currencies)
    {
        await database.Currencies.AddRangeAsync(currencies);

        await database.Users.AddAsync(new User
        {
            UserId = "TestAccount",
            LocalCurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
            IsFrozen = false,
            IsBanned = false
        });

        await database.Accounts.AddRangeAsync([
            new Account
            {
                AccountId = Guid.NewGuid(),
                UserId = "TestAccount",
                CurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
                Balance = 200.00
            },
            new Account
            {
                AccountId = Guid.NewGuid(),
                UserId = "TestAccount",
                CurrencyId = Guid.Parse("6c84631c-838b-403e-8e2b-38614d2e907d"),
                Balance = 100.00
            }
        ]);
        
        // TODO dont seed my data
        await database.Users.AddAsync(new User
        {
            UserId = "auth0|65cf6dac8b7a7cb0b5f26732",
            LocalCurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
            IsFrozen = false,
            IsBanned = false
        });

        await database.Accounts.AddRangeAsync([
            new Account
            {
                AccountId = Guid.NewGuid(),
                UserId = "auth0|65cf6dac8b7a7cb0b5f26732",
                CurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
                Balance = 200.00
            },
            new Account
            {
                AccountId = Guid.NewGuid(),
                UserId = "auth0|65cf6dac8b7a7cb0b5f26732",
                CurrencyId = Guid.Parse("6c84631c-838b-403e-8e2b-38614d2e907d"),
                Balance = 100.00
            }
        ]);
        
        await database.SaveChangesAsync();
    }
}
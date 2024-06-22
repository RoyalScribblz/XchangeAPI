using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Services.CurrencyService;

namespace XchangeAPI.Services.AccountService;

public sealed class AccountService(ICurrencyService currencyService, XchangeDatabase database) : IAccountService
{
    public async Task<Account?> Create(string userId, Guid currencyId, CancellationToken cancellationToken)
    {
        if (await database.Accounts.AnyAsync(a => a.UserId == userId && a.CurrencyId == currencyId, cancellationToken))
        {
            return null;
        }

        var account = new Account
        {
            AccountId = Guid.NewGuid(),
            UserId = userId,
            CurrencyId = currencyId,
            Balance = 0
        };

        await database.Accounts.AddAsync(account, cancellationToken);

        await database.SaveChangesAsync(cancellationToken);

        return account;
    }

    public async Task<bool> Exchange(string userId, double amount, Guid fromCurrencyId, Guid toCurrencyId, CancellationToken cancellationToken)
    {
        var exchangeRate = await currencyService.GetExchangeRate(fromCurrencyId, toCurrencyId, cancellationToken);

        if (exchangeRate == null)
        {
            return false;
        }

        var fromAccount = await database.Accounts.SingleOrDefaultAsync(
            a => a.UserId == userId && a.CurrencyId == fromCurrencyId,
            cancellationToken);

        var toAccount = await database.Accounts.SingleOrDefaultAsync(
            a => a.UserId == userId && a.CurrencyId == toCurrencyId,
            cancellationToken);

        if (fromAccount == null || toAccount == null)
        {
            return false;
        }

        var toAmount = amount * (double)exchangeRate;

        fromAccount.Balance -= amount;
        toAccount.Balance += toAmount;

        await database.SaveChangesAsync(cancellationToken);
        await CheckTransactionLimit(userId, amount, fromCurrencyId, cancellationToken);

        return true;
    }

    private async Task CheckTransactionLimit(string userId, double amount, Guid currencyId, CancellationToken cancellationToken)
    {
        var currency = await database.Currencies.SingleOrDefaultAsync(c => c.CurrencyId == currencyId, cancellationToken);

        if (currency != null && amount >= currency.TransactionLimit)
        {
            await FreezeUser(userId, cancellationToken);
        }
    }

    private async Task FreezeUser(string userId, CancellationToken cancellationToken)
    {
        var user = await database.Users.SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);

        if (user == null)
        {
            return;
        }

        user.IsFrozen = true;

        await database.EvidenceRequests.AddAsync(
            new EvidenceRequest
            {
                EvidenceRequestId = Guid.NewGuid(),
                UserId = userId,
                Evidence = string.Empty,
                Status = EvidenceRequestStatus.Active,
            },
            cancellationToken);

        await database.SaveChangesAsync(cancellationToken);
    }

    public IList<Account> GetAccounts(string userId)
    {
        return database.Accounts.Where(a => a.UserId == userId).ToList();
    }

    public async Task<Account?> Deposit(Guid accountId, double amount, CancellationToken cancellationToken)
    {
        var account = await database.Accounts.SingleOrDefaultAsync(
            a => a.AccountId == accountId, cancellationToken);

        if (account == null)
        {
            return null;
        }

        account.Balance += amount;

        await database.SaveChangesAsync(cancellationToken);

        return account;
    }
    
    public async Task<Account?> Withdraw(Guid accountId, double amount, CancellationToken cancellationToken)
    {
        var account = await database.Accounts.SingleOrDefaultAsync(
            a => a.AccountId == accountId, cancellationToken);

        if (account == null)
        {
            return null;
        }

        account.Balance -= amount;

        await database.SaveChangesAsync(cancellationToken);

        return account;
    }
}

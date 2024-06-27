using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Services.PendingExchangeService.Models;

namespace XchangeAPI.Services.AccountService;

public sealed class AccountService(
    XchangeDatabase database) : IAccountService
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

    public async Task<bool> CompleteExchange(string userId, PendingExchange pendingExchange, CancellationToken cancellationToken)
    {
        var fromAccount = await database.Accounts.SingleOrDefaultAsync(
            a => a.UserId == userId && a.CurrencyId == pendingExchange.FromCurrencyId,
            cancellationToken);

        var toAccount = await database.Accounts.SingleOrDefaultAsync(
            a => a.UserId == userId && a.CurrencyId == pendingExchange.ToCurrencyId,
            cancellationToken);

        if (fromAccount == null)
        {
            return false;
        }

        toAccount ??= await Create(userId, pendingExchange.ToCurrencyId, cancellationToken);

        if (toAccount == null)
        {
            return false;
        }
        
        fromAccount.Balance -= pendingExchange.FromAmount;
        toAccount.Balance += pendingExchange.ToAmount;
        
        await database.SaveChangesAsync(cancellationToken);
        await CheckTransactionLimit(userId, pendingExchange.FromAmount, pendingExchange.FromCurrencyId, cancellationToken);

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

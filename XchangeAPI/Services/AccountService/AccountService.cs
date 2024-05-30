using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Services.CurrencyService;

namespace XchangeAPI.Services.AccountService;

public sealed class AccountService(ILogger<AccountService> logger, ICurrencyService currencyService, XchangeDatabase database) : IAccountService
{
    public async Task<bool> Exchange(string userId, double amount, Guid fromCurrencyId, Guid toCurrencyId, CancellationToken cancellationToken)
    {
        double? exchangeRate = await currencyService.GetExchangeRate(fromCurrencyId, toCurrencyId, cancellationToken);

        if (exchangeRate == null)
        {
            return false;
        }
        
        var fromAccount = await database.Accounts.SingleOrDefaultAsync(
            a => a.UserId == userId && a.CurrencyId == fromCurrencyId, cancellationToken);
        
        var toAccount = await database.Accounts.SingleOrDefaultAsync(
            a => a.UserId == userId && a.CurrencyId == toCurrencyId, cancellationToken);
        

        if (fromAccount == null || toAccount == null)
        {
            return false;
        }

        double toAmount = amount * (double)exchangeRate;
        
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

        await database.EvidenceRequests.AddAsync(new EvidenceRequest
        {
            EvidenceRequestId = Guid.NewGuid(),
            UserId = userId,
            Evidence = string.Empty,
            Status = EvidenceRequestStatus.Active
        }, cancellationToken);
        
        await database.SaveChangesAsync(cancellationToken);
    }

    public List<Account> GetAccounts(string userId)
    {
        return database.Accounts.Where(a => a.UserId == userId).ToList();
    }
}
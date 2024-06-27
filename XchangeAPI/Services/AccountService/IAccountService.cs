using XchangeAPI.Database.Dtos;
using XchangeAPI.Services.PendingExchangeService.Models;

namespace XchangeAPI.Services.AccountService;

public interface IAccountService
{
    Task<Account?> Create(string userId, Guid currencyId, CancellationToken cancellationToken);

    Task<bool> CompleteExchange(string userId, PendingExchange pendingExchange, CancellationToken cancellationToken);

    IList<Account> GetAccounts(string userId);

    Task<Account?> Deposit(Guid accountId, double amount, CancellationToken cancellationToken);
    
    Task<Account?> Withdraw(Guid accountId, double amount, CancellationToken cancellationToken);

}

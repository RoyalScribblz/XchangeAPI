using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.AccountService;

public interface IAccountService
{
    Task<Account?> Create(string userId, Guid currencyId, CancellationToken cancellationToken);
    
    Task<bool> Exchange(
        string userId,
        double amount,
        Guid fromCurrencyId,
        Guid toCurrencyId,
        CancellationToken cancellationToken);

    IList<Account> GetAccounts(string userId);

    Task<Account?> Deposit(Guid accountId, double amount, CancellationToken cancellationToken);
    
    Task<Account?> Withdraw(Guid accountId, double amount, CancellationToken cancellationToken);

}

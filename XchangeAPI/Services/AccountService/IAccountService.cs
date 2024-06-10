using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.AccountService;

public interface IAccountService
{
    Task<bool> Exchange(
        string userId,
        double amount,
        Guid fromCurrencyId,
        Guid toCurrencyId,
        CancellationToken cancellationToken);
    
    List<Account> GetAccounts(string userId);
}
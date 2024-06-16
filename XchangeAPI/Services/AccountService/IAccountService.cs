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

    IList<Account> GetAccounts(string userId);
}

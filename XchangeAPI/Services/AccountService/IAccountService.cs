using XchangeAPI.Endpoints.Contracts;

namespace XchangeAPI.Services.AccountService;

public interface IAccountService
{
    Task<bool> Exchange(
        string userId,
        double amount,
        Guid fromCurrencyId,
        Guid toCurrencyId,
        CancellationToken cancellationToken);

    Task<List<GetAccountsResponse>> GetAccounts(
        string userId, 
        Guid localCurrencyId,
        CancellationToken cancellationToken);
}
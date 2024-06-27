using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.UserService;

public interface IUserService
{
    Task<User> CreateUser(string userId, CancellationToken cancellationToken);
    Task<User?> GetUser(string userId, CancellationToken cancellationToken);
    Task<bool> IsFrozen(string userId, CancellationToken cancellationToken);
    Task<Currency?> UpdateLocalCurrency(string userId, Guid currencyId, CancellationToken cancellationToken);
    IList<User> GetUsers();
    Task<Guid> GetLocalCurrencyId(string userId, CancellationToken cancellationToken);
}

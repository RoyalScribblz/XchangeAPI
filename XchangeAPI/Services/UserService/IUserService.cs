using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Services.UserService;

public interface IUserService
{
    Task<User?> GetUser(string userId, CancellationToken cancellationToken);
    Task<bool> IsFrozen(string userId, CancellationToken cancellationToken);
    Task UpdateLocalCurrency(string userId, Guid currencyId, CancellationToken cancellationToken);
}
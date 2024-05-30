namespace XchangeAPI.Services.UserService;

public interface IUserService
{
    Task<bool> IsFrozen(string userId, CancellationToken cancellationToken);
}
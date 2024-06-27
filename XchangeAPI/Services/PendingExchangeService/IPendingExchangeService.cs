using XchangeAPI.Services.PendingExchangeService.Models;

namespace XchangeAPI.Services.PendingExchangeService;

public interface IPendingExchangeService
{
    PendingExchange? Create(string userId, Guid fromCurrencyId, double fromAmount, Guid toCurrencyId, double toAmount);

    PendingExchange? Get(string userId, Guid pendingExchangeId);

    void Remove(string userId);
}
using System.Collections.Concurrent;
using XchangeAPI.Services.PendingExchangeService.Models;

namespace XchangeAPI.Services.PendingExchangeService;

public sealed class PendingExchangeService : IPendingExchangeService
{
    private readonly ConcurrentDictionary<string, PendingExchange> _pendingExchanges = new();

    public PendingExchange Create(
        string userId,
        Guid fromCurrencyId,
        double fromAmount,
        Guid toCurrencyId,
        double toAmount)
    {
        var pendingExchange = new PendingExchange
        {
            PendingExchangeId = Guid.NewGuid(),
            FromCurrencyId = fromCurrencyId,
            FromAmount = fromAmount,
            ToCurrencyId = toCurrencyId,
            ToAmount = toAmount,
        };

        _pendingExchanges[userId] = pendingExchange;
        
        _ = Task.Run(async () =>
        {
            await Task.Delay(30000);
            _pendingExchanges.Remove(userId, out _);
        });

        return pendingExchange;
    }

    public PendingExchange? Get(string userId, Guid pendingExchangeId)
    {
        if (!_pendingExchanges.TryGetValue(userId, out var pendingExchange)
            || pendingExchange.PendingExchangeId != pendingExchangeId)
        {
            return null;
        }

        return pendingExchange;
    }

    public void Remove(string userId)
    {
        _pendingExchanges.TryRemove(userId, out _);
    }
}

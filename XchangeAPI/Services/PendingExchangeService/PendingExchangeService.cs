using System.Collections.Concurrent;

namespace XchangeAPI.Services.PendingExchangeService;

public sealed class PendingExchangeService : IPendingExchangeService
{
    private readonly ConcurrentDictionary<string, PendingExchange> pendingExchanges = new();

    public async Task Test(string userId)
    {
        pendingExchanges.AddOrUpdate()
            
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            pendingExchanges.Remove(userId);
        });
    }
}

sealed record PendingExchange()
{
    public Guid PendingExchangeId { get; init; }
}
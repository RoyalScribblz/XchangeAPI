using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Database;

public sealed class XchangeDatabase(DbContextOptions<XchangeDatabase> options) : DbContext(options)
{
    public required DbSet<User> Users { get; init; }

    public required DbSet<Account> Accounts { get; init; }

    public required DbSet<Currency> Currencies { get; init; }

    public required DbSet<EvidenceRequest> EvidenceRequests { get; init; }
}

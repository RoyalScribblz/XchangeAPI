using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Database;

public sealed class XchangeDatabase(DbContextOptions<XchangeDatabase> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }

    public DbSet<Account> Accounts { get; init; }

    public DbSet<Currency> Currencies { get; init; }

    public DbSet<EvidenceRequest> EvidenceRequests { get; init; }
}

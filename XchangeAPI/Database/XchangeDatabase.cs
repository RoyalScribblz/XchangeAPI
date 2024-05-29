using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database.Dtos;

namespace XchangeAPI.Database;

public sealed class XchangeDatabase : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseInMemoryDatabase("XchangeDatabase");
    }
    
    public required DbSet<User> Users { get; init; }
    
    public required DbSet<Account> Accounts { get; init; }
    
    public required DbSet<Currency> Currencies { get; init; }
    
    public required DbSet<EvidenceRequest> EvidenceRequests { get; init; }
}
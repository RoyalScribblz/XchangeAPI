using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database;
using XchangeAPI.Services.AccountService;
using XchangeAPI.Services.PendingExchangeService.Models;

namespace XchangeAPI.UnitTests;

public class AccountServiceTests
{
    private XchangeDatabase _database = null!;
    private AccountService _accountService = null!;

    [SetUp]
    public void Setup()
    {
        DbContextOptionsBuilder<XchangeDatabase> builder = new DbContextOptionsBuilder<XchangeDatabase>();
        builder.UseInMemoryDatabase("UnitTestDb");
        _database = new XchangeDatabase(builder.Options);
        _accountService = new AccountService(_database);
    }

    [TearDown]
    public void TearDown()
    {
        _database.Database.EnsureDeleted();
        _database.Dispose();
    }

    [Test]
    public async Task Create_ShouldReturnAccount_WhenAccountDoesNotExist()
    {
        // Arrange
        var userId = "user1";
        var currencyId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _accountService.Create(userId, currencyId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.CurrencyId.Should().Be(currencyId);
        result.Balance.Should().Be(0);
    }

    [Test]
    public async Task Create_ShouldReturnNull_WhenAccountAlreadyExists()
    {
        // Arrange
        var userId = "user1";
        var currencyId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        await _accountService.Create(userId, currencyId, cancellationToken);

        // Act
        var result = await _accountService.Create(userId, currencyId, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task CompleteExchange_ShouldReturnTrue_WhenExchangeIsSuccessful()
    {
        // Arrange
        var userId = "user1";
        var fromCurrencyId = Guid.NewGuid();
        var toCurrencyId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var fromAccount = await _accountService.Create(userId, fromCurrencyId, cancellationToken);
        fromAccount!.Balance = 100;
        await _database.SaveChangesAsync(cancellationToken);

        var pendingExchange = new PendingExchange
        {
            PendingExchangeId = Guid.NewGuid(),
            FromCurrencyId = fromCurrencyId,
            ToCurrencyId = toCurrencyId,
            FromAmount = 50,
            ToAmount = 50,
        };

        // Act
        var result = await _accountService.CompleteExchange(userId, pendingExchange, cancellationToken);

        // Assert
        result.Should().BeTrue();
        fromAccount.Balance.Should().Be(50);
        var toAccount = _database.Accounts.Single(a => a.UserId == userId && a.CurrencyId == toCurrencyId);
        toAccount.Balance.Should().Be(50);
    }

    [Test]
    public async Task CompleteExchange_ShouldReturnFalse_WhenFromAccountDoesNotExist()
    {
        // Arrange
        var userId = "user1";
        var pendingExchange = new PendingExchange
        {
            PendingExchangeId = Guid.NewGuid(),
            FromCurrencyId = Guid.NewGuid(),
            ToCurrencyId = Guid.NewGuid(),
            FromAmount = 50,
            ToAmount = 50,
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _accountService.CompleteExchange(userId, pendingExchange, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task Deposit_ShouldIncreaseBalance_WhenAccountExists()
    {
        // Arrange
        var userId = "user1";
        var currencyId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var account = await _accountService.Create(userId, currencyId, cancellationToken);
        var amountToDeposit = 100;

        // Act
        var result = await _accountService.Deposit(account!.AccountId, amountToDeposit, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Balance.Should().Be(amountToDeposit);
    }

    [Test]
    public async Task Withdraw_ShouldDecreaseBalance_WhenAccountExists()
    {
        // Arrange
        var userId = "user1";
        var currencyId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        var account = await _accountService.Create(userId, currencyId, cancellationToken);
        account!.Balance = 100;
        await _database.SaveChangesAsync(cancellationToken);

        var amountToWithdraw = 50;

        // Act
        var result = await _accountService.Withdraw(account.AccountId, amountToWithdraw, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Balance.Should().Be(50);
    }

    [Test]
    public async Task Withdraw_ShouldReturnNull_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var amountToWithdraw = 50;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _accountService.Withdraw(accountId, amountToWithdraw, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetAccounts_ShouldReturnAllAccountsForUser()
    {
        // Arrange
        var userId = "user1";
        var currencyId1 = Guid.NewGuid();
        var currencyId2 = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        await _accountService.Create(userId, currencyId1, cancellationToken);
        await _accountService.Create(userId, currencyId2, cancellationToken);

        // Act
        var accounts = _accountService.GetAccounts(userId);

        // Assert
        accounts.Should().HaveCount(2);
        accounts.All(a => a.UserId == userId).Should().BeTrue();
    }
}
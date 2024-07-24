using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Services.CurrencyService;
using XchangeAPI.Services.UserService;

namespace XchangeAPI.UnitTests;

public class UserServiceTests
{
    private XchangeDatabase _database = null!;
    private ICurrencyService _currencyService = null!;
    private UserService _userService = null!;

    [SetUp]
    public void Setup()
    {
        var builder = new DbContextOptionsBuilder<XchangeDatabase>();
        builder.UseInMemoryDatabase("UnitTestDb");
        _database = new XchangeDatabase(builder.Options);

        _currencyService = Substitute.For<ICurrencyService>();

        _userService = new UserService(_database, _currencyService);
    }

    [TearDown]
    public void TearDown()
    {
        _database.Dispose();
    }

    [Test]
    public async Task CreateUser_ShouldReturnNewUser_WhenCalled()
    {
        // Arrange
        var userId = "user1";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _userService.CreateUser(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.LocalCurrencyId.Should().Be(Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"));
        result.IsFrozen.Should().BeFalse();
        result.IsBanned.Should().BeFalse();
    }

    [Test]
    public async Task GetUser_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = "user1";
        var user = new User
        {
            UserId = userId,
            LocalCurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
            IsFrozen = false,
            IsBanned = false,
        };
        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _userService.GetUser(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Test]
    public async Task GetUser_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonexistent_user";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _userService.GetUser(userId, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task IsFrozen_ShouldReturnTrue_WhenUserIsFrozen()
    {
        // Arrange
        var userId = "user1";
        var user = new User
        {
            UserId = userId,
            LocalCurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
            IsFrozen = true,
            IsBanned = false,
        };
        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _userService.IsFrozen(userId, cancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task IsFrozen_ShouldReturnFalse_WhenUserIsNotFrozen()
    {
        // Arrange
        var userId = "user1";
        var user = new User
        {
            UserId = userId,
            LocalCurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
            IsFrozen = false,
            IsBanned = false,
        };
        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _userService.IsFrozen(userId, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task UpdateLocalCurrency_ShouldReturnUpdatedCurrency_WhenUserExists()
    {
        // Arrange
        const string userId = "user1";
        var newCurrencyId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            LocalCurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
            IsFrozen = false,
            IsBanned = false,
        };
        var currency = new Currency
        {
            CurrencyId = newCurrencyId,
            Name = string.Empty,
            CurrencyCode = "EUR",
            FlagImageUrl = string.Empty,
            Symbol = string.Empty,
            UsdValue = 0,
            TransactionLimit = 0,
        };
        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        _currencyService.GetCurrency(newCurrencyId, Arg.Any<CancellationToken>()).Returns(currency);

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _userService.UpdateLocalCurrency(userId, newCurrencyId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.CurrencyCode.Should().Be("EUR");
        user.LocalCurrencyId.Should().Be(newCurrencyId);
    }

    [Test]
    public async Task UpdateLocalCurrency_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonexistent_user";
        var newCurrencyId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _userService.UpdateLocalCurrency(userId, newCurrencyId, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetUsers_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = new User
        {
            UserId = "user1",
            LocalCurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
            IsFrozen = false,
            IsBanned = false,
        };
        var user2 = new User
        {
            UserId = "user2",
            LocalCurrencyId = Guid.Parse("3ca59b04-be8a-4344-90d2-5d78c5009da6"),
            IsFrozen = false,
            IsBanned = false,
        };
        _database.Users.Add(user1);
        _database.Users.Add(user2);
        _database.SaveChanges();

        // Act
        var users = _userService.GetUsers();

        // Assert
        users.Should().HaveCount(2);
        users.Should().Contain(u => u.UserId == "user1");
        users.Should().Contain(u => u.UserId == "user2");
    }

    [Test]
    public async Task GetLocalCurrencyId_ShouldReturnCurrencyId_WhenUserExists()
    {
        // Arrange
        var userId = "user1";
        var localCurrencyId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            LocalCurrencyId = localCurrencyId,
            IsFrozen = false,
            IsBanned = false,
        };
        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _userService.GetLocalCurrencyId(userId, cancellationToken);

        // Assert
        result.Should().Be(localCurrencyId);
    }

    [Test]
    public async Task GetLocalCurrencyId_ShouldReturnEmptyGuid_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = "nonexistent_user";
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _userService.GetLocalCurrencyId(userId, cancellationToken);

        // Assert
        result.Should().Be(Guid.Empty);
    }
}

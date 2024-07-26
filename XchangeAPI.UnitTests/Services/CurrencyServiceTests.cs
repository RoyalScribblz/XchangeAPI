using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Options;
using XchangeAPI.Services.CurrencyService;

namespace XchangeAPI.UnitTests.Services;

public class CurrencyServiceTests
{
    private XchangeDatabase _database;
    private CurrencyService _currencyService;

    [SetUp]
    public void Setup()
    {
        var builder = new DbContextOptionsBuilder<XchangeDatabase>();
        builder.UseInMemoryDatabase("UnitTestDb");
        _database = new XchangeDatabase(builder.Options);

        IServiceCollection services = new ServiceCollection();
        services.AddHttpClient<CurrencyService>();
        var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        
        _currencyService = new CurrencyService(new OptionsWrapper<OpenExchangeRatesOptions>(new OpenExchangeRatesOptions
        {
            ApiKey = "7c47009a92194d19aab4fa73eb8d4af6"
        }), _database, factory);
    }

    [TearDown]
    public void TearDown()
    {
        _database.Database.EnsureDeleted();
        _database.Dispose();
    }

    [Test]
    public async Task GetExchangeRate_ShouldReturnCorrectRate_WhenCurrenciesExist()
    {
        // Arrange
        var fromCurrency = new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Name = string.Empty,
            CurrencyCode = "USD",
            FlagImageUrl = string.Empty,
            Symbol = string.Empty,
            UsdValue = 1,
            TransactionLimit = 0,
        };
        var toCurrency = new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Name = string.Empty,
            CurrencyCode = "EUR",
            FlagImageUrl = string.Empty,
            Symbol = string.Empty,
            UsdValue = 0.85,
            TransactionLimit = 0,
        };
        _database.Currencies.Add(fromCurrency);
        _database.Currencies.Add(toCurrency);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _currencyService.GetExchangeRate(fromCurrency.CurrencyId, toCurrency.CurrencyId, cancellationToken);

        // Assert
        result.Should().BeApproximately(0.85, 0.0001);
    }

    [Test]
    public async Task GetExchangeRate_ShouldReturnNull_WhenFromCurrencyDoesNotExist()
    {
        // Arrange
        var toCurrency = new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Name = string.Empty,
            CurrencyCode = "EUR",
            FlagImageUrl = string.Empty,
            Symbol = string.Empty,
            UsdValue = 0.85,
            TransactionLimit = 0,
        };
        _database.Currencies.Add(toCurrency);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _currencyService.GetExchangeRate(Guid.NewGuid(), toCurrency.CurrencyId, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetExchangeRate_ShouldReturnNull_WhenToCurrencyDoesNotExist()
    {
        // Arrange
        var fromCurrency = new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Name = string.Empty,
            CurrencyCode = "USD",
            FlagImageUrl = string.Empty,
            Symbol = string.Empty,
            UsdValue = 1,
            TransactionLimit = 0,
        };
        _database.Currencies.Add(fromCurrency);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _currencyService.GetExchangeRate(fromCurrency.CurrencyId, Guid.NewGuid(), cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateTransactionLimit_ShouldUpdateLimit_WhenCurrencyExists()
    {
        // Arrange
        var currency = new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Name = string.Empty,
            CurrencyCode = "USD",
            FlagImageUrl = string.Empty,
            Symbol = string.Empty,
            UsdValue = 0,
            TransactionLimit = 1000,
        };
        _database.Currencies.Add(currency);
        await _database.SaveChangesAsync();

        const double newLimit = 2000;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _currencyService.UpdateTransactionLimit(currency.CurrencyId, newLimit, cancellationToken);

        // Assert
        result.Should().BeTrue();
        currency.TransactionLimit.Should().Be(newLimit);
    }

    [Test]
    public async Task UpdateTransactionLimit_ShouldReturnFalse_WhenCurrencyDoesNotExist()
    {
        // Arrange
        const double newLimit = 2000;
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _currencyService.UpdateTransactionLimit(Guid.NewGuid(), newLimit, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task GetCurrency_ShouldReturnCurrency_WhenCurrencyExists()
    {
        // Arrange
        var currency = new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Name = string.Empty,
            CurrencyCode = "USD",
            FlagImageUrl = string.Empty,
            Symbol = string.Empty,
            UsdValue = 0,
            TransactionLimit = 0,
        };
        _database.Currencies.Add(currency);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _currencyService.GetCurrency(currency.CurrencyId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.CurrencyCode.Should().Be("USD");
    }

    [Test]
    public async Task GetCurrency_ShouldReturnNull_WhenCurrencyDoesNotExist()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _currencyService.GetCurrency(Guid.NewGuid(), cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void GetCurrencies_ShouldReturnAllCurrencies()
    {
        // Arrange
        var currency1 = new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Name = string.Empty,
            CurrencyCode = "USD",
            FlagImageUrl = string.Empty,
            Symbol = string.Empty,
            UsdValue = 0,
            TransactionLimit = 0,
        };
        var currency2 = new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Name = string.Empty,
            CurrencyCode = "EUR",
            FlagImageUrl = string.Empty,
            Symbol = string.Empty,
            UsdValue = 0,
            TransactionLimit = 0,
        };
        _database.Currencies.Add(currency1);
        _database.Currencies.Add(currency2);
        _database.SaveChanges();

        // Act
        var currencies = _currencyService.GetCurrencies();

        // Assert
        currencies.Should().HaveCount(2);
        currencies.Should().Contain(c => c.CurrencyCode == "USD");
        currencies.Should().Contain(c => c.CurrencyCode == "EUR");
    }
}

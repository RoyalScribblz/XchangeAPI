using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XchangeAPI.Database;
using XchangeAPI.Database.Dtos;
using XchangeAPI.Services.EvidenceRequestService;

namespace XchangeAPI.UnitTests.Services;

public class EvidenceRequestServiceTests
{
    private XchangeDatabase _database;
    private EvidenceRequestService _evidenceRequestService;

    [SetUp]
    public void Setup()
    {
        var builder = new DbContextOptionsBuilder<XchangeDatabase>();
        builder.UseInMemoryDatabase("UnitTestDb");
        _database = new XchangeDatabase(builder.Options);

        _evidenceRequestService = new EvidenceRequestService(_database);
    }

    [TearDown]
    public void TearDown()
    {
        _database.Database.EnsureDeleted();
        _database.Dispose();
    }

    [Test]
    public void GetEvidenceRequests_ShouldReturnAllEvidenceRequests()
    {
        // Arrange
        var evidenceRequest1 = new EvidenceRequest
        {
            EvidenceRequestId = Guid.NewGuid(),
            UserId = "user1",
            EvidenceIds = [],
            Status = EvidenceRequestStatus.Waiting,
            CurrencyId = Guid.NewGuid(),
            Amount = 100,
        };
        var evidenceRequest2 = new EvidenceRequest
        {
            EvidenceRequestId = Guid.NewGuid(),
            UserId = "user2",
            EvidenceIds = [],
            Status = EvidenceRequestStatus.Active,
            CurrencyId = Guid.NewGuid(),
            Amount = 200,
        };
        _database.EvidenceRequests.Add(evidenceRequest1);
        _database.EvidenceRequests.Add(evidenceRequest2);
        _database.SaveChanges();

        // Act
        var evidenceRequests = _evidenceRequestService.GetEvidenceRequests();

        // Assert
        evidenceRequests.Should().HaveCount(2);
        evidenceRequests.Should().Contain(er => er.UserId == "user1");
        evidenceRequests.Should().Contain(er => er.UserId == "user2");
    }

    [Test]
    public async Task GetEvidenceRequestByUserId_ShouldReturnCorrectRequest()
    {
        // Arrange
        const string userId = "user1";
        var evidenceRequest = new EvidenceRequest
        {
            EvidenceRequestId = Guid.NewGuid(),
            UserId = userId,
            EvidenceIds = [],
            Status = EvidenceRequestStatus.Active,
            CurrencyId = Guid.NewGuid(),
            Amount = 100,
        };
        _database.EvidenceRequests.Add(evidenceRequest);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _evidenceRequestService.GetEvidenceRequest(userId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Test]
    public async Task GetEvidenceRequestById_ShouldReturnCorrectRequest()
    {
        // Arrange
        var evidenceRequestId = Guid.NewGuid();
        var evidenceRequest = new EvidenceRequest
        {
            EvidenceRequestId = evidenceRequestId,
            UserId = "user1",
            EvidenceIds = [],
            Status = EvidenceRequestStatus.Waiting,
            CurrencyId = Guid.NewGuid(),
            Amount = 100,
        };
        _database.EvidenceRequests.Add(evidenceRequest);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _evidenceRequestService.GetEvidenceRequest(evidenceRequestId, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.EvidenceRequestId.Should().Be(evidenceRequestId);
    }

    [Test]
    public async Task SubmitEvidence_ShouldAddEvidenceIdToRequest()
    {
        // Arrange
        var evidenceRequestId = Guid.NewGuid();
        var evidenceId = Guid.NewGuid();
        var evidenceRequest = new EvidenceRequest
        {
            EvidenceRequestId = evidenceRequestId,
            UserId = "user1",
            EvidenceIds = [],
            Status = EvidenceRequestStatus.Waiting,
            CurrencyId = Guid.NewGuid(),
            Amount = 100,
        };
        _database.EvidenceRequests.Add(evidenceRequest);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        await _evidenceRequestService.SubmitEvidence(evidenceRequestId, evidenceId, cancellationToken);

        // Assert
        evidenceRequest.EvidenceIds.Should().Contain(evidenceId);
    }

    [Test]
    public async Task AcceptEvidence_ShouldChangeStatusAndUnfreezeUser()
    {
        // Arrange
        var evidenceRequestId = Guid.NewGuid();
        const string userId = "user1";
        var evidenceRequest = new EvidenceRequest
        {
            EvidenceRequestId = evidenceRequestId,
            UserId = userId,
            EvidenceIds = [],
            Status = EvidenceRequestStatus.Waiting,
            CurrencyId = Guid.NewGuid(),
            Amount = 100,
        };
        var user = new User
        {
            UserId = userId,
            LocalCurrencyId = Guid.NewGuid(),
            IsFrozen = true,
            IsBanned = false,
        };
        _database.EvidenceRequests.Add(evidenceRequest);
        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        await _evidenceRequestService.AcceptEvidence(evidenceRequestId, cancellationToken);

        // Assert
        evidenceRequest.Status.Should().Be(EvidenceRequestStatus.Accepted);
        user.IsFrozen.Should().BeFalse();
    }

    [Test]
    public async Task RejectEvidence_ShouldChangeStatusAndBanUser()
    {
        // Arrange
        var evidenceRequestId = Guid.NewGuid();
        const string userId = "user1";
        var evidenceRequest = new EvidenceRequest
        {
            EvidenceRequestId = evidenceRequestId,
            UserId = userId,
            EvidenceIds = [],
            Status = EvidenceRequestStatus.Waiting,
            CurrencyId = Guid.NewGuid(),
            Amount = 100,
        };
        var user = new User
        {
            UserId = userId,
            LocalCurrencyId = Guid.NewGuid(),
            IsFrozen = false,
            IsBanned = false,
        };
        _database.EvidenceRequests.Add(evidenceRequest);
        _database.Users.Add(user);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        await _evidenceRequestService.RejectEvidence(evidenceRequestId, cancellationToken);

        // Assert
        evidenceRequest.Status.Should().Be(EvidenceRequestStatus.Rejected);
        user.IsBanned.Should().BeTrue();
    }

    [Test]
    public async Task SetActive_ShouldChangeStatusToActive()
    {
        // Arrange
        var evidenceRequestId = Guid.NewGuid();
        var evidenceRequest = new EvidenceRequest
        {
            EvidenceRequestId = evidenceRequestId,
            UserId = "user1",
            EvidenceIds = [],
            Status = EvidenceRequestStatus.Waiting,
            CurrencyId = Guid.NewGuid(),
            Amount = 100,
        };
        _database.EvidenceRequests.Add(evidenceRequest);
        await _database.SaveChangesAsync();

        var cancellationToken = CancellationToken.None;

        // Act
        await _evidenceRequestService.SetActive(evidenceRequestId, cancellationToken);

        // Assert
        evidenceRequest.Status.Should().Be(EvidenceRequestStatus.Active);
    }
}

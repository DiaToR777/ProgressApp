using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProgressApp.Domain.Models.Journal;
using ProgressApp.Infrastructure.Data;
using ProgressApp.Infrastructure.Repositories;

namespace ProgressAppTest.RepositoryTests;

[TestClass]
public class JournalRepositoryTests
{
    private Mock<IServiceScopeFactory> _scopeFactoryMock;
    private Mock<IDbState> _dbStateMock;
    private ProgressDbContext _dbContext;
    private SqliteConnection _connection;    
    private JournalRepository _repository;
    
    [TestInitialize]
    public void Setup()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ProgressDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbStateMock = new Mock<IDbState>();

        _dbStateMock.Setup(x => x.GetConnectionString(It.IsAny<string>())).Returns("DataSource=:memory:");

        _dbContext = new ProgressDbContext(options, _dbStateMock.Object);

        _dbContext.Database.EnsureCreated();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(x => x.GetService(typeof(ProgressDbContext))).Returns(_dbContext);

        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(x => x.ServiceProvider).Returns(serviceProviderMock.Object);

        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);
        _repository = new JournalRepository(_scopeFactoryMock.Object);
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
        _connection.Close();
    }
    
    [TestMethod]
    public async Task GetTodayAsync_WhenNoEntryExists_ReturnsNull()
    {
        var result = await _repository.GetTodayAsync();

        result.Should().BeNull();
    }
    
    [TestMethod]
    public async Task SaveTodayAsync_NewEntry_SavesCorrectData()
    {
        var description = "Test Achievement";
        var resultType = DayResult.Success;

        await _repository.SaveTodayAsync(description, resultType);

        var entry = await _dbContext.Entries.FirstOrDefaultAsync();
        entry.Should().NotBeNull();
        entry!.Description.Should().Be(description);
        entry.Result.Should().Be(resultType);
        entry.Date.Date.Should().Be(DateTime.Today);
    }
    
    [TestMethod]
    public async Task SaveTodayAsync_ExistingEntry_UpdatesInsteadOfCreating()
    {
        await _repository.SaveTodayAsync("First Version", DayResult.PartialSuccess);

        await _repository.SaveTodayAsync("Updated Version", DayResult.Success);

        var entries = await _dbContext.Entries.ToListAsync();
        entries.Should().HaveCount(1);
        entries[0].Description.Should().Be("Updated Version");
        entries[0].Result.Should().Be(DayResult.Success);
    }
    
    [TestMethod]
    public async Task GetAllEntriesAsync_ShouldReturnSortedByDateDescending()
    {
        var oldDate = DateTime.Today.AddDays(-5);
        var newDate = DateTime.Today;

        _dbContext.Entries.AddRange(
            new JournalEntry { Date = oldDate, Description = "Old", Result = DayResult.Success },
            new JournalEntry { Date = newDate, Description = "New", Result = DayResult.Success }
        );
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetAllEntriesAsync();

        result.Should().HaveCount(2);
        result[0].Date.Should().Be(newDate);
        result[1].Date.Should().Be(oldDate);
    }
}

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ProgressApp.Domain.Models.Settings;
using ProgressApp.Infrastructure.Data;
using ProgressApp.Infrastructure.Repositories;

namespace ProgressAppTest.RepositoryTests;

[TestClass]
public class SettingsRepositoryTests
{
    private Mock<IServiceScopeFactory> _scopeFactoryMock = null!;
    private Mock<IDbState> _dbStateMock = null!;
    private ProgressDbContext _dbContext = null!;
    private SqliteConnection _connection = null!;
    private SettingsRepository _repository = null!;

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
        _repository = new SettingsRepository(_scopeFactoryMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
        _connection.Close();
    }

    [TestMethod]
    public async Task GetGoalAsync_WhenNoGoal_ReturnsEmptyString()
    {
        var result = await _repository.GetGoalAsync();

        result.Should().Be("");
    }

    [TestMethod]
    public async Task SaveGoalAsync_ShouldUpdateExistingSetting_WhenAlreadyExists()
    {
        await _repository.SaveGoalAsync("Old Goal");

        await _repository.SaveGoalAsync("New Cool Goal");

        var allSettings = await _dbContext.Settings.ToListAsync();
        allSettings.Should().HaveCount(1);
        allSettings[0].Value.Should().Be("New Cool Goal");
    }

    [TestMethod]
    public async Task GetGoalAsync_WhenGoalExists_ReturnsGoal()
    {
        var expectedGoal = "Стать Senior .NET разработчиком";
        _dbContext.Settings.Add(new AppSettings { Key = SettingsKeys.Goal, Value = expectedGoal });
        await _dbContext.SaveChangesAsync();

        var result = await _repository.GetGoalAsync();

        result.Should().Be(expectedGoal);
    }

    [TestMethod]
    public async Task SaveGoalAsync_WhenNoSettingExists_InsertsNewSetting()
    {
        var goalText = "test";

        await _repository.SaveGoalAsync(goalText);

        var setting = await _dbContext.Settings.FirstOrDefaultAsync(s => s.Key == SettingsKeys.Goal);
        setting.Should().NotBeNull();
        setting!.Value.Should().Be(goalText);
    }
}